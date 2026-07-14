using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace WinMate.Services;

// Swaps the whole surface + accent palette at runtime by mutating the shared
// brush resources (everything binds them with DynamicResource).
// "blue" is the WinMate Dark palette from the design spec; "gold" is the same
// system rebuilt in warm tones.
public static class ThemeService
{
    public record Palette(
        string Id,
        Color AppBackground, Color Sidebar, Color Surface, Color SurfaceRaised,
        Color NavActive, Color Border,
        Color TextPrimary, Color TextSecondary,
        Color Accent, Color AccentStrong, Color AccentAlt,
        Color Success, Color Warning, Color Disabled,
        Color GradientFrom, Color GradientTo, Color OnPrimary);

    // Design-spec tokens (WinMate Dark). Sidebar matches the app background: one
    // flat canvas, separated by a hairline rather than a shade change.
    public static readonly Palette Blue = new(
        "blue",
        AppBackground: C("#061321"), Sidebar: C("#061321"), Surface: C("#0A1C2B"), SurfaceRaised: C("#0D2234"),
        NavActive: C("#122A45"), Border: C("#29445B"),
        TextPrimary: C("#F4F7FB"), TextSecondary: C("#9EADC1"),
        Accent: C("#25BDF5"), AccentStrong: C("#117FEA"), AccentAlt: C("#7A4FE8"),
        Success: C("#35D58A"), Warning: C("#F5A524"), Disabled: C("#667386"),
        GradientFrom: C("#138CEB"), GradientTo: C("#086BDD"), OnPrimary: C("#FFFFFF"));

    // Same structure, warm palette.
    public static readonly Palette Gold = new(
        "gold",
        AppBackground: C("#12100B"), Sidebar: C("#12100B"), Surface: C("#1C1812"), SurfaceRaised: C("#241F16"),
        NavActive: C("#2E2718"), Border: C("#4E4327"),
        TextPrimary: C("#F8F5EE"), TextSecondary: C("#BFB39C"),
        Accent: C("#E6C566"), AccentStrong: C("#C99A2E"), AccentAlt: C("#C97B4A"),
        Success: C("#35D58A"), Warning: C("#F5A524"), Disabled: C("#6E6553"),
        GradientFrom: C("#E0B84A"), GradientTo: C("#C2952A"), OnPrimary: C("#12100B"));

    public static Palette Current { get; private set; } = Blue;

    public static void Apply(string id)
    {
        var p = id == "gold" ? Gold : Blue;
        Current = p;

        var r = Application.Current.Resources;

        // Surfaces
        SetBrush(r, "AppBackgroundBrush", p.AppBackground);
        SetBrush(r, "ApplicationBackgroundBrush", p.AppBackground);
        SetBrush(r, "SidebarBrush", p.Sidebar);
        SetBrush(r, "SurfaceBrush", p.Surface);
        SetBrush(r, "SurfaceRaisedBrush", p.SurfaceRaised);
        SetBrush(r, "NavActiveBrush", p.NavActive);
        SetBrush(r, "CardBorderBrush", p.Border);

        // Text
        SetBrush(r, "TextPrimaryBrush", p.TextPrimary);
        SetBrush(r, "TextSecondaryBrush", p.TextSecondary);
        SetBrush(r, "DisabledBrush", p.Disabled);

        // Accents
        SetBrush(r, "AccentBrush", p.Accent);
        SetBrush(r, "AccentStrongBrush", p.AccentStrong);
        SetBrush(r, "AccentAltBrush", p.AccentAlt);
        SetBrush(r, "SuccessBrush", p.Success);
        SetBrush(r, "WarningBrush", p.Warning);
        SetBrush(r, "OnPrimaryBrush", p.OnPrimary);

        // Tints for icon tiles and chips (accent over a dark surface).
        SetBrush(r, "AccentTintBrush", WithAlpha(p.Accent, 0x33));
        SetBrush(r, "AccentAltTintBrush", WithAlpha(p.AccentAlt, 0x33));
        SetBrush(r, "SuccessTintBrush", WithAlpha(p.Success, 0x33));
        SetBrush(r, "WarningTintBrush", WithAlpha(p.Warning, 0x33));
        SetBrush(r, "AccentBorderBrush", WithAlpha(p.Accent, 0x88));
        SetBrush(r, "AccentAltBorderBrush", WithAlpha(p.AccentAlt, 0x88));
        SetBrush(r, "SuccessBorderBrush", WithAlpha(p.Success, 0x88));
        SetBrush(r, "WarningBorderBrush", WithAlpha(p.Warning, 0x88));

        // Primary action buttons use a restrained horizontal gradient.
        SetGradient(r, "PrimaryButtonBrush", p.GradientFrom, p.GradientTo);

        // WPF-UI's own theme keys, so its stock controls follow the palette.
        // The navigation pane and content host paint from these dark-theme
        // surface keys, not from ApplicationBackgroundBrush.
        SetBrush(r, "ApplicationBackgroundColorDarkBrush", p.AppBackground);
        SetBrush(r, "ApplicationBackgroundColorLightBrush", p.AppBackground);
        SetBrush(r, "LayerFillColorDefaultBrush", p.Surface);
        SetBrush(r, "LayerFillColorAltBrush", p.AppBackground);
        SetBrush(r, "CardBackgroundFillColorDefaultBrush", p.Surface);
        SetBrush(r, "CardBackgroundFillColorSecondaryBrush", p.SurfaceRaised);
        SetBrush(r, "TextFillColorPrimaryBrush", p.TextPrimary);
        SetBrush(r, "TextFillColorSecondaryBrush", p.TextSecondary);
        SetBrush(r, "TextFillColorTertiaryBrush", p.TextSecondary);
        SetBrush(r, "TextFillColorDisabledBrush", p.Disabled);
        SetBrush(r, "ControlFillColorDefaultBrush", p.Surface);
        SetBrush(r, "ControlFillColorSecondaryBrush", p.SurfaceRaised);
        SetBrush(r, "ControlStrokeColorDefaultBrush", p.Border);
        SetBrush(r, "NavigationViewItemForeground", p.TextSecondary);
        SetBrush(r, "NavigationViewItemForegroundPointerOver", p.TextPrimary);
        SetBrush(r, "NavigationViewItemForegroundSelected", p.Accent);
        SetBrush(r, "NavigationViewItemForegroundPressed", p.TextSecondary);
        SetBrush(r, "NavigationViewItemBackgroundSelected", p.NavActive);
        SetBrush(r, "NavigationViewItemBackgroundSelectedPointerOver", p.NavActive);
        // The content host paints its own dark plate by default — clear it so the
        // window background shows through behind the pages.
        SetBrush(r, "NavigationViewContentBackground", Colors.Transparent);
        SetBrush(r, "NavigationViewContentGridBorderBrush", Colors.Transparent);
        SetBrush(r, "ButtonForeground", p.TextPrimary);
        SetBrush(r, "ButtonForegroundPointerOver", p.TextPrimary);
        SetBrush(r, "ButtonForegroundPressed", p.TextSecondary);
        SetBrush(r, "TextControlForeground", p.TextPrimary);
        SetBrush(r, "TextControlForegroundPointerOver", p.TextPrimary);
        SetBrush(r, "TextControlForegroundFocused", p.TextPrimary);
        SetBrush(r, "TextControlPlaceholderForeground", p.TextSecondary);
        SetBrush(r, "ExpanderHeaderForeground", p.TextPrimary);

        // Toggles and selection highlights.
        ApplicationAccentColorManager.Apply(p.Accent, ApplicationTheme.Dark);

        SettingsService.Current.Theme = id;
        SettingsService.Save();
    }

    public static void Toggle() => Apply(Current.Id == "gold" ? "blue" : "gold");

    // Mutate in place when possible so every DynamicResource consumer updates live.
    private static void SetBrush(ResourceDictionary r, string key, Color c)
    {
        if (r[key] is SolidColorBrush { IsFrozen: false } brush)
            brush.Color = c;
        else
            r[key] = new SolidColorBrush(c);
    }

    private static void SetGradient(ResourceDictionary r, string key, Color from, Color to)
    {
        if (r[key] is LinearGradientBrush { IsFrozen: false } g && g.GradientStops.Count == 2)
        {
            g.GradientStops[0].Color = from;
            g.GradientStops[1].Color = to;
            return;
        }

        r[key] = new LinearGradientBrush(
            [new GradientStop(from, 0), new GradientStop(to, 1)],
            new Point(0, 0), new Point(1, 0));
    }

    private static Color WithAlpha(Color c, byte alpha) => Color.FromArgb(alpha, c.R, c.G, c.B);

    private static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
