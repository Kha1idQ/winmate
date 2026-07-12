using System.Windows;

namespace WinMate.Services;

public static class RestorePointService
{
    private static bool _offeredThisSession;

    // Offered once per session, right before the first tweak is applied.
    public static async Task OfferOnceAsync()
    {
        if (_offeredThisSession)
            return;
        _offeredThisSession = true;

        var box = new Wpf.Ui.Controls.MessageBox
        {
            Title = (string)Application.Current.FindResource("Restore_Title"),
            Content = (string)Application.Current.FindResource("Restore_Body"),
            PrimaryButtonText = (string)Application.Current.FindResource("Restore_Yes"),
            CloseButtonText = (string)Application.Current.FindResource("Restore_No"),
        };

        var result = await box.ShowDialogAsync();
        if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            await CreateAsync();
    }

    public static async Task CreateAsync()
    {
        LogService.Write((string)Application.Current.FindResource("Restore_Creating"));

        // Windows silently skips restore points made less than 24h apart, so we
        // verify one was really created in the last few minutes and say so.
        const string script =
            "Enable-ComputerRestore -Drive ($env:SystemDrive + '\\'); " +
            "Checkpoint-Computer -Description 'WinMate' -RestorePointType 'MODIFY_SETTINGS'; " +
            "$rp = Get-ComputerRestorePoint | Sort-Object SequenceNumber | Select-Object -Last 1; " +
            "if ($rp -and ((Get-Date) - $rp.ConvertToDateTime($rp.CreationTime)).TotalMinutes -lt 5) { 'RP_OK' } else { 'RP_SKIPPED' }";

        var (_, output) = await ProcessRunner.RunCapturedAsync(
            "powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"");

        if (output.Contains("RP_OK"))
            LogService.Write((string)Application.Current.FindResource("Restore_Done"));
        else
            LogService.Write((string)Application.Current.FindResource("Restore_Skipped24h"));
    }
}
