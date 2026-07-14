using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinMate.Models;

namespace WinMate.Services;

// Reads and manages everything winget already knows about this machine.
public static class WingetManager
{
    private const string CommonFlags = "--disable-interactivity --accept-source-agreements";

    // Full path to the winget alias — CreateProcessAsUser needs a real path, and
    // relying on PATH inside a de-elevated child is not dependable.
    private static string WingetPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "WindowsApps", "winget.exe");

    public static async Task<IReadOnlyList<InstalledApp>> GetInstalledAsync()
    {
        // "winget list" fills its Available column even for packages whose real
        // version it can't determine, which invents updates that don't exist.
        // "winget upgrade" is the authoritative list of genuine upgrades, so we
        // take the inventory from one and the update flags from the other.
        var (_, listOutput) = await ProcessRunner.RunCapturedAsync("winget", $"list {CommonFlags}");
        var installed = ParseTable(listOutput);

        var (_, upgradeOutput) = await ProcessRunner.RunCapturedAsync("winget", $"upgrade {CommonFlags}");
        var realUpgrades = ParseTable(upgradeOutput)
            .Where(u => !string.IsNullOrWhiteSpace(u.Available))
            .ToDictionary(u => u.Id, u => u.Available, StringComparer.OrdinalIgnoreCase);

        return installed
            .Select(a => a with
            {
                Available = realUpgrades.GetValueOrDefault(a.Id, ""),
            })
            .ToList();
    }

    public static Task<bool> UpgradeAsync(InstalledApp app, Action<string> onLine) =>
        RunPackageActionAsync($"upgrade --id {app.Id} -e --silent {CommonFlags} --accept-package-agreements", onLine);

    public static Task<bool> UpgradeAllAsync(Action<string> onLine) =>
        RunPackageActionAsync($"upgrade --all --silent {CommonFlags} --accept-package-agreements", onLine);

    public static Task<bool> UninstallAsync(InstalledApp app, Action<string> onLine)
    {
        // Store packages are listed as "MSIX\<full name>" — winget --id can't take
        // that, so remove them the Appx way (works for any Store app, not just Edge).
        if (app.Id.StartsWith("MSIX\\", StringComparison.OrdinalIgnoreCase))
            return RemoveAppxAsync(app.Id[5..], onLine);

        return RunPackageActionAsync($"uninstall --id {app.Id} -e --silent {CommonFlags}", onLine);
    }

    // Microsoft blocks Edge's own uninstaller (setup.exe returns 93), so we don't
    // rely on it. We try the official uninstall first (cleanest when it works),
    // and if that's blocked we force-remove Edge ourselves: stop its services and
    // tasks, close its processes, then delete its folders and registry entries.
    public static async Task<bool> UninstallEdgeAsync(InstalledApp app, Action<string> onLine)
    {
        // Edge shows up twice: as a desktop install (files under Program Files)
        // and as an MSIX Store package. They need completely different removal —
        // an Appx package is uninstalled with Remove-AppxPackage, not by deleting files.
        if (app.Id.StartsWith("MSIX\\", StringComparison.OrdinalIgnoreCase))
            return await RemoveAppxAsync(app.Id[5..], onLine);

        EnableDebugPrivilege(); // lets us reach protected processes holding Edge files
        await StopEdgeAsync(onLine);

        // 1. Try the supported uninstaller after unlocking it — best case.
        SetEdgeUnlockFlags();
        var setup = FindEdgeSetup();
        if (setup is not null)
        {
            var code = await ProcessRunner.RunAsync(
                setup,
                "--uninstall --msedge --channel=stable --system-level --verbose-logging --force-uninstall",
                onLine);
            if (code == 0 || !EdgeStillPresent())
            {
                onLine("✓ Edge removed by its own uninstaller.");
                return true;
            }
            onLine($"Uninstaller blocked (code {code}). Switching to force removal...");
        }

        // 2. Force removal — delete everything ourselves.
        await StopEdgeAsync(onLine);
        return await ForceRemoveEdgeAsync(onLine);
    }

    // Store (MSIX) packages — the id after "MSIX\" is the package full name.
    private static async Task<bool> RemoveAppxAsync(string packageFullName, Action<string> onLine)
    {
        onLine($"Removing Store package: {packageFullName}");

        // Remove-AppxPackage is the right, admin-level call. We deliberately don't
        // touch the *provisioned* package (Remove-AppxProvisionedPackage needs a
        // DISM online session and throws "Access denied" here) — removing the
        // installed package for the user is what actually uninstalls Edge.
        var script = $"try {{ Remove-AppxPackage -Package '{packageFullName}' -AllUsers -ErrorAction Stop; 'APPX_OK' }} " +
                     $"catch {{ try {{ Remove-AppxPackage -Package '{packageFullName}' -ErrorAction Stop; 'APPX_OK' }} " +
                     "catch { 'APPX_FAIL: ' + $_.Exception.Message } }";

        var (_, output) = await ProcessRunner.RunCapturedAsync(
            "powershell.exe",
            $"-NoProfile -Command \"{script}\"");

        foreach (var line in output.Split('\n'))
            if (!string.IsNullOrWhiteSpace(line))
                onLine(line.Trim());

        // Success if the command reported OK, or if the package is simply gone.
        return output.Contains("APPX_OK") || output.Contains("not found", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task StopEdgeAsync(Action<string> onLine)
    {
        onLine("Stopping Edge services, tasks and processes...");

        foreach (var svc in new[] { "edgeupdate", "edgeupdatem", "MicrosoftEdgeElevationService" })
            await ProcessRunner.RunAsync("sc.exe", $"stop {svc}");
        foreach (var svc in new[] { "edgeupdate", "edgeupdatem", "MicrosoftEdgeElevationService" })
            await ProcessRunner.RunAsync("sc.exe", $"config {svc} start= disabled");

        await ProcessRunner.RunAsync("schtasks",
            "/Change /TN \"MicrosoftEdgeUpdateTaskMachineCore\" /Disable");
        await ProcessRunner.RunAsync("schtasks",
            "/Change /TN \"MicrosoftEdgeUpdateTaskMachineUA\" /Disable");

        // Edge's WebView is loaded by Windows components too — Start menu search
        // (SearchHost) and Widgets hold EmbeddedBrowserWebView.dll open. They
        // restart themselves automatically, so stopping them to free the file is safe.
        foreach (var name in new[]
                 {
                     "msedge", "msedgewebview2", "MicrosoftEdgeUpdate", "identity_helper",
                     "elevation_service", "SearchHost", "Widgets", "WidgetService",
                 })
        {
            foreach (var proc in System.Diagnostics.Process.GetProcessesByName(name))
            {
                try { proc.Kill(entireProcessTree: true); proc.WaitForExit(4000); }
                catch { /* already gone or protected */ }
            }
        }
        await Task.Delay(1200);
    }

    private static void SetEdgeUnlockFlags()
    {
        var keys = new[]
        {
            (RegistryView.Registry32, @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdateDev"),
            (RegistryView.Registry64, @"SOFTWARE\Microsoft\EdgeUpdateDev"),
        };
        foreach (var (view, path) in keys)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var key = baseKey.CreateSubKey(path);
                key?.SetValue("AllowUninstall", 1, RegistryValueKind.DWord);
            }
            catch { /* ACL-protected; not fatal — force removal doesn't need it */ }
        }
    }

    private static async Task<bool> ForceRemoveEdgeAsync(Action<string> onLine)
    {
        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var folders = new[]
        {
            Path.Combine(pf86, "Microsoft", "Edge"),
            Path.Combine(pf86, "Microsoft", "EdgeCore"),
            Path.Combine(pf86, "Microsoft", "EdgeUpdate"),
            Path.Combine(pf86, "Microsoft", "EdgeWebView"),
            Path.Combine(pf, "Microsoft", "Edge"),
            Path.Combine(pf, "Microsoft", "EdgeCore"),
        };

        var removedAny = false;
        var scheduledForReboot = 0;
        foreach (var folder in folders.Where(Directory.Exists))
        {
            // Take ownership from TrustedInstaller and grant admins full control
            // over the whole tree, so the file deletes below can succeed.
            await ProcessRunner.RunAsync("cmd.exe", $"/c takeown /f \"{folder}\" /r /d y >nul 2>&1");
            await ProcessRunner.RunAsync("cmd.exe", $"/c icacls \"{folder}\" /grant *S-1-5-32-544:F /t /c >nul 2>&1");

            // First pass: try deleting every file, collecting the ones still locked.
            var locked = new List<string>();
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (!TryDeleteFile(file))
                    locked.Add(file);
            }

            // Ask Restart Manager who's holding the locked files and shut them down
            // (SearchHost, Widgets, WebView hosts) — then retry those files.
            if (locked.Count > 0)
            {
                RestartManager.FreeFiles(locked, onLine);
                await Task.Delay(600);

                foreach (var file in locked.ToList())
                {
                    if (TryDeleteFile(file))
                        locked.Remove(file);
                }
            }

            // Anything still locked is queued for deletion on the next reboot.
            foreach (var file in locked)
            {
                if (MoveFileEx(file, null, MoveFileDelayUntilReboot))
                    scheduledForReboot++;
            }

            try
            {
                Directory.Delete(folder, recursive: true);
                onLine($"✓ Removed {folder}");
                removedAny = true;
            }
            catch
            {
                // Directory still has queued-for-reboot files; remove it on reboot too.
                MoveFileEx(folder, null, MoveFileDelayUntilReboot);
                onLine($"✓ {folder} — emptied; folder removed on restart");
                removedAny = true;
            }
        }

        if (scheduledForReboot > 0)
            onLine($"ℹ {scheduledForReboot} file(s) were in use and will be deleted after a restart.");

        // Remove the registry entries that keep Edge showing as installed. The
        // Programs-and-Features entry is keyed by a random GUID (different on every
        // machine, e.g. {9F2EDF9C-...}), not by the name "Microsoft Edge" — so we
        // find it by its DisplayName, not a fixed path. We also clear the
        // EdgeUpdate client keys winget reads.
        DeleteUninstallEntriesNamed("Microsoft Edge");

        foreach (var (view, path) in new[]
                 {
                     (RegistryView.Registry32, @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients"),
                     (RegistryView.Registry64, @"SOFTWARE\Microsoft\EdgeUpdate\Clients"),
                     (RegistryView.Registry32, @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate"),
                     (RegistryView.Registry64, @"SOFTWARE\Microsoft\EdgeUpdate"),
                 })
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                baseKey.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
            }
            catch { /* protected key; folders being gone is what matters */ }
        }

        // Remove the leftover shortcuts (the desktop / Start-menu Edge icons).
        foreach (var shortcut in new[]
                 {
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Microsoft Edge.lnk"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Microsoft Edge.lnk"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "Microsoft Edge.lnk"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\Microsoft Edge.lnk"),
                 })
        {
            try { if (File.Exists(shortcut)) File.Delete(shortcut); }
            catch { /* pinned/locked shortcut — not critical */ }
        }

        // Block Windows Update from silently reinstalling Edge.
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var key = baseKey.CreateSubKey(@"SOFTWARE\Microsoft\EdgeUpdate");
            key?.SetValue("DoNotUpdateToEdgeWithChromium", 1, RegistryValueKind.DWord);
        }
        catch { /* best effort */ }

        // What matters is the end state, not how many folders this run touched.
        // If Edge's binary is gone it's uninstalled — whether we removed it now or
        // a previous run already did.
        if (!EdgeStillPresent())
        {
            onLine(scheduledForReboot > 0
                ? "✓ Edge removed. Restart your PC to clear the last few in-use files."
                : "✓ Edge removed.");
            return true;
        }

        if (scheduledForReboot > 0)
        {
            onLine("✓ Edge queued for removal — restart your PC to finish deleting the in-use files.");
            return true;
        }

        onLine("✗ Some Edge files are locked and could not be removed. Restart your PC, then try once more.");
        return false;
    }

    private static void EnableDebugPrivilege()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess().Handle;
            if (!OpenProcessToken(process, TokenAdjustPrivileges | TokenQuery, out var token))
                return;
            try
            {
                if (LookupPrivilegeValue(null, "SeDebugPrivilege", out var luid))
                {
                    var tp = new TokenPrivileges
                    {
                        PrivilegeCount = 1,
                        Luid = luid,
                        Attributes = SePrivilegeEnabled,
                    };
                    AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                }
            }
            finally
            {
                CloseHandle(token);
            }
        }
        catch { /* best effort */ }
    }

    private const uint TokenAdjustPrivileges = 0x0020;
    private const uint TokenQuery = 0x0008;
    private const uint SePrivilegeEnabled = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct Luid { public uint LowPart; public int HighPart; }

    [StructLayout(LayoutKind.Sequential)]
    private struct TokenPrivileges
    {
        public uint PrivilegeCount;
        public Luid Luid;
        public uint Attributes;
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool LookupPrivilegeValue(string? system, string name, out Luid luid);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr token, bool disableAll,
        ref TokenPrivileges newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    // Deletes any "Programs and Features" entry whose DisplayName exactly matches,
    // across all registry views. Edge's entry is keyed by a random GUID, so we
    // must match on the name rather than a fixed key path.
    private static void DeleteUninstallEntriesNamed(string displayName)
    {
        var views = new[]
        {
            (RegistryHive.LocalMachine, RegistryView.Registry64),
            (RegistryHive.LocalMachine, RegistryView.Registry32),
            (RegistryHive.CurrentUser, RegistryView.Registry64),
        };

        foreach (var (hive, view) in views)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                const string uninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                using var root = baseKey.OpenSubKey(uninstall, writable: true);
                if (root is null)
                    continue;

                foreach (var subName in root.GetSubKeyNames())
                {
                    string? name;
                    using (var sub = root.OpenSubKey(subName))
                        name = sub?.GetValue("DisplayName") as string;

                    if (string.Equals(name, displayName, StringComparison.OrdinalIgnoreCase))
                        root.DeleteSubKeyTree(subName, throwOnMissingSubKey: false);
                }
            }
            catch { /* a view we can't read is not fatal */ }
        }
    }

    private static bool TryDeleteFile(string file)
    {
        try
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool EdgeStillPresent()
    {
        // Edge's browser binary lives in a versioned Application folder; if none
        // of them contain msedge.exe, Edge is not installed.
        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var appDir = Path.Combine(pf86, "Microsoft", "Edge", "Application");
        if (!Directory.Exists(appDir))
            return false;

        try
        {
            return Directory.EnumerateFiles(appDir, "msedge.exe", SearchOption.AllDirectories).Any();
        }
        catch
        {
            return File.Exists(Path.Combine(appDir, "msedge.exe"));
        }
    }

    private const uint MoveFileDelayUntilReboot = 0x4;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool MoveFileEx(string existingFileName, string? newFileName, uint flags);

    private static string? FindEdgeSetup()
    {
        foreach (var root in new[]
                 {
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                 })
        {
            var appDir = Path.Combine(root, "Microsoft", "Edge", "Application");
            if (!Directory.Exists(appDir))
                continue;

            // Pick the newest version folder that actually has a setup.exe.
            var setup = Directory.GetDirectories(appDir)
                .Where(d => char.IsDigit(Path.GetFileName(d).FirstOrDefault()))
                .OrderByDescending(d => d)
                .Select(d => Path.Combine(d, "Installer", "setup.exe"))
                .FirstOrDefault(File.Exists);

            if (setup is not null)
                return setup;
        }
        return null;
    }

    // Machine-scope packages need our elevated token; user-scope packages refuse
    // it outright. So: try elevated first (silent for machine-scope), and if that
    // fails, run the same command back at the user's own privilege level.
    private static async Task<bool> RunPackageActionAsync(string args, Action<string> onLine)
    {
        var code = await ProcessRunner.RunAsync("winget", args, onLine);
        if (code == 0)
            return true;

        onLine("Retrying without administrator privileges...");
        var userCode = await UserProcessRunner.RunAsync(WingetPath, args, onLine);
        return userCode == 0;
    }

    // winget prints a fixed-width table. Rather than assuming English headers or
    // fixed widths, we read the column start positions out of the header line
    // itself — so this keeps working on a non-English Windows.
    internal static List<InstalledApp> ParseTable(string output)
    {
        var lines = output.Replace("\r", "").Split('\n');

        var separator = Array.FindIndex(lines, l =>
            l.Trim().Length > 10 && l.Trim().All(c => c == '-'));
        if (separator <= 0)
            return [];

        var offsets = ColumnOffsets(lines[separator - 1]);
        if (offsets.Count < 3)
            return [];

        var apps = new List<InstalledApp>();
        for (var i = separator + 1; i < lines.Length; i++)
        {
            var cells = SplitRow(lines[i], offsets);
            if (cells is null)
                continue;

            // Column order is Name, Id, Version, [Available], Source.
            var name = cells[0];
            var id = cells[1];
            var version = cells.Length > 2 ? cells[2] : "";
            var available = cells.Length > 4 ? cells[3] : "";
            var source = cells.Length > 4 ? cells[4] : (cells.Length > 3 ? cells[3] : "");

            // Trailing summary lines ("17 upgrades available.") carry no package
            // id, and real ids never contain spaces.
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id) || id.Contains(' '))
                continue;

            apps.Add(new InstalledApp(name, id, version, available, source));
        }

        return apps;
    }

    private static List<int> ColumnOffsets(string header)
    {
        var offsets = new List<int>();
        for (var i = 0; i < header.Length; i++)
        {
            if (header[i] == ' ')
                continue;

            // A column starts at the line start, or after a run of 2+ spaces.
            if (i == 0 || (i >= 2 && header[i - 1] == ' ' && header[i - 2] == ' '))
                offsets.Add(i);
        }
        return offsets;
    }

    private static string[]? SplitRow(string line, List<int> offsets)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var cells = new string[offsets.Count];
        for (var c = 0; c < offsets.Count; c++)
        {
            var start = offsets[c];
            if (start >= line.Length)
            {
                cells[c] = "";
                continue;
            }

            var end = c + 1 < offsets.Count
                ? Math.Min(offsets[c + 1], line.Length)
                : line.Length;
            cells[c] = line[start..end].Trim();
        }
        return cells;
    }
}
