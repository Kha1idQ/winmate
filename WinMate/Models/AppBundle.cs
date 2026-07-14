namespace WinMate.Models;

// A one-click set of apps: clicking it ticks all its apps in the catalog so the
// user can review and install them together.
public record AppBundle(
    string Id,
    string NameEn,
    string NameAr,
    string Icon,
    IReadOnlyList<string> WingetIds);
