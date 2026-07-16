using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using WinMate.Services;
using WinMate.Views.Pages;

namespace WinMate;

public partial class MainWindow : FluentWindow
{
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(20) };

    public MainWindow()
    {
        InitializeComponent();

        RootNavigation.SetPageProviderService(new SimplePageProvider());
        UiHelpers.ClearLibraryPlates(this);
        LocalizationService.Apply(SettingsService.Current.Language, this);

        LogoMark.Content = UiFactory.Icon("winmate-mark", 19, "AccentBrush");
        LangButton.Icon = UiFactory.IconElement("language", "TextSecondaryBrush");

        BuildNavItem(NavHome, "home", "Nav_Home");
        BuildNavItem(NavApps, "apps", "Nav_Apps");
        BuildNavItem(NavManager, "download", "Nav_Manager");
        BuildNavItem(NavTweaks, "tweaks", "Nav_Tweaks");
        BuildNavItem(NavGaming, "gamepad", "Nav_Gaming");

        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"WinMate v{version?.ToString(3)}";

        UpdateClock();
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
        Closed += (_, _) => _clock.Stop();

        Loaded += (_, _) => RootNavigation.Navigate(typeof(HomePage));

        // Log lines can arrive from background threads — marshal to UI thread.
        LogService.LineWritten += line => Dispatcher.Invoke(() =>
        {
            LogBox.AppendText(line + Environment.NewLine);
            LogBox.ScrollToEnd();
            LogExpander.IsExpanded = true;
        });
    }

    // The compact command rail uses a vertical icon/label lockup. Icon strokes
    // inherit the NavigationViewItem foreground so selected and hover states stay live.
    private static void BuildNavItem(NavigationViewItem item, string iconName, string textKey)
    {
        var icon = UiFactory.Icon(iconName, 20, brushKey: null);
        icon.HorizontalAlignment = HorizontalAlignment.Center;

        var label = new System.Windows.Controls.TextBlock
        {
            Width = 82,
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            FontFamily = (System.Windows.Media.FontFamily)Application.Current.FindResource("BodyFont"),
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0),
        };
        label.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, textKey);

        var panel = new System.Windows.Controls.StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(icon);
        panel.Children.Add(label);
        item.Content = panel;
    }

    private void UpdateClock() => ClockText.Text = DateTime.Now.ToString("HH:mm");

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
