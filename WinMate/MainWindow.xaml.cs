using System.Windows;
using Wpf.Ui.Controls;
using WinMate.Services;
using WinMate.Views.Pages;

namespace WinMate;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        RootNavigation.SetPageProviderService(new SimplePageProvider());
        LocalizationService.Apply(SettingsService.Current.Language, this);
        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));

        // Log lines can arrive from background threads — marshal to UI thread.
        LogService.LineWritten += line => Dispatcher.Invoke(() =>
        {
            LogBox.AppendText(line + Environment.NewLine);
            LogBox.ScrollToEnd();
            LogExpander.IsExpanded = true;
        });
    }

    private void LangButton_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService.Toggle(this);
    }
}

// Creates page instances for the NavigationView (no dependency injection needed).
public class SimplePageProvider : Wpf.Ui.Abstractions.INavigationViewPageProvider
{
    public object? GetPage(Type pageType) => Activator.CreateInstance(pageType);
}
