using System.Windows;
using WinMate.Services;

namespace WinMate;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Apply the saved accent palette (gold or neon blue).
        ThemeService.Apply(SettingsService.Current.Theme);
    }
}
