namespace WinMate.Models;

// A one-click set of apps: clicking it ticks all its apps in the catalog so the
// user can review and install them together.
public record AppBundle(
    string Id,
    string NameEn,
    string NameAr,
    Wpf.Ui.Controls.SymbolRegular Icon,
    IReadOnlyList<string> WingetIds);
