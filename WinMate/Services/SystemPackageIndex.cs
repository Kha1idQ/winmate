using Microsoft.Win32;
using WinMate.Models;

namespace WinMate.Services;

public enum PackageClass
{
    Normal,     // an ordinary app — safe to remove
    System,     // a runtime, driver or Windows component — removing it can break other software
    Protected,  // Windows itself forbids removal (e.g. Microsoft Edge)
}

// Decides what a package actually IS, by asking Windows rather than guessing
// from names. Every machine carries different components, so the index is built
// from that machine's own metadata each time the page loads.
//
// No single flag is enough — Windows marks each kind differently:
//   Microsoft Edge         -> NoRemove = 1
//   .NET Desktop Runtime   -> SystemComponent = 1
//   Visual C++ Redist      -> a "removing this may prevent applications from
//                             running" caution string in Comments
//   Graphics/audio drivers -> neither; identified by publisher
//   Store frameworks       -> Appx IsFramework / NonRemovable / SignatureKind
// so we read all of them and combine.
public sealed class SystemPackageIndex
{
    private readonly Dictionary<string, ArpEntry> _arpByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ArpEntry> _arpByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AppxEntry> _appxByFullName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AppxEntry> _appxByName = new(StringComparer.OrdinalIgnoreCase);

    private record ArpEntry(
        string Name, string Publisher, bool SystemComponent, bool NoRemove,
        bool HasUninstallString, string Comments);

    private record AppxEntry(
        string Name, string FullName, bool IsFramework, bool NonRemovable, string SignatureKind);

    public static async Task<SystemPackageIndex> BuildAsync()
    {
        var index = new SystemPackageIndex();
        index.ReadRegistry();
        await index.ReadAppxAsync();
        return index;
    }

    // Microsoft's browser: flagged NoRemove by Windows, but users routinely want
    // it gone. We treat it as a removable system component and take it out via a
    // dedicated path (WingetManager.UninstallEdgeAsync).
    public static bool IsEdge(InstalledApp app) =>
        app.Id.Equals("Microsoft.Edge", StringComparison.OrdinalIgnoreCase)
        || app.Name.Equals("Microsoft Edge", StringComparison.OrdinalIgnoreCase);

    public PackageClass Classify(InstalledApp app)
    {
        if (IsEdge(app))
            return PackageClass.System; // removable, but warn first

        var arp = LookupArp(app);
        var appx = LookupAppx(app);

        // 1. Windows says this cannot be removed at all.
        if (arp is { NoRemove: true })
            return PackageClass.Protected;
        if (arp is { HasUninstallString: false })
            return PackageClass.Protected;
        if (appx is { NonRemovable: true })
            return PackageClass.Protected;
        if (appx is not null && appx.SignatureKind.Equals("System", StringComparison.OrdinalIgnoreCase))
            return PackageClass.Protected;

        // 2. Removable, but other software depends on it.
        if (arp is { SystemComponent: true })
            return PackageClass.System;
        if (arp is not null && HasVendorCaution(arp.Comments))
            return PackageClass.System;
        if (appx is { IsFramework: true })
            return PackageClass.System;

        // Windows' own platform components live under the "MicrosoftWindows."
        // namespace. Some are technically removable, but nobody should delete one
        // without knowing what it is.
        if (appx is not null && appx.Name.StartsWith("MicrosoftWindows.", StringComparison.OrdinalIgnoreCase))
            return PackageClass.System;

        if (IsDriver(app, arp?.Publisher))
            return PackageClass.System;
        if (Data.SystemPackages.IsKnownRuntime(app))
            return PackageClass.System;

        return PackageClass.Normal;
    }

    // Microsoft ships this exact caution on the redistributables.
    private static bool HasVendorCaution(string comments) =>
        comments.Contains("prevent some applications", StringComparison.OrdinalIgnoreCase)
        || comments.Contains("Caution", StringComparison.OrdinalIgnoreCase);

    private static readonly string[] DriverPublishers =
    [
        "NVIDIA", "Advanced Micro Devices", "AMD", "Intel", "Realtek", "Qualcomm",
        "Broadcom", "Synaptics", "MediaTek", "Logitech", "ASUS", "Gigabyte", "MSI",
    ];

    private static readonly string[] DriverWords =
    [
        "driver", "chipset", "graphics", "audio", "firmware", "physx",
    ];

    private static bool IsDriver(InstalledApp app, string? publisher)
    {
        if (string.IsNullOrEmpty(publisher))
            return false;

        var fromDriverVendor = DriverPublishers.Any(p =>
            publisher.Contains(p, StringComparison.OrdinalIgnoreCase));

        return fromDriverVendor && DriverWords.Any(w =>
            app.Name.Contains(w, StringComparison.OrdinalIgnoreCase));
    }

    private ArpEntry? LookupArp(InstalledApp app)
    {
        // winget shows registry apps as "ARP\Machine\X64\<registry key name>".
        if (app.Id.StartsWith("ARP\\", StringComparison.OrdinalIgnoreCase))
        {
            var key = app.Id[(app.Id.LastIndexOf('\\') + 1)..];
            if (_arpByKey.TryGetValue(key, out var byKey))
                return byKey;
        }

        return _arpByName.GetValueOrDefault(app.Name);
    }

    private AppxEntry? LookupAppx(InstalledApp app)
    {
        if (app.Id.StartsWith("MSIX\\", StringComparison.OrdinalIgnoreCase))
        {
            var fullName = app.Id[5..];
            if (_appxByFullName.TryGetValue(fullName, out var byFullName))
                return byFullName;

            // Fall back to the family name (everything before the version).
            var family = fullName.Split('_')[0];
            if (_appxByName.TryGetValue(family, out var byFamily))
                return byFamily;
        }

        return _appxByName.GetValueOrDefault(app.Id);
    }

    private void ReadRegistry()
    {
        var roots = new (RegistryHive Hive, RegistryView View)[]
        {
            (RegistryHive.LocalMachine, RegistryView.Registry64),
            (RegistryHive.LocalMachine, RegistryView.Registry32),
            (RegistryHive.CurrentUser, RegistryView.Registry64),
        };

        foreach (var (hive, view) in roots)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var uninstall = baseKey.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (uninstall is null)
                    continue;

                foreach (var keyName in uninstall.GetSubKeyNames())
                {
                    using var entry = uninstall.OpenSubKey(keyName);
                    if (entry is null)
                        continue;

                    var name = entry.GetValue("DisplayName") as string;
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var record = new ArpEntry(
                        Name: name,
                        Publisher: entry.GetValue("Publisher") as string ?? "",
                        SystemComponent: ReadFlag(entry, "SystemComponent"),
                        NoRemove: ReadFlag(entry, "NoRemove"),
                        HasUninstallString:
                            entry.GetValue("UninstallString") is string u && !string.IsNullOrWhiteSpace(u),
                        Comments: entry.GetValue("Comments") as string ?? "");

                    _arpByKey[keyName] = record;
                    _arpByName[name] = record;
                }
            }
            catch
            {
                // A hive we can't read is not a reason to fail the whole page.
            }
        }
    }

    private static bool ReadFlag(RegistryKey key, string name) =>
        key.GetValue(name) is int value && value == 1;

    private async Task ReadAppxAsync()
    {
        // Get-AppxPackage is the only source for the Store-package flags.
        const string script =
            "Get-AppxPackage | ForEach-Object { " +
            "$_.Name + '|' + $_.PackageFullName + '|' + $_.IsFramework + '|' + " +
            "$_.NonRemovable + '|' + $_.SignatureKind }";

        try
        {
            var (_, output) = await ProcessRunner.RunCapturedAsync(
                "powershell.exe", $"-NoProfile -Command \"{script}\"");

            foreach (var line in output.Split('\n'))
            {
                var parts = line.Trim().Split('|');
                if (parts.Length < 5)
                    continue;

                var record = new AppxEntry(
                    Name: parts[0],
                    FullName: parts[1],
                    IsFramework: parts[2].Equals("True", StringComparison.OrdinalIgnoreCase),
                    NonRemovable: parts[3].Equals("True", StringComparison.OrdinalIgnoreCase),
                    SignatureKind: parts[4]);

                _appxByFullName[record.FullName] = record;
                _appxByName[record.Name] = record;
            }
        }
        catch
        {
            // Without Appx data we still have the registry signals.
        }
    }
}
