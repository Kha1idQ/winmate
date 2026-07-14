using System.Windows;
using System.Windows.Controls;
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
        UiHelpers.ClearLibraryPlates(this);
        LocalizationService.Apply(SettingsService.Current.Language, this);

        LogoMark.Content = UiFactory.Icon("winmate-mark", 19, "AccentBrush");
        ThemeButton.Icon = UiFactory.IconElement("sun", "TextSecondaryBrush");
        LangButton.Icon = UiFactory.IconElement("language", "TextSecondaryBrush");

        BuildNavItem(NavHome, "home", "Nav_Home");
        BuildNavItem(NavApps, "apps", "Nav_Apps");
        BuildNavItem(NavManager, "download", "Nav_Manager");
        BuildNavItem(NavTweaks, "tweaks", "Nav_Tweaks");
        BuildNavItem(NavGaming, "gamepad", "Nav_Gaming");

        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"WinMate v{version?.ToString(3)}";

        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));

        // Log lines can arrive from background threads — marshal to UI thread.
        LogService.LineWritten += line => Dispatcher.Invoke(() =>
        {
            LogBox.AppendText(line + Environment.NewLine);
            LogBox.ScrollToEnd();
            LogExpander.IsExpanded = true;
        });
    }

    // Nav icons live in the item's content (not its Icon slot) so their stroke can
    // bind to the item's Foreground and follow the selected / hover state.
    private static void BuildNavItem(NavigationViewItem item, string iconName, string textKey)
    {
        var icon = UiFactory.Icon(iconName, 22, brushKey: null);

        var label = new System.Windows.Controls.TextBlock
        {
            FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14, 0, 0, 0),
        };
        label.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, textKey);

        var panel = new System.Windows.Controls.StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(icon);
        panel.Children.Add(label);
        item.Content = panel;
    }

    private void LangButton_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService.Toggle(this);
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        // Brushes mutate in place (DynamicResource), so the switch is instant.
        ThemeService.Toggle();
    }
}

// Creates page instances for the NavigationView (no dependency injection needed).
public class SimplePageProvider : Wpf.Ui.Abstractions.INavigationViewPageProvider
{
    public object? GetPage(Type pageType) => Activator.CreateInstance(pageType);
}
