using System.Windows;
using System.Windows.Controls;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"WinMate v{version?.ToString(3)}";
    }

    private async void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        RestoreButton.IsEnabled = false;
        await RestorePointService.CreateAsync();
        RestoreButton.IsEnabled = true;
    }
}
