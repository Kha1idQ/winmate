using WinMate.Models;

namespace WinMate.Data;

public static class ProfileCatalog
{
    // Every tweak, across both catalogs. Profiles resolve their ids against this.
    public static readonly IReadOnlyList<Tweak> AllTweaks =
        TweakCatalog.All.Concat(GamingCatalog.All).ToList();

    public static readonly IReadOnlyList<Profile> All =
    [
        new(
            Id: "gamer",
            NameEn: "Full Gamer Mode",
            NameAr: "وضع القيمر الكامل",
            DescEn: "Turns on every gaming tweak and tidies up the taskbar for playing",
            DescAr: "يشغّل كل تحسينات الألعاب ويرتّب شريط المهام عشان اللعب",
            Icon: "gamepad",
            Tile: "profile-gamer",
            TweakIds:
            [
                "game_mode_on", "gamedvr_off", "gamebar_popups_off", "ultimate_power",
                "hags_on", "mouse_accel_off", "sticky_keys_off", "end_task_taskbar",
            ]),

        new(
            Id: "privacy",
            NameEn: "Max Privacy",
            NameAr: "الخصوصية القصوى",
            DescEn: "Turns off telemetry, Copilot, Recall, location, and ad tracking",
            DescAr: "يطفّي التتبع وكوبايلوت وريكول والموقع وتتبّع الإعلانات",
            Icon: "shield-lock",
            Tile: "profile-privacy",
            TweakIds:
            [
                "telemetry_off", "sched_tasks_off", "copilot_off", "recall_off",
                "location_off", "advertising_id_off", "suggestions_off", "bing_start_off",
            ]),

        new(
            Id: "clean_fast",
            NameEn: "Clean & Fast",
            NameAr: "نظيف وسريع",
            DescEn: "Clears disk space, speeds up browsing, and cuts background junk",
            DescAr: "يفضّي مساحة، يسرّع التصفح، ويقلّل شغل الخلفية",
            Icon: "rocket",
            Tile: "profile-clean-fast",
            TweakIds:
            [
                "hibernate_off", "dns_cloudflare", "telemetry_off", "sched_tasks_off",
                "suggestions_off", "game_mode_on",
            ]),
    ];
}
