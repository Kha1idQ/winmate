using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace WinMate.Services;

// Swaps the app's accent palette at runtime by mutating the shared brush
// resources (everything references them via DynamicResource), plus the
// WPF-UI accent and the particle-background color.
public static class ThemeService
{
    public record Palette(
        string Id,
        Color Accent, Color AccentDim, Color AccentTertiary, Color AccentBright,
        Color WpfUiAccent, Color Background, Color CardFill, Color CardStroke, Color Particle);

    public static readonly Palette Gold = new(
        "gold",
        Accent: C("#E6C566"), AccentDim: C("#B69A4E"), AccentTertiary: C("#8A7538"), AccentBright: C("#F5DB8A"),
        WpfUiAccent: C("#D4AF37"), Background: C("#1B1916"), CardFill: C("#26221A"), CardStroke: C("#463C1F"),
        Particle: C("#D4AF37"));

    public static readonly Palette NeonBlue = new(
        "neon",
        Accent: C("#45D2FF"), AccentDim: C("#2C93BE"), AccentTertiary: C("#1E6F8F"), AccentBright: C("#8AE6FF"),
        WpfUiAccent: C("#00B7FF"), Background: C("#0A0F14"), CardFill: C("#111C24"), CardStroke: C("#1F3D4C"),
        Particle: C("#35C6FF"));

    public static Palette Current { get; private set; } = Gold;
    public static Color ParticleColor => Current.Particle;

    // ParticleBackground listens so it can recolor its dust on a theme switch.
    public static event Action? ThemeChanged;

    public static void Apply(string id)
    {
        var p = id == "neon" ? NeonBlue : Gold;
        Current = p;

        var r = Application.Current.Resources;
        Set(r, "GoldBrush", p.Accent);
        Set(r, "GoldDimBrush", p.AccentDim);
        Set(r, "ApplicationBackgroundBrush", p.Background);
        Set(r, "AppBackgroundBrush", p.Background);
        Set(r, "TextFillColorPrimaryBrush", p.Accent);
        Set(r, "TextFillColorSecondaryBrush", p.AccentDim);
        Set(r, "TextFillColorTertiaryBrush", p.AccentTertiary);
        Set(r, "ControlFillColorDefaultBrush", p.CardFill);
        Set(r, "ControlStrokeColorDefaultBrush", p.CardStroke);
        Set(r, "NavigationViewItemForeground", p.Accent);
        Set(r, "NavigationViewItemForegroundPointerOver", p.AccentBright);
        Set(r, "NavigationViewItemForegroundSelected", p.AccentBright);
        Set(r, "NavigationViewItemForegroundPressed", p.AccentDim);
        Set(r, "ButtonForeground", p.Accent);
        Set(r, "ButtonForegroundPointerOver", p.AccentBright);
        Set(r, "ButtonForegroundPressed", p.AccentDim);
        Set(r, "TextControlForeground", p.Accent);
        Set(r, "TextControlForegroundPointerOver", p.Accent);
        Set(r, "TextControlForegroundFocused", p.Accent);
        Set(r, "ExpanderHeaderForeground", p.Accent);

        // Toggles and selection highlights follow the WPF-UI accent.
        ApplicationAccentColorManager.Apply(p.WpfUiAccent, ApplicationTheme.Dark);

        SettingsService.Current.Theme = id;
        SettingsService.Save();
        ThemeChanged?.Invoke();
    }

    public static void Toggle() => Apply(Current.Id == "neon" ? "gold" : "neon");

    // Mutate the existing brush in place so every DynamicResource consumer updates.
    private static void Set(ResourceDictionary r, string key, Color c)
    {
        if (r[key] is SolidColorBrush { IsFrozen: false } brush)
            brush.Color = c;
        else
            r[key] = new SolidColorBrush(c);
    }

    private static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
