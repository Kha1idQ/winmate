using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace WinMate.Services;

// Keeps the chosen Overdrive system intact while offering two low-light
// calibrations. Existing setting ids ("blue" / "gold") remain valid so this
// redesign does not invalidate a user's saved preferences.
public static class ThemeService
{
    public record Palette(
        string Id,
        Color AppBackground, Color Sidebar, Color Surface, Color SurfaceRaised,
        Color NavActive, Color Border, Color BorderStrong,
        Color TextPrimary, Color TextSecondary, Color TextQuiet,
        Color Accent, Color AccentStrong, Color AccentAlt,
        Color Success, Color Warning, Color Error, Color Focus, Color Disabled,
        Color OnPrimary);

    public static readonly Palette Overdrive = new(
        "blue",
        AppBackground: C("#0B0907"), Sidebar: C("#0A0806"), Surface: C("#120F0B"), SurfaceRaised: C("#17130D"),
        NavActive: C("#211809"), Border: C("#3A3022"), BorderStrong: C("#685333"),
        TextPrimary: C("#F2EEE5"), TextSecondary: C("#A39886"), TextQuiet: C("#756C5E"),
        Accent: C("#F2A514"), AccentStrong: C("#D98E08"), AccentAlt: C("#D5B56A"),
        Success: C("#78CF82"), Warning: C("#E7B95F"), Error: C("#E06A54"), Focus: C("#FFD166"),
        Disabled: C("#625A4C"), OnPrimary: C("#211506"));

    public static readonly Palette LowLight = new(
        "gold",
        AppBackground: C("#080705"), Sidebar: C("#080705"), Surface: C("#0E0C09"), SurfaceRaised: C("#151109"),
        NavActive: C("#1A1308"), Border: C("#32291C"), BorderStrong: C("#5C482A"),
        TextPrimary: C("#EDE8DE"), TextSecondary: C("#928775"), TextQuiet: C("#6B6255"),
        Accent: C("#D8900A"), AccentStrong: C("#C27B05"), AccentAlt: C("#B79A58"),
        Success: C("#6DBB75"), Warning: C("#D7AA54"), Error: C("#CB604F"), Focus: C("#EFB848"),
        Disabled: C("#574F43"), OnPrimary: C("#1A1105"));

    public static Palette Current { get; private set; } = Overdrive;

    public static void Apply(string id)
    {
        var p = id == "gold" ? LowLight : Overdrive;
        Current = p;

        var r = Application.Current.Resources;

        SetBrush(r, "AppBackgroundBrush", p.AppBackground);
        SetBrush(r, "ApplicationBackgroundBrush", p.AppBackground);
        SetBrush(r, "SidebarBrush", p.Sidebar);
        SetBrush(r, "SurfaceBrush", p.Surface);
        SetBrush(r, "SurfaceRaisedBrush", p.SurfaceRaised);
        SetBrush(r, "NavActiveBrush", p.NavActive);
        SetBrush(r, "CardBorderBrush", p.Border);
        SetBrush(r, "CardBorderStrongBrush", p.BorderStrong);

        SetBrush(r, "TextPrimaryBrush", p.TextPrimary);
        SetBrush(r, "TextSecondaryBrush", p.TextSecondary);
        SetBrush(r, "TextQuietBrush", p.TextQuiet);
        SetBrush(r, "DisabledBrush", p.Disabled);

        SetBrush(r, "AccentBrush", p.Accent);
        SetBrush(r, "AccentStrongBrush", p.AccentStrong);
        SetBrush(r, "AccentAltBrush", p.AccentAlt);
        SetBrush(r, "SuccessBrush", p.Success);
        SetBrush(r, "WarningBrush", p.Warning);
        SetBrush(r, "ErrorBrush", p.Error);
        SetBrush(r, "FocusBrush", p.Focus);
        SetBrush(r, "OnPrimaryBrush", p.OnPrimary);
        SetBrush(r, "PrimaryButtonBrush", p.Accent);

        SetBrush(r, "AccentTintBrush", WithAlpha(p.Accent, 0x24));
        SetBrush(r, "AccentAltTintBrush", WithAlpha(p.AccentAlt, 0x20));
        SetBrush(r, "SuccessTintBrush", WithAlpha(p.Success, 0x20));
        SetBrush(r, "WarningTintBrush", WithAlpha(p.Warning, 0x24));
        SetBrush(r, "AccentBorderBrush", WithAlpha(p.Accent, 0xA6));
        SetBrush(r, "AccentAltBorderBrush", WithAlpha(p.AccentAlt, 0x80));
        SetBrush(r, "SuccessBorderBrush", WithAlpha(p.Success, 0x80));
        SetBrush(r, "WarningBorderBrush", WithAlpha(p.Warning, 0x8F));

        // WPF-UI stock templates follow the same paper/ink system.
        SetBrush(r, "ApplicationBackgroundColorDarkBrush", p.AppBackground);
        SetBrush(r, "ApplicationBackgroundColorLightBrush", p.AppBackground);
        SetBrush(r, "LayerFillColorDefaultBrush", p.Surface);
        SetBrush(r, "LayerFillColorAltBrush", p.AppBackground);
        SetBrush(r, "CardBackgroundFillColorDefaultBrush", p.Surface);
        SetBrush(r, "CardBackgroundFillColorSecondaryBrush", p.SurfaceRaised);
        SetBrush(r, "TextFillColorPrimaryBrush", p.TextPrimary);
        SetBrush(r, "TextFillColorSecondaryBrush", p.TextSecondary);
        SetBrush(r, "TextFillColorTertiaryBrush", p.TextQuiet);
        SetBrush(r, "TextFillColorDisabledBrush", p.Disabled);
        SetBrush(r, "ControlFillColorDefaultBrush", p.Surface);
        SetBrush(r, "ControlFillColorSecondaryBrush", p.SurfaceRaised);
        SetBrush(r, "ControlFillColorTertiaryBrush", p.NavActive);
        SetBrush(r, "ControlStrokeColorDefaultBrush", p.Border);
        SetBrush(r, "ControlStrokeColorSecondaryBrush", p.BorderStrong);
        SetBrush(r, "NavigationViewItemForeground", p.TextSecondary);
        SetBrush(r, "NavigationViewItemForegroundPointerOver", p.TextPrimary);
        SetBrush(r, "NavigationViewItemForegroundSelected", p.Accent);
        SetBrush(r, "NavigationViewItemForegroundPressed", p.TextPrimary);
        SetBrush(r, "NavigationViewItemBackgroundSelected", p.NavActive);
        SetBrush(r, "NavigationViewItemBackgroundSelectedPointerOver", p.NavActive);
        SetBrush(r, "NavigationViewContentBackground", Colors.Transparent);
        SetBrush(r, "NavigationViewContentGridBorderBrush", Colors.Transparent);
        SetBrush(r, "ButtonForeground", p.TextPrimary);
        SetBrush(r, "ButtonForegroundPointerOver", p.TextPrimary);
        SetBrush(r, "ButtonForegroundPressed", p.TextPrimary);
        SetBrush(r, "ButtonForegroundDisabled", p.TextQuiet);
        SetBrush(r, "ButtonBorderBrushDisabled", p.Border);
        SetBrush(r, "TextControlForeground", p.TextPrimary);
        SetBrush(r, "TextControlForegroundPointerOver", p.TextPrimary);
        SetBrush(r, "TextControlForegroundFocused", p.TextPrimary);
        SetBrush(r, "TextControlPlaceholderForeground", p.TextSecondary);
        SetBrush(r, "ExpanderHeaderForeground", p.TextPrimary);

        ApplicationAccentColorManager.Apply(p.Accent, ApplicationTheme.Dark);

        SettingsService.Current.Theme = id;
        SettingsService.Save();
    }

    public static void Toggle() => Apply(Current.Id == "gold" ? "blue" : "gold");

    private static void SetBrush(ResourceDictionary resources, string key, Color color)
    {
        if (resources[key] is SolidColorBrush { IsFrozen: false } brush)
            brush.Color = color;
        else
            resources[key] = new SolidColorBrush(color);
    }

    private static Color WithAlpha(Color color, byte alpha) =>
        Color.FromArgb(alpha, color.R, color.G, color.B);

    private static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
