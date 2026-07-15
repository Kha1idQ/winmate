using System.Diagnostics;
using System.IO;
using System.Text;

namespace WinMate.Services;

public static class ProcessRunner
{
    // Runs an external command hidden, streaming every output line to onLine
    // as it happens. Returns the process exit code.
    public static async Task<int> RunAsync(string exe, string args, Action<string>? onLine = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ResolveExe(exe),
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            // WinMate runs elevated, and CreateProcess searches the current
            // directory before System32. Pinning the working directory to
            // System32 stops a winget.exe/cmd.exe planted next to a portable copy
            // of the app (e.g. in Downloads) from being run with admin rights.
            WorkingDirectory = Environment.SystemDirectory,
        };

        using var process = new Process { StartInfo = psi };

        // These callbacks arrive on a background thread — anyone updating UI
        // from onLine must marshal through the Dispatcher (LogService does).
        process.OutputDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) onLine?.Invoke(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) onLine?.Invoke(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    // Known Windows tools are launched by absolute System32 path so a same-named
    // binary planted in the app directory or CWD can't be run instead. Anything
    // already rooted is passed through; a bare name we don't recognise (e.g.
    // "winget", whose real location is an execution alias) keeps its name and is
    // protected by the fixed WorkingDirectory set above.
    private static string ResolveExe(string exe)
    {
        if (Path.IsPathRooted(exe))
            return exe;

        var sys = Environment.SystemDirectory;
        return exe.ToLowerInvariant() switch
        {
            "cmd" or "cmd.exe" => Path.Combine(sys, "cmd.exe"),
            "sc" or "sc.exe" => Path.Combine(sys, "sc.exe"),
            "schtasks" or "schtasks.exe" => Path.Combine(sys, "schtasks.exe"),
            "reg" or "reg.exe" => Path.Combine(sys, "reg.exe"),
            "powershell" or "powershell.exe" => Path.Combine(sys, "WindowsPowerShell", "v1.0", "powershell.exe"),
            _ => exe,
        };
    }

    // Convenience overload that captures all output as one string.
    public static async Task<(int ExitCode, string Output)> RunCapturedAsync(string exe, string args)
    {
        var sb = new StringBuilder();
        var code = await RunAsync(exe, args, line => { lock (sb) sb.AppendLine(line); });
        return (code, sb.ToString());
    }
}
