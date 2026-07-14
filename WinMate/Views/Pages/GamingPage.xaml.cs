using System.Windows;
using System.Windows.Controls;
using WinMate.Data;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class GamingPage : Page
{
    public GamingPage()
    {
        InitializeComponent();
        UiHelpers.PreparePageHost(this);
        EnableAllButton.Icon = UiFactory.IconElement("rocket", "OnPrimaryBrush");
        TweakList.Load([], GamingCatalog.All); // flat list, no category headers
    }

    private async void EnableAllButton_Click(object sender, RoutedEventArgs e)
    {
        EnableAllButton.IsEnabled = false;
        await TweakList.ApplyAllAsync();
        EnableAllButton.IsEnabled = true;
    }
}
