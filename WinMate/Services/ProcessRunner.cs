using System.Diagnostics;
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
            FileName = exe,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
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

    // Convenience overload that captures all output as one string.
    public static async Task<(int ExitCode, string Output)> RunCapturedAsync(string exe, string args)
    {
        var sb = new StringBuilder();
        var code = await RunAsync(exe, args, line => { lock (sb) sb.AppendLine(line); });
        return (code, sb.ToString());
    }
}
