using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace WinMate;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Gold accent — toggles, primary buttons and highlights.
        ApplicationAccentColorManager.Apply(
            Color.FromRgb(0xD4, 0xAF, 0x37),
            ApplicationTheme.Dark);
    }
}
