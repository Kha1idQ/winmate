using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace WinMate.Services;

// WinMate runs elevated (it has to, for HKLM tweaks and services), but winget
// refuses to touch packages installed in the user's own scope while elevated:
//   "The package installed for user scope cannot be uninstalled when running
//    with administrator privileges."
//
// The fix is to launch winget back down at the user's normal privilege level.
// We borrow the token of the running shell (explorer.exe), which is exactly the
// interactive, non-elevated user, and start the process with it.
public static class UserProcessRunner
{
    public static async Task<int> RunAsync(string exe, string args, Action<string>? onLine = null)
    {
        var shell = Process.GetProcessesByName("explorer").FirstOrDefault();
        if (shell is null)
            return await ProcessRunner.RunAsync(exe, args, onLine); // no shell: nothing to de-elevate to

        var process = OpenProcess(ProcessQueryInformation, false, shell.Id);
        if (process == IntPtr.Zero)
            return await ProcessRunner.RunAsync(exe, args, onLine);

        IntPtr shellToken = IntPtr.Zero, userToken = IntPtr.Zero, environment = IntPtr.Zero;
        IntPtr readPipe = IntPtr.Zero, writePipe = IntPtr.Zero;

        try
        {
            if (!OpenProcessToken(process, TokenDuplicate | TokenQuery, out shellToken))
                return await ProcessRunner.RunAsync(exe, args, onLine);

            if (!DuplicateTokenEx(shellToken, TokenAllAccess, IntPtr.Zero,
                    SecurityImpersonation, TokenPrimary, out userToken))
                return await ProcessRunner.RunAsync(exe, args, onLine);

            // One pipe for both stdout and stderr; the child inherits the write end.
            var security = new SecurityAttributes
            {
                Length = Marshal.SizeOf<SecurityAttributes>(),
                InheritHandle = true,
            };
            if (!CreatePipe(out readPipe, out writePipe, ref security, 0))
                return await ProcessRunner.RunAsync(exe, args, onLine);
            SetHandleInformation(readPipe, HandleFlagInherit, 0);

            var startup = new StartupInfo
            {
                cb = Marshal.SizeOf<StartupInfo>(),
                lpDesktop = @"winsta0\default",
                dwFlags = StartfUseStdHandles,
                hStdOutput = writePipe,
                hStdError = writePipe,
            };

            CreateEnvironmentBlock(out environment, userToken, false);

            var created = CreateProcessAsUser(
                userToken,
                null,
                $"\"{exe}\" {args}",
                IntPtr.Zero, IntPtr.Zero,
                bInheritHandles: true,
                CreateNoWindow | CreateUnicodeEnvironment,
                environment,
                null,
                ref startup,
                out var info);

            if (!created)
                return await ProcessRunner.RunAsync(exe, args, onLine);

            // The parent must drop its copy of the write end, otherwise the read
            // below never sees end-of-stream.
            CloseHandle(writePipe);
            writePipe = IntPtr.Zero;

            await ReadOutputAsync(readPipe, onLine);
            readPipe = IntPtr.Zero; // the stream owns it now

            WaitForSingleObject(info.hProcess, Infinite);
            GetExitCodeProcess(info.hProcess, out var exitCode);
            CloseHandle(info.hThread);
            CloseHandle(info.hProcess);
            return (int)exitCode;
        }
        finally
        {
            if (environment != IntPtr.Zero) DestroyEnvironmentBlock(environment);
            if (userToken != IntPtr.Zero) CloseHandle(userToken);
            if (shellToken != IntPtr.Zero) CloseHandle(shellToken);
            if (writePipe != IntPtr.Zero) CloseHandle(writePipe);
            if (readPipe != IntPtr.Zero) CloseHandle(readPipe);
            CloseHandle(process);
        }
    }

    private static async Task ReadOutputAsync(IntPtr readPipe, Action<string>? onLine)
    {
        using var handle = new SafeFileHandle(readPipe, ownsHandle: true);
        await using var stream = new FileStream(handle, FileAccess.Read);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                onLine?.Invoke(line);
        }
    }

    private const uint ProcessQueryInformation = 0x0400;
    private const uint TokenDuplicate = 0x0002;
    private const uint TokenQuery = 0x0008;
    private const uint TokenAllAccess = 0xF01FF;
    private const uint CreateNoWindow = 0x08000000;
    private const uint CreateUnicodeEnvironment = 0x00000400;
    private const uint StartfUseStdHandles = 0x00000100;
    private const uint HandleFlagInherit = 0x00000001;
    private const uint Infinite = 0xFFFFFFFF;
    private const int SecurityImpersonation = 2;
    private const int TokenPrimary = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes
    {
        public int Length;
        public IntPtr SecurityDescriptor;
        public bool InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int cb;
        public string? lpReserved;
        public string? lpDesktop;
        public string? lpTitle;
        public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput, hStdOutput, hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr hProcess, hThread;
        public int dwProcessId, dwThreadId;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, int processId);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx(IntPtr existing, uint access, IntPtr attributes,
        int impersonationLevel, int tokenType, out IntPtr newToken);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CreatePipe(out IntPtr readPipe, out IntPtr writePipe,
        ref SecurityAttributes attributes, uint size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetHandleInformation(IntPtr handle, uint mask, uint flags);

    [DllImport("userenv.dll", SetLastError = true)]
    private static extern bool CreateEnvironmentBlock(out IntPtr environment, IntPtr token, bool inherit);

    [DllImport("userenv.dll", SetLastError = true)]
    private static extern bool DestroyEnvironmentBlock(IntPtr environment);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessAsUser(IntPtr token, string? applicationName,
        string commandLine, IntPtr processAttributes, IntPtr threadAttributes, bool bInheritHandles,
        uint creationFlags, IntPtr environment, string? currentDirectory,
        ref StartupInfo startupInfo, out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);
}
