using System.Windows;
using WinMate.Services;

namespace WinMate;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Apply the saved Overdrive low-light calibration.
        ThemeService.Apply(SettingsService.Current.Theme);
    }
}
