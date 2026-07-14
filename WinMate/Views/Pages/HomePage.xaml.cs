using System.Windows;
using System.Windows.Controls;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;
using Wpf.Ui.Controls;

namespace WinMate.Views.Pages;

public partial class HomePage : Page
{
    // Each profile gets its own accent so the three cards read apart at a glance.
    private static readonly Dictionary<string, string> ProfileAccents = new()
    {
        ["gamer"] = "Accent",
        ["privacy"] = "AccentAlt",
        ["clean_fast"] = "Success",
    };

    public HomePage()
    {
        InitializeComponent();
        UiHelpers.PreparePageHost(this);
        RestoreButton.Icon = UiFactory.IconElement("shield", "TextPrimaryBrush");
        ResetButton.Icon = UiFactory.IconElement("undo", "TextPrimaryBrush");
        BuildProfiles();
    }

    private void BuildProfiles()
    {
        foreach (var profile in ProfileCatalog.All)
            ProfilesPanel.Items.Add(BuildProfileCard(profile));
    }

    private UIElement BuildProfileCard(Profile profile)
    {
        var accent = ProfileAccents.GetValueOrDefault(profile.Id, "Accent");

        var tile = UiFactory.ProfileTile(profile.Tile, size: 64);
        tile.Margin = new Thickness(0, 0, 20, 0);

        var title = UiFactory.Title(LocalizationService.Pick(profile.NameEn, profile.NameAr), 19);
        var desc = UiFactory.Description(LocalizationService.Pick(profile.DescEn, profile.DescAr), 15);

        var countChip = UiFactory.Chip("", accent,
            literalText: $"{profile.TweakIds.Count} {FindResource("Home_ProfileTweakCount")}");
        countChip.HorizontalAlignment = HorizontalAlignment.Left;
        countChip.Margin = new Thickness(0, 10, 0, 0);

        var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textPanel.Children.Add(title);
        textPanel.Children.Add(desc);
        textPanel.Children.Add(countChip);

        var applyButton = UiFactory.PrimaryButton("play", $"{accent}Brush");
        applyButton.Margin = new Thickness(20, 0, 0, 0);
        applyButton.SetResourceReference(ContentControl.ContentProperty, "Home_ProfileApply");
        applyButton.Click += async (_, _) => await ApplyProfileAsync(profile, applyButton);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(tile, 0);
        Grid.SetColumn(textPanel, 1);
        Grid.SetColumn(applyButton, 2);
        grid.Children.Add(tile);
        grid.Children.Add(textPanel);
        grid.Children.Add(applyButton);

        return UiFactory.Card(grid, minHeight: 112);
    }

    private async Task ApplyProfileAsync(Profile profile, Wpf.Ui.Controls.Button button)
    {
        var body = LocalizationService.Pick(profile.DescEn, profile.DescAr)
                   + "\n\n" + (string)FindResource("Home_ProfileConfirmBody")
                   + "\n\n" + ProfileService.DescribeTweaks(profile);

        var box = new Wpf.Ui.Controls.MessageBox
        {
            Title = LocalizationService.Pick(profile.NameEn, profile.NameAr),
            Content = body,
            PrimaryButtonText = (string)FindResource("Home_ProfileConfirmYes"),
            CloseButtonText = (string)FindResource("Tweaks_Cancel"),
        };
        if (await box.ShowDialogAsync() != Wpf.Ui.Controls.MessageBoxResult.Primary)
            return;

        await RestorePointService.OfferOnceAsync();

        button.IsEnabled = false;
        LogService.Write($"=== {LocalizationService.Pick(profile.NameEn, profile.NameAr)} ===");
        var (applied, skipped, failed) = await ProfileService.ApplyAsync(profile);
        LogService.Write(
            $"✓ {LocalizationService.Pick(profile.NameEn, profile.NameAr)}: " +
            $"{applied} {FindResource("Home_ProfileApplied")}, " +
            $"{skipped} {FindResource("Home_ProfileAlreadyOn")}" +
            (failed > 0 ? $", {failed} {FindResource("Home_ProfileFailed")}" : ""));
        button.IsEnabled = true;
    }

    private async void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        RestoreButton.IsEnabled = false;
        await RestorePointService.CreateAsync();
        RestoreButton.IsEnabled = true;
    }

    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var box = new Wpf.Ui.Controls.MessageBox
        {
            Title = (string)FindResource("Home_ResetButton"),
            Content = (string)FindResource("Home_ResetConfirmBody"),
            PrimaryButtonText = (string)FindResource("Home_ResetConfirmYes"),
            CloseButtonText = (string)FindResource("Tweaks_Cancel"),
        };
        if (await box.ShowDialogAsync() != Wpf.Ui.Controls.MessageBoxResult.Primary)
            return;

        ResetButton.IsEnabled = false;
        LogService.Write($"=== {FindResource("Home_ResetButton")} ===");
        var (undone, failed) = await ProfileService.UndoAllAppliedAsync();
        LogService.Write(
            $"✓ {undone} {FindResource("Home_ResetUndone")}" +
            (failed > 0 ? $", {failed} {FindResource("Home_ProfileFailed")}" : ""));
        ResetButton.IsEnabled = true;
    }
}
