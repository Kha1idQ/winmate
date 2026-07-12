using WinMate.Data;
using WinMate.Models;

namespace WinMate.Services;

public static class WingetService
{
    // winget returns this when the package is already installed and has no
    // applicable upgrade — treat it as success, not failure.
    private const int AlreadyInstalledCode = unchecked((int)0x8A15002B);

    public static async Task<bool> InstallAsync(AppItem app, Action<string> onLine)
    {
        var exitCode = await ProcessRunner.RunAsync(
            "winget",
            $"install --id {app.WingetId} -e --silent --accept-source-agreements --accept-package-agreements --disable-interactivity",
            onLine);

        return exitCode == 0 || exitCode == AlreadyInstalledCode;
    }

    // One "winget list" pass; returns the catalog ids found on this machine.
    public static async Task<HashSet<string>> GetInstalledIdsAsync()
    {
        var (_, output) = await ProcessRunner.RunCapturedAsync(
            "winget", "list --disable-interactivity --accept-source-agreements");

        return AppCatalog.All
            .Where(a => output.Contains(a.WingetId, StringComparison.OrdinalIgnoreCase))
            .Select(a => a.WingetId)
            .ToHashSet();
    }
}
