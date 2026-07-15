namespace WinMate.Services;

// Thrown when a winget scan genuinely fails (non-zero exit with no usable
// output). We raise it instead of returning an empty list so the UI can show a
// real error rather than a misleading "nothing installed / up to date".
public sealed class WingetException(int exitCode, string output)
    : Exception($"winget scan failed (exit 0x{exitCode:X8}).")
{
    public int ExitCode { get; } = exitCode;
    public string Output { get; } = output;
}
