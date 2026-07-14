using System.Windows;
using System.Windows.Controls;
using WinMate.Models;
using WinMate.Services;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace WinMate.Views.Controls;

// Renders any tweak catalog as design-system cards: reversible tweaks get a
// ToggleSwitch that reflects the REAL system state, one-shot actions get a
// confirm button. Used by both the Tweaks page and the Gaming page.
public partial class TweakListControl : UserControl
{
    private readonly Dictionary<Tweak, Wpf.Ui.Controls.ToggleSwitch> _toggles = [];
    private readonly List<(Tweak Tweak, FrameworkElement Card)> _cards = [];
    private readonly List<(string Category, FrameworkElement Header)> _headers = [];
    private IReadOnlyList<Tweak> _tweaks = [];

    // True while WE set IsChecked from code — the Checked/Unchecked handlers
    // must ignore those, otherwise loading states would apply tweaks.
    private bool _suppressEvents;

    public TweakListControl()
    {
        InitializeComponent();
    }

    // categories may be empty => flat list without headers.
    // Header resource keys are $"TweakCat_{category}".
    public void Load(string[] categories, IReadOnlyList<Tweak> tweaks)
    {
        _tweaks = tweaks;
        BuildList(categories);
        _ = LoadStatesAsync();
    }

    // Applies every reversible tweak that is currently off (Gaming "enable all").
    public async Task ApplyAllAsync()
    {
        await RestorePointService.OfferOnceAsync();
        foreach (var (tweak, toggle) in _toggles)
        {
            if (toggle.IsChecked != true)
                await ApplyTweakAsync(tweak, toggle, apply: true);
        }
    }

    private void BuildList(string[] categories)
    {
        ListPanel.Children.Clear();
        _toggles.Clear();
        _cards.Clear();
        _headers.Clear();

        if (categories.Length == 0)
        {
            foreach (var tweak in _tweaks)
            {
                var card = BuildCard(tweak);
                _cards.Add((tweak, card));
                ListPanel.Children.Add(card);
            }
            return;
        }

        foreach (var category in categories)
        {
            var header = new TextBlock
            {
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 12, 0, 14),
            };
            header.SetResourceReference(TextBlock.TextProperty, $"TweakCat_{category}");
            header.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
            _headers.Add((category, header));
            ListPanel.Children.Add(header);

            foreach (var tweak in _tweaks.Where(t => t.Category == category))
            {
                var card = BuildCard(tweak);
                _cards.Add((tweak, card));
                ListPanel.Children.Add(card);
            }
        }
    }

    // Filters cards by tweak name/description (both languages). A category header
    // hides when none of its cards match.
    public void Filter(string query)
    {
        query = query?.Trim() ?? "";

        foreach (var (tweak, card) in _cards)
            card.Visibility = SearchMatch.Matches(query, tweak.NameEn, tweak.NameAr, tweak.DescEn, tweak.DescAr)
                ? Visibility.Visible : Visibility.Collapsed;

        foreach (var (category, header) in _headers)
        {
            var anyVisible = _cards.Any(c => c.Tweak.Category == category && c.Card.Visibility == Visibility.Visible);
            header.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Border BuildCard(Tweak tweak)
    {
        // Title row: name + any status chips.
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Children.Add(UiFactory.Title(LocalizationService.Pick(tweak.NameEn, tweak.NameAr), 17));

        var desc = UiFactory.Description(LocalizationService.Pick(tweak.DescEn, tweak.DescAr));

        var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textPanel.Children.Add(titleRow);
        textPanel.Children.Add(desc);

        // Chips sit under the description so long names never collide with them.
        var chipRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        if (tweak.RequiresRestart)
            chipRow.Children.Add(UiFactory.Chip("Chip_RestartRequired"));
        if (tweak.IsAdvanced)
        {
            var advanced = UiFactory.Chip("Chip_Advanced", "AccentAlt");
            advanced.Margin = new Thickness(chipRow.Children.Count > 0 ? 8 : 0, 0, 0, 0);
            chipRow.Children.Add(advanced);
        }
        if (chipRow.Children.Count > 0)
            textPanel.Children.Add(chipRow);

        UIElement control = tweak.IsReversible ? BuildToggle(tweak) : BuildRunButton(tweak);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        if (tweak.Icon is { } symbol)
        {
            var tile = UiFactory.IconTile(symbol, "Accent", size: 44, iconSize: 18);
            tile.Margin = new Thickness(0, 0, 16, 0);
            Grid.SetColumn(tile, 0);
            grid.Children.Add(tile);
        }

        Grid.SetColumn(textPanel, 1);
        Grid.SetColumn(control, 2);
        grid.Children.Add(textPanel);
        grid.Children.Add(control);

        return UiFactory.Card(grid);
    }

    private Wpf.Ui.Controls.ToggleSwitch BuildToggle(Tweak tweak)
    {
        var toggle = new Wpf.Ui.Controls.ToggleSwitch
        {
            IsEnabled = false, // enabled once the real state is loaded
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0),
        };
        toggle.Checked += (_, _) => OnToggle(tweak, toggle, apply: true);
        toggle.Unchecked += (_, _) => OnToggle(tweak, toggle, apply: false);
        _toggles[tweak] = toggle;
        return toggle;
    }

    private Wpf.Ui.Controls.Button BuildRunButton(Tweak tweak)
    {
        var button = new Wpf.Ui.Controls.Button
        {
            Icon = UiFactory.IconElement("warning", "TextPrimaryBrush"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0),
            Height = 40,
        };
        button.SetResourceReference(ContentControl.ContentProperty, "Tweaks_Run");
        button.Click += async (_, _) => await RunOneShotAsync(tweak, button);
        return button;
    }

    private async Task LoadStatesAsync()
    {
        foreach (var (tweak, toggle) in _toggles)
        {
            var applied = await TweakEngine.IsAppliedAsync(tweak);
            _suppressEvents = true;
            toggle.IsChecked = applied;
            _suppressEvents = false;
            toggle.IsEnabled = true;
        }
    }

    private async void OnToggle(Tweak tweak, Wpf.Ui.Controls.ToggleSwitch toggle, bool apply)
    {
        if (_suppressEvents)
            return;

        await RestorePointService.OfferOnceAsync();
        await ApplyTweakAsync(tweak, toggle, apply);
    }

    private async Task ApplyTweakAsync(Tweak tweak, Wpf.Ui.Controls.ToggleSwitch toggle, bool apply)
    {
        toggle.IsEnabled = false;
        try
        {
            LogService.Write($"{FindResource(apply ? "Tweaks_LogApplying" : "Tweaks_LogUndoing")}: {tweak.NameEn}");

            if (apply)
                await TweakEngine.ApplyAsync(tweak);
            else
                await TweakEngine.UndoAsync(tweak);

            if (tweak.RestartExplorer)
                await TweakEngine.RestartExplorerAsync();

            if (tweak.RequiresRestart)
                LogService.Write((string)FindResource("Tweaks_RestartNeeded"));
        }
        catch (Exception ex)
        {
            LogService.Write($"✗ {tweak.Id}: {ex.Message}");
        }

        // Re-read reality — the toggle shows what the system actually says.
        var actual = await TweakEngine.IsAppliedAsync(tweak);
        _suppressEvents = true;
        toggle.IsChecked = actual;
        _suppressEvents = false;
        toggle.IsEnabled = true;

        LogService.Write($"✓ {tweak.Id} => {(actual ? "ON" : "OFF")}");
    }

    private async Task RunOneShotAsync(Tweak tweak, Wpf.Ui.Controls.Button button)
    {
        var details = LocalizationService.Pick(tweak.DescEn, tweak.DescAr)
                      + "\n\n" + (string)FindResource("Tweaks_ConfirmIrreversible");
        if (tweak.Id == "debloat_appx")
            details += "\n\n" + string.Join("\n", Data.TweakCatalog.BloatApps);

        var box = new Wpf.Ui.Controls.MessageBox
        {
            Title = LocalizationService.Pick(tweak.NameEn, tweak.NameAr),
            Content = details,
            PrimaryButtonText = (string)FindResource("Tweaks_Confirm"),
            CloseButtonText = (string)FindResource("Tweaks_Cancel"),
        };
        if (await box.ShowDialogAsync() != Wpf.Ui.Controls.MessageBoxResult.Primary)
            return;

        await RestorePointService.OfferOnceAsync();

        button.IsEnabled = false;
        try
        {
            LogService.Write($"{FindResource("Tweaks_LogApplying")}: {tweak.NameEn}");
            await TweakEngine.ApplyAsync(tweak);
            LogService.Write($"✓ {tweak.Id} done");
        }
        catch (Exception ex)
        {
            LogService.Write($"✗ {tweak.Id}: {ex.Message}");
        }
        button.IsEnabled = true;
    }
}
