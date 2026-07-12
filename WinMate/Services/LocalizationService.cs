using System.Windows;

namespace WinMate.Services;

public static class LocalizationService
{
    public static string CurrentLanguage { get; private set; } = "en";

    // Pick the right text for the current language (used by catalog items later).
    public static string Pick(string en, string ar) => CurrentLanguage == "ar" ? ar : en;

    public static void Apply(string lang, Window window)
    {
        CurrentLanguage = lang;

        var newDict = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Resources/Strings.{lang}.xaml")
        };

        var dicts = Application.Current.Resources.MergedDictionaries;
        var oldDict = dicts.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains("/Resources/Strings.") == true);
        if (oldDict != null)
            dicts.Remove(oldDict);
        dicts.Add(newDict);

        window.FlowDirection = lang == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        SettingsService.Current.Language = lang;
        SettingsService.Save();
    }

    public static void Toggle(Window window) =>
        Apply(CurrentLanguage == "ar" ? "en" : "ar", window);
}
