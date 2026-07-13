using System.Windows;
using System.Windows.Controls;
using WinMate.Data;
using WinMate.Models;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        UiHelpers.DisableHostScrolling(this);
        BuildProfiles();
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"WinMate v{version?.ToString(3)}";
    }

    private void BuildProfiles()
    {
        foreach (var profile in ProfileCatalog.All)
            ProfilesPanel.Items.Add(BuildProfileCard(profile));
    }

    private UIElement BuildProfileCard(Profile profile)
    {
        var icon = new Wpf.Ui.Controls.SymbolIcon
        {
            Symbol = profile.Icon,
            FontSize = 28,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 14, 0),
        };
        icon.SetResourceReference(Wpf.Ui.Controls.SymbolIcon.ForegroundProperty, "GoldBrush");

        var title = new TextBlock
        {
            Text = LocalizationService.Pick(profile.NameEn, profile.NameAr),
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
        };
        title.SetResourceReference(TextBlock.ForegroundProperty, "GoldBrush");

        var desc = new TextBlock
        {
            Text = LocalizationService.Pick(profile.DescEn, profile.DescAr),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0),
        };
        desc.SetResourceReference(TextBlock.ForegroundProperty, "GoldDimBrush");

        var countText = new TextBlock
        {
            Text = $"{profile.TweakIds.Count} {FindResource("Home_ProfileTweakCount")}",
            FontSize = 11,
            Margin = new Thickness(0, 4, 0, 0),
        };
        countText.SetResourceReference(TextBlock.ForegroundProperty, "GoldDimBrush");

        var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textPanel.Children.Add(title);
        textPanel.Children.Add(desc);
        textPanel.Children.Add(countText);

        var applyButton = new Wpf.Ui.Controls.Button
        {
            Icon = new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.Play24),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0),
        };
        // Solid accent fill that follows the theme (accent bg + dark text).
        applyButton.SetResourceReference(Control.BackgroundProperty, "GoldBrush");
        applyButton.SetResourceReference(Control.BorderBrushProperty, "GoldBrush");
        applyButton.SetResourceReference(Control.ForegroundProperty, "ApplicationBackgroundBrush");
        applyButton.SetResourceReference(ContentControl.ContentProperty, "Home_ProfileApply");
        applyButton.Click += async (_, _) => await ApplyProfileAsync(profile, applyButton);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(icon, 0);
        Grid.SetColumn(textPanel, 1);
        Grid.SetColumn(applyButton, 2);
        grid.Children.Add(icon);
        grid.Children.Add(textPanel);
        grid.Children.Add(applyButton);

        var card = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 14, 16, 14),
            Margin = new Thickness(0, 0, 0, 10),
            BorderThickness = new Thickness(1),
            Child = grid,
        };
        card.SetResourceReference(Border.BackgroundProperty, "ControlFillColorDefaultBrush");
        card.SetResourceReference(Border.BorderBrushProperty, "ControlStrokeColorDefaultBrush");
        return card;
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
