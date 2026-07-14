using System.Windows;
using System.Windows.Controls;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class ManagerPage : Page
{
    private readonly List<(InstalledApp App, FrameworkElement Card)> _cards = [];
    private IReadOnlyList<InstalledApp> _apps = [];
    private SystemPackageIndex? _index;
    private bool _busy;

    public ManagerPage()
    {
        InitializeComponent();
        UiHelpers.PreparePageHost(this);
        SearchBox.Icon = UiFactory.IconElement("search", "TextSecondaryBrush", 18);
        RefreshButton.Icon = UiFactory.IconElement("undo", "TextPrimaryBrush");
        UpdateAllButton.Icon = UiFactory.IconElement("download", "OnPrimaryBrush");

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        SetBusy(true);
        StatsText.SetResourceReference(TextBlock.TextProperty, "Manager_Loading");
        ListPanel.Children.Clear();
        _cards.Clear();

        try
        {
            // Ask Windows what each package actually is, then ask winget what's installed.
            _index = await SystemPackageIndex.BuildAsync();
            _apps = await WingetManager.GetInstalledAsync();
        }
        catch (Exception ex)
        {
            LogService.Write($"✗ {ex.Message}");
            _apps = [];
        }

        // Anything with an update waiting goes to the top.
        foreach (var app in _apps.OrderByDescending(a => a.HasUpdate).ThenBy(a => a.Name))
        {
            var card = BuildCard(app);
            _cards.Add((app, card));
            ListPanel.Children.Add(card);
        }

        var updates = _apps.Count(a => a.HasUpdate);
        StatsText.Text = $"{_apps.Count} {FindResource("Manager_Installed")} · " +
                         $"{updates} {FindResource("Manager_UpdatesAvailable")}";

        UpdateAllButton.Content = $"{FindResource("Manager_UpdateAll")} ({updates})";
        SetBusy(false);
        UpdateAllButton.IsEnabled = updates > 0;

        Filter(SearchBox.Text);
    }

    private Border BuildCard(InstalledApp app)
    {
        var kind = _index?.Classify(app) ?? PackageClass.Normal;

        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Children.Add(UiFactory.Title(app.Name, 17));
        if (app.HasUpdate)
        {
            var chip = UiFactory.Chip("Manager_UpdateChip");
            chip.Margin = new Thickness(10, 0, 0, 0);
            titleRow.Children.Add(chip);
        }
        if (kind != PackageClass.Normal)
        {
            // Runtimes, drivers and Windows components look like ordinary apps
            // here — flag them before someone removes one by mistake.
            var chip = UiFactory.Chip(
                kind == PackageClass.Protected ? "Manager_ProtectedChip" : "Manager_SystemChip",
                "Warning");
            chip.Margin = new Thickness(10, 0, 0, 0);
            titleRow.Children.Add(chip);
        }

        // "Git.Git · 2.54.0 → 2.55.0" — the arrow only when an update exists.
        var detail = app.HasUpdate
            ? $"{app.Id} · {app.Version} → {app.Available}"
            : $"{app.Id} · {app.Version}";

        var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textPanel.Children.Add(titleRow);
        textPanel.Children.Add(UiFactory.Description(detail, 13));

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0),
        };

        if (app.HasUpdate)
        {
            var update = UiFactory.PrimaryButton("download");
            update.Height = 40;
            update.SetResourceReference(ContentControl.ContentProperty, "Manager_Update");
            update.Click += async (_, _) => await UpdateOneAsync(app, update);
            actions.Children.Add(update);
        }

        var uninstall = new Wpf.Ui.Controls.Button
        {
            Icon = UiFactory.IconElement("close", "TextPrimaryBrush", 18),
            Height = 40,
            Margin = new Thickness(app.HasUpdate ? 10 : 0, 0, 0, 0),
        };
        uninstall.SetResourceReference(Control.ForegroundProperty,
            kind == PackageClass.Normal ? "TextPrimaryBrush" : "WarningBrush");
        uninstall.SetResourceReference(ContentControl.ContentProperty, "Manager_Uninstall");

        if (kind == PackageClass.Protected)
        {
            // Windows refuses to remove these, so don't pretend we can try.
            uninstall.IsEnabled = false;
            uninstall.SetResourceReference(ToolTipProperty, "Manager_ProtectedTooltip");
        }
        else
        {
            uninstall.Click += async (_, _) => await UninstallAsync(app, kind);
        }
        actions.Children.Add(uninstall);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(textPanel, 0);
        Grid.SetColumn(actions, 1);
        grid.Children.Add(textPanel);
        grid.Children.Add(actions);

        return UiFactory.Card(grid);
    }

    private async Task UpdateOneAsync(InstalledApp app, Wpf.Ui.Controls.Button button)
    {
        SetBusy(true);
        LogService.Write($"=== {FindResource("Manager_Update")}: {app.Name} ({app.Id}) ===");

        var ok = await WingetManager.UpgradeAsync(app, LogService.WriteRaw);
        LogService.Write(ok
            ? $"✓ {app.Name} — {FindResource("Manager_LogUpdated")}"
            : $"✗ {app.Name} — {FindResource("Apps_LogFailed")}");

        await LoadAsync();
    }

    private async Task UninstallAsync(InstalledApp app, PackageClass kind)
    {
        var body = $"{app.Name}\n{app.Id}\n\n{FindResource("Manager_UninstallConfirm")}";
        if (kind == PackageClass.System)
            body = $"{FindResource("Manager_SystemWarning")}\n\n" + body;

        var box = new Wpf.Ui.Controls.MessageBox
        {
            Title = (string)FindResource("Manager_Uninstall"),
            Content = body,
            PrimaryButtonText = (string)FindResource("Manager_Uninstall"),
            CloseButtonText = (string)FindResource("Tweaks_Cancel"),
        };
        if (await box.ShowDialogAsync() != Wpf.Ui.Controls.MessageBoxResult.Primary)
            return;

        SetBusy(true);
        LogService.Write($"=== {FindResource("Manager_Uninstall")}: {app.Name} ({app.Id}) ===");

        // Edge blocks the normal uninstall — desktop install needs force removal,
        // the Store package needs Remove-AppxPackage. UninstallEdgeAsync routes both.
        var ok = SystemPackageIndex.IsEdge(app)
            ? await WingetManager.UninstallEdgeAsync(app, LogService.WriteRaw)
            : await WingetManager.UninstallAsync(app, LogService.WriteRaw);

        LogService.Write(ok
            ? $"✓ {app.Name} — {FindResource("Manager_LogUninstalled")}"
            : $"✗ {app.Name} — {FindResource("Apps_LogFailed")}");

        await LoadAsync();
    }

    private async void UpdateAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (_busy)
            return;

        SetBusy(true);
        LogService.Write($"=== {FindResource("Manager_UpdateAll")} ===");

        await WingetManager.UpgradeAllAsync(LogService.WriteRaw);
        LogService.Write($"✓ {FindResource("Manager_LogUpdatedAll")}");

        await LoadAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_busy)
            await LoadAsync();
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        RefreshButton.IsEnabled = !busy;
        UpdateAllButton.IsEnabled = !busy && _apps.Any(a => a.HasUpdate);
        ListPanel.IsEnabled = !busy;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => Filter(SearchBox.Text);

    private void Filter(string query)
    {
        foreach (var (app, card) in _cards)
            card.Visibility = SearchMatch.Matches(query, app.Name, app.Id)
                ? Visibility.Visible : Visibility.Collapsed;
    }
}
