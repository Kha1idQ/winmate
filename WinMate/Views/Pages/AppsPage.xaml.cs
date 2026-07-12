using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class AppsPage : Page
{
    private readonly HashSet<AppItem> _selected = [];

    public AppsPage()
    {
        InitializeComponent();
        UiHelpers.DisableHostScrolling(this);
        BuildCatalog();
        UpdateInstallButton();
    }

    private void BuildCatalog()
    {
        CatalogPanel.Children.Clear();

        foreach (var category in AppCatalog.Categories)
        {
            var header = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 14, 0, 8),
            };
            // SetResourceReference keeps the header live-updating on language switch.
            header.SetResourceReference(TextBlock.TextProperty, $"Cat_{category}");
            CatalogPanel.Children.Add(header);

            var wrap = new WrapPanel { ItemWidth = 230 };
            foreach (var app in AppCatalog.All.Where(a => a.Category == category))
            {
                var box = new CheckBox
                {
                    Content = BuildAppLabel(app),
                    Tag = app,
                    Margin = new Thickness(0, 4, 12, 4),
                };
                box.Checked += AppBox_Changed;
                box.Unchecked += AppBox_Changed;
                wrap.Children.Add(box);
            }
            CatalogPanel.Children.Add(wrap);
        }
    }

    // Icon + name side by side. Icons are embedded resources named after the
    // winget id with non-alphanumerics replaced by "_" (see Assets\AppIcons).
    private object BuildAppLabel(AppItem app)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var iconFile = Regex.Replace(app.WingetId, "[^A-Za-z0-9]", "_") + ".png";
        try
        {
            var image = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/AppIcons/{iconFile}")),
                Width = 20,
                Height = 20,
                Margin = new Thickness(0, 0, 8, 0),
            };
            panel.Children.Add(image);
        }
        catch
        {
            // No icon bundled for this app — name only.
        }

        panel.Children.Add(new TextBlock
        {
            Text = LocalizationService.Pick(app.NameEn, app.NameAr),
            VerticalAlignment = VerticalAlignment.Center,
        });
        return panel;
    }

    private void AppBox_Changed(object sender, RoutedEventArgs e)
    {
        var box = (CheckBox)sender;
        var app = (AppItem)box.Tag;

        if (box.IsChecked == true)
            _selected.Add(app);
        else
            _selected.Remove(app);

        UpdateInstallButton();
    }

    private void UpdateInstallButton()
    {
        InstallButton.Content = $"{FindResource("Apps_InstallSelected")} ({_selected.Count})";
        InstallButton.IsEnabled = _selected.Count > 0;
    }

    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        // Real winget installs arrive in Phase 3.
    }
}
