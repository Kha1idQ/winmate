namespace WinMate.Services;

// Central app log. Pages write lines from any thread; MainWindow subscribes
// and displays them in the bottom log panel (marshalling to the UI thread).
public static class LogService
{
    public static event Action<string>? LineWritten;

    public static void Write(string line) =>
        LineWritten?.Invoke($"[{DateTime.Now:HH:mm:ss}] {line}");

    public static void WriteRaw(string line) => LineWritten?.Invoke(line);
}
