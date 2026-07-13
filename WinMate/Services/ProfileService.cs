using WinMate.Data;
using WinMate.Models;

namespace WinMate.Services;

public static class ProfileService
{
    // Applies every reversible tweak in the profile that isn't already on.
    // Returns (applied, skippedAlreadyOn, failed) counts for a summary.
    public static async Task<(int Applied, int Skipped, int Failed)> ApplyAsync(Profile profile)
    {
        int applied = 0, skipped = 0, failed = 0;

        foreach (var id in profile.TweakIds)
        {
            var tweak = ProfileCatalog.AllTweaks.FirstOrDefault(t => t.Id == id);
            if (tweak is null)
            {
                // Guards against a typo'd id in the profile silently doing nothing.
                LogService.Write($"⚠ profile '{profile.Id}': unknown tweak id '{id}'");
                failed++;
                continue;
            }

            try
            {
                if (await TweakEngine.IsAppliedAsync(tweak))
                {
                    skipped++;
                    continue;
                }

                LogService.Write($"{LocalizationService.Pick("Applying", "جاري تفعيل")}: {tweak.NameEn}");
                await TweakEngine.ApplyAsync(tweak);
                if (tweak.RestartExplorer)
                    await TweakEngine.RestartExplorerAsync();
                applied++;
            }
            catch (Exception ex)
            {
                LogService.Write($"✗ {tweak.Id}: {ex.Message}");
                failed++;
            }
        }

        return (applied, skipped, failed);
    }

    // Undoes every reversible tweak that is currently applied, across all
    // catalogs — the "reset everything back to Windows defaults" button.
    public static async Task<(int Undone, int Failed)> UndoAllAppliedAsync()
    {
        int undone = 0, failed = 0;

        foreach (var tweak in ProfileCatalog.AllTweaks)
        {
            if (!tweak.IsReversible)
                continue; // one-shot actions (debloat, temp clean) can't be undone

            try
            {
                if (!await TweakEngine.IsAppliedAsync(tweak))
                    continue;

                LogService.Write($"{LocalizationService.Pick("Undoing", "جاري التراجع عن")}: {tweak.NameEn}");
                await TweakEngine.UndoAsync(tweak);
                if (tweak.RestartExplorer)
                    await TweakEngine.RestartExplorerAsync();
                undone++;
            }
            catch (Exception ex)
            {
                LogService.Write($"✗ {tweak.Id}: {ex.Message}");
                failed++;
            }
        }

        return (undone, failed);
    }

    // Human-readable list of the tweak names in a profile (for the confirm dialog).
    public static string DescribeTweaks(Profile profile)
    {
        var names = profile.TweakIds
            .Select(id => ProfileCatalog.AllTweaks.FirstOrDefault(t => t.Id == id))
            .Where(t => t is not null)
            .Select(t => "• " + LocalizationService.Pick(t!.NameEn, t.NameAr));
        return string.Join("\n", names);
    }
}
