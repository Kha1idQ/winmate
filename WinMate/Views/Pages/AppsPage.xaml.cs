using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class AppsPage : Page
{
    private readonly HashSet<AppItem> _selected = [];
    private readonly HashSet<AppItem> _installed = [];
    private readonly Dictionary<AppItem, CheckBox> _boxes = [];
    private readonly List<(string Category, TextBlock Header, WrapPanel Wrap)> _categoryUi = [];
    private bool _installing;

    public AppsPage()
    {
        InitializeComponent();
        UiHelpers.DisableHostScrolling(this);
        BuildCatalog();
        BuildBundles();
        UpdateInstallButton();
        _ = LoadInstalledAsync();
    }

    private void BuildBundles()
    {
        foreach (var bundle in AppBundleCatalog.All)
        {
            var button = new Wpf.Ui.Controls.Button
            {
                Content = LocalizationService.Pick(bundle.NameEn, bundle.NameAr),
                Icon = new Wpf.Ui.Controls.SymbolIcon(bundle.Icon),
                Margin = new Thickness(0, 0, 8, 0),
            };
            button.Click += (_, _) => SelectBundle(bundle);
            BundlesPanel.Children.Add(button);
        }
    }

    // Ticks every app in the bundle that isn't already installed.
    private void SelectBundle(Models.AppBundle bundle)
    {
        foreach (var id in bundle.WingetIds)
        {
            var app = AppCatalog.All.FirstOrDefault(a => a.WingetId == id);
            if (app is not null && _boxes.TryGetValue(app, out var box) && box.IsEnabled)
                box.IsChecked = true;
        }
    }

    private void BuildCatalog()
    {
        CatalogPanel.Children.Clear();
        _boxes.Clear();
        _categoryUi.Clear();

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
            header.SetResourceReference(TextBlock.ForegroundProperty, "GoldBrush");
            CatalogPanel.Children.Add(header);

            var wrap = new WrapPanel { ItemWidth = 230 };
            foreach (var app in AppCatalog.All.Where(a => a.Category == category))
            {
                var box = new CheckBox
                {
                    Content = BuildAppLabel(app, installed: false),
                    Tag = app,
                    Margin = new Thickness(0, 4, 12, 4),
                };
                box.Checked += AppBox_Changed;
                box.Unchecked += AppBox_Changed;
                _boxes[app] = box;
                wrap.Children.Add(box);
            }
            CatalogPanel.Children.Add(wrap);
            _categoryUi.Add((category, header, wrap));
        }
    }

    // Filters apps by name (both languages) or winget id. A category header and
    // its row hide when nothing in it matches.
    public void Filter(string query)
    {
        query = query?.Trim() ?? "";

        foreach (var (app, box) in _boxes)
            box.Visibility = SearchMatch.Matches(query, app.NameEn, app.NameAr, app.WingetId)
                ? Visibility.Visible : Visibility.Collapsed;

        foreach (var (category, header, wrap) in _categoryUi)
        {
            var anyVisible = AppCatalog.All
                .Where(a => a.Category == category)
                .Any(a => _boxes[a].Visibility == Visibility.Visible);
            header.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;
            wrap.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    // Icon + name side by side. Icons are embedded resources named after the
    // winget id with non-alphanumerics replaced by "_" (see Assets\AppIcons).
    private object BuildAppLabel(AppItem app, bool installed)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var iconFile = Regex.Replace(app.WingetId, "[^A-Za-z0-9]", "_") + ".png";
        try
        {
            panel.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/AppIcons/{iconFile}")),
                Width = 20,
                Height = 20,
                Margin = new Thickness(0, 0, 8, 0),
            });
        }
        catch
        {
            // No icon bundled for this app — name only.
        }

        var nameText = new TextBlock
        {
            Text = LocalizationService.Pick(app.NameEn, app.NameAr),
            VerticalAlignment = VerticalAlignment.Center,
        };
        nameText.SetResourceReference(TextBlock.ForegroundProperty, "GoldBrush");
        panel.Children.Add(nameText);

        if (installed)
        {
            var badge = new TextBlock
            {
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
            badge.SetResourceReference(TextBlock.ForegroundProperty, "GoldBrush");
            badge.SetResourceReference(TextBlock.TextProperty, "Apps_Installed");
            panel.Children.Add(badge);
        }

        return panel;
    }

    private async Task LoadInstalledAsync()
    {
        try
        {
            var installed = await WingetService.GetInstalledIdsAsync();
            foreach (var (app, box) in _boxes)
            {
                if (installed.Contains(app.WingetId))
                    MarkInstalled(app, box);
            }
        }
        catch
        {
            // winget missing or listing failed — catalog still usable without badges.
        }
    }

    private void MarkInstalled(AppItem app, CheckBox box)
    {
        _installed.Add(app);
        box.IsChecked = false;
        box.IsEnabled = false;
        box.Content = BuildAppLabel(app, installed: true);
        _selected.Remove(app);
        UpdateInstallButton();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Filter(SearchBox.Text);
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
        InstallButton.IsEnabled = _selected.Count > 0 && !_installing;
    }

    private async void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (_installing || _selected.Count == 0)
            return;

        _installing = true;
        UpdateInstallButton();
        foreach (var box in _boxes.Values)
            box.IsEnabled = false;

        var queue = _selected.ToList();
        var failed = new List<AppItem>();

        foreach (var app in queue)
        {
            LogService.Write($"=== {FindResource("Apps_LogInstalling")}: {app.NameEn} ({app.WingetId}) ===");
            try
            {
                var ok = await WingetService.InstallAsync(app, LogService.WriteRaw);

                if (ok)
                {
                    LogService.Write($"✓ {app.NameEn} — {FindResource("Apps_LogDone")}");
                    MarkInstalled(app, _boxes[app]);
                }
                else
                {
                    LogService.Write($"✗ {app.NameEn} — {FindResource("Apps_LogFailed")}");
                    failed.Add(app);
                }
            }
            catch (Exception ex)
            {
                // winget missing or process failure — keep going with the rest.
                LogService.Write($"✗ {app.NameEn} — {ex.Message}");
                failed.Add(app);
            }
        }

        _installing = false;
        // Re-enable every box except the ones now installed.
        foreach (var (app, box) in _boxes)
            box.IsEnabled = !_installed.Contains(app);
        UpdateInstallButton();

        if (failed.Count > 0)
            LogService.Write($"{FindResource("Apps_LogFailedSummary")}: {string.Join(", ", failed.Select(a => a.NameEn))}");
    }
}
