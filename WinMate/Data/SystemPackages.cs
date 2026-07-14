using WinMate.Models;

namespace WinMate.Data;

// A safety net, not the primary source. SystemPackageIndex classifies packages
// from Windows' own metadata; this list catches the well-known runtimes that
// carry no distinguishing flag at all — Visual C++ redistributables are visible,
// removable, and unmarked, yet removing one breaks half the software on the PC.
public static class SystemPackages
{
    // Matched anywhere in the package id (MSIX and ARP ids carry a prefix).
    private static readonly string[] IdMarkers =
    [
        "Microsoft.VCRedist",
        "Microsoft.VCLibs",
        "Microsoft.NET.Native",
        "Microsoft.DotNet.Native",
        "Microsoft.DotNet.Runtime",
        "Microsoft.DotNet.DesktopRuntime",
        "Microsoft.DotNet.AspNetCore",
        "Microsoft.WindowsAppRuntime",
        "Microsoft.UI.Xaml",
        "Microsoft.AppInstaller",
        "Microsoft.DesktopAppInstaller",
        "Microsoft.WindowsStore",
        "Microsoft.Edge",
        "Microsoft.Services.Store",
        "Microsoft.WebView",
    ];

    // Matched anywhere in the display name, for packages with opaque ids.
    private static readonly string[] NameMarkers =
    [
        "visual c++", "redistributable", "webview",
        "desktop runtime", ".net runtime", "native framework", "native runtime",
        "runtime package", "app installer", "microsoft store", "microsoft edge",
        "directx", "windows sdk", "software development kit", "xna framework",
    ];

    public static bool IsKnownRuntime(InstalledApp app) =>
        IdMarkers.Any(m => app.Id.Contains(m, StringComparison.OrdinalIgnoreCase))
        || NameMarkers.Any(m => app.Name.Contains(m, StringComparison.OrdinalIgnoreCase));
}
