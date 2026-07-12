using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using WinMate.Views.Pages;

namespace WinMate;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        RootNavigation.SetPageProviderService(new SimplePageProvider());
        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));
    }
}

// Creates page instances for the NavigationView (no dependency injection needed).
public class SimplePageProvider : Wpf.Ui.Abstractions.INavigationViewPageProvider
{
    public object? GetPage(Type pageType) => Activator.CreateInstance(pageType);
}
