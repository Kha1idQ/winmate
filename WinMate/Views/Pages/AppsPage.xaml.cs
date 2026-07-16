using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;
using Image = System.Windows.Controls.Image;

namespace WinMate.Views.Pages;

public partial class AppsPage : Page
{
    private readonly HashSet<AppItem> _selected = [];
    private readonly HashSet<AppItem> _installed = [];
    private readonly Dictionary<AppItem, CheckBox> _boxes = [];
    private readonly Dictionary<AppItem, Border> _cells = [];
    private readonly List<(string Category, Border Panel)> _categoryPanels = [];
    private bool _installing;

    public AppsPage()
    {
        InitializeComponent();
        UiHelpers.PreparePageHost(this);
        SearchBox.Icon = UiFactory.IconElement("search", "TextSecondaryBrush", 18);
        InstallButton.Icon = UiFactory.IconElement("download", "AccentBrush");
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
                Icon = UiFactory.IconElement(bundle.Icon, "AccentBrush"),
                Margin = new Thickness(0, 0, 12, 0),
            };
            button.SetResourceReference(FrameworkElement.StyleProperty, "OverdriveSecondaryButtonStyle");
            button.Click += (_, _) => SelectBundle(bundle);
            BundlesPanel.Children.Add(button);
        }
    }

    // Ticks every app in the bundle that isn't already installed.
    private void SelectBundle(AppBundle bundle)
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
        _cells.Clear();
        _categoryPanels.Clear();

        foreach (var category in AppCatalog.Categories)
        {
            var header = new TextBlock
            {
                FontSize = 21,
                FontWeight = FontWeights.Bold,
                FontFamily = (System.Windows.Media.FontFamily)FindResource("DisplayFont"),
                VerticalAlignment = VerticalAlignment.Center,
            };
            header.SetResourceReference(TextBlock.TextProperty, $"Cat_{category}");
            header.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

            var grid = new UniformGrid { Columns = 3 };
            foreach (var app in AppCatalog.All.Where(a => a.Category == category))
                grid.Children.Add(BuildAppCell(app));

            var headerRow = new Grid { Margin = new Thickness(0, 0, 0, 16) };
            headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var rule = new Rectangle { Height = 1, Margin = new Thickness(16, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            rule.SetResourceReference(Shape.FillProperty, "CardBorderBrush");
            Grid.SetColumn(header, 0);
            Grid.SetColumn(rule, 1);
            headerRow.Children.Add(header);
            headerRow.Children.Add(rule);

            var inner = new StackPanel();
            inner.Children.Add(headerRow);
            inner.Children.Add(grid);

            // One containment layer: app cells carry the interaction boundary;
            // the category is only a ruled bank, not a card around cards.
            var panel = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 16),
                Margin = new Thickness(0, 0, 0, 20),
                Child = inner,
            };
            panel.SetResourceReference(Border.BorderBrushProperty, "CardBorderBrush");

            CatalogPanel.Children.Add(panel);
            _categoryPanels.Add((category, panel));
        }
    }

    // One app = a bordered cell holding a checkbox, its real icon, name and status.
    private Border BuildAppCell(AppItem app)
    {
        var box = new CheckBox
        {
            Content = BuildAppLabel(app, installed: false),
            Tag = app,
            MinHeight = 44,
            VerticalAlignment = VerticalAlignment.Center,
        };
        box.Checked += AppBox_Changed;
        box.Unchecked += AppBox_Changed;
        _boxes[app] = box;

        var cell = new Border
        {
            CornerRadius = new CornerRadius(0),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 0, 12, 12),
            MinHeight = 56,
            Child = box,
        };
        cell.SetResourceReference(Border.BackgroundProperty, "SurfaceRaisedBrush");
        cell.SetResourceReference(Border.BorderBrushProperty, "CardBorderBrush");
        _cells[app] = cell;
        return cell;
    }

    // Icon + name (+ Installed chip). Icons are embedded resources named after the
    // winget id with non-alphanumerics replaced by "_" (see Assets\AppIcons).
    private object BuildAppLabel(AppItem app, bool installed)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var iconFile = Regex.Replace(app.WingetId, "[^A-Za-z0-9]", "_") + ".png";
        try
        {
            panel.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/WinMate;component/Assets/AppIcons/{iconFile}")),
                Width = 22,
                Height = 22,
                Margin = new Thickness(0, 0, 12, 0),
            });
        }
        catch
        {
            // No icon bundled for this app — name only.
        }

        var nameText = new TextBlock
        {
            Text = LocalizationService.Pick(app.NameEn, app.NameAr),
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
        };
        nameText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
        panel.Children.Add(nameText);

        if (installed)
        {
            var chip = UiFactory.Chip("Apps_Installed", "Success");
            chip.Margin = new Thickness(12, 0, 0, 0);
            panel.Children.Add(chip);
        }

        return panel;
    }

    // Filters apps by name (both languages) or winget id. A category panel hides
    // when nothing inside it matches.
    public void Filter(string query)
    {
        query = query?.Trim() ?? "";

        foreach (var (app, cell) in _cells)
            cell.Visibility = SearchMatch.Matches(query, app.NameEn, app.NameAr, app.WingetId)
                ? Visibility.Visible : Visibility.Collapsed;

        foreach (var (category, panel) in _categoryPanels)
        {
            var anyVisible = AppCatalog.All
                .Where(a => a.Category == category)
                .Any(a => _cells[a].Visibility == Visibility.Visible);
            panel.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Filter(SearchBox.Text);
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
