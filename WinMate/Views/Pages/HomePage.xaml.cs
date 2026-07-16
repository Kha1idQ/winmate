using System.Windows;
using System.Windows.Controls;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace WinMate.Views.Pages;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        UiHelpers.PreparePageHost(this);
        RestoreButton.Icon = UiFactory.IconElement("shield", "AccentBrush");
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
        var count = new TextBlock
        {
            Text = $"{profile.TweakIds.Count:00} {FindResource("Home_ProfileActionCount")}",
            FontFamily = (System.Windows.Media.FontFamily)FindResource("MonoFont"),
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
        };
        count.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");

        var title = UiFactory.Title(LocalizationService.Pick(profile.NameEn, profile.NameAr), 21);
        title.Margin = new Thickness(0, 8, 0, 0);
        var desc = UiFactory.Description(LocalizationService.Pick(profile.DescEn, profile.DescAr), 13);

        var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textPanel.Children.Add(count);
        textPanel.Children.Add(title);
        textPanel.Children.Add(desc);

        var applyButton = UiFactory.PrimaryButton("play");
        applyButton.Margin = new Thickness(16, 0, 0, 0);
        applyButton.SetResourceReference(ContentControl.ContentProperty, "Home_ProfileApply");
        applyButton.Click += async (_, _) => await ApplyProfileAsync(profile, applyButton);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(textPanel, 0);
        Grid.SetColumn(applyButton, 1);
        grid.Children.Add(textPanel);
        grid.Children.Add(applyButton);

        var row = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(0, 20, 0, 20),
            MinHeight = 126,
            Child = grid,
        };
        row.SetResourceReference(Border.BorderBrushProperty, "CardBorderBrush");
        return row;
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
