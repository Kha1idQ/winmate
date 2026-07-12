using Microsoft.Win32;
using WinMate.Models;
using WinMate.Services;

namespace WinMate.Data;

public static class GamingCatalog
{
    private const string GameBar = @"SOFTWARE\Microsoft\GameBar";
    private const string GameConfigStore = @"System\GameConfigStore";
    private const string GameDvrUser = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR";
    private const string GameDvrPolicy = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR";
    private const string SystemProfile = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
    private const string GamesTasks = SystemProfile + @"\Tasks\Games";

    private const string BalancedPlanGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";
    private const string HighPerfPlanGuid = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
    private const string UltimatePlanGuid = "e9a42b02-d5df-448d-aa66-32f5e213869b";

    public static readonly IReadOnlyList<Tweak> All =
    [
        new(
            Id: "game_mode_on",
            NameEn: "Enable Game Mode",
            NameAr: "تفعيل وضع الألعاب",
            DescEn: "Windows prioritizes your game and pauses background noise while playing",
            DescAr: "الويندوز يعطي لعبتك الأولوية ويوقف الإزعاج بالخلفية وأنت تلعب",
            Category: "gaming",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, GameBar, "AutoGameModeEnabled", RegistryValueKind.DWord, 1)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, GameBar, "AutoGameModeEnabled", RegistryValueKind.DWord, 0)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, GameBar, "AutoGameModeEnabled", 1)),

        new(
            Id: "gamedvr_off",
            NameEn: "Disable Xbox Game DVR",
            NameAr: "إيقاف تسجيل Xbox DVR",
            DescEn: "Stop background game recording — one of the biggest hidden FPS killers",
            DescAr: "يوقف تسجيل اللعب بالخلفية — من أكبر أسباب نزول الفريمات المخفية",
            Category: "gaming",
            Apply:
            [
                new RegistryAction(RegistryHive.CurrentUser, GameConfigStore, "GameDVR_Enabled", RegistryValueKind.DWord, 0),
                new RegistryAction(RegistryHive.CurrentUser, GameDvrUser, "AppCaptureEnabled", RegistryValueKind.DWord, 0),
                new RegistryAction(RegistryHive.LocalMachine, GameDvrPolicy, "AllowGameDVR", RegistryValueKind.DWord, 0),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.CurrentUser, GameConfigStore, "GameDVR_Enabled", RegistryValueKind.DWord, 1),
                new RegistryAction(RegistryHive.CurrentUser, GameDvrUser, "AppCaptureEnabled", RegistryValueKind.DWord, 1),
                new RegistryAction(RegistryHive.LocalMachine, GameDvrPolicy, "AllowGameDVR", RegistryValueKind.DWord, null),
            ],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, GameConfigStore, "GameDVR_Enabled", 0)),

        new(
            Id: "gamebar_popups_off",
            NameEn: "Disable Game Bar popups",
            NameAr: "إيقاف نوافذ Game Bar المنبثقة",
            DescEn: "No more Game Bar startup panel and overlay popups",
            DescAr: "يوقف لوحة Game Bar والنوافذ اللي تطلع فوق اللعبة",
            Category: "gaming",
            Apply:
            [
                new RegistryAction(RegistryHive.CurrentUser, GameBar, "UseNexusForGameBarEnabled", RegistryValueKind.DWord, 0),
                new RegistryAction(RegistryHive.CurrentUser, GameBar, "ShowStartupPanel", RegistryValueKind.DWord, 0),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.CurrentUser, GameBar, "UseNexusForGameBarEnabled", RegistryValueKind.DWord, 1),
                new RegistryAction(RegistryHive.CurrentUser, GameBar, "ShowStartupPanel", RegistryValueKind.DWord, 1),
            ],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, GameBar, "UseNexusForGameBarEnabled", 0)),

        new(
            Id: "ultimate_power",
            NameEn: "Max performance power plan",
            NameAr: "خطة الطاقة للأداء الأقصى",
            DescEn: "Activate Ultimate Performance if your Windows has it, otherwise High performance",
            DescAr: "يفعّل Ultimate Performance إذا نسخة ويندوزك تدعمها، وإلا High performance",
            Category: "gaming",
            Apply:
            [
                // Try Ultimate (existing copy, then duplicating the hidden template);
                // newer Windows 11 builds removed it, so fall back to High performance.
                new PowerShellAction(
                    "$guidRx = '([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})'; " +
                    "$existing = powercfg /list | Select-String 'Ultimate'; " +
                    "if ($existing -and $existing.ToString() -match $guidRx) { powercfg /setactive $Matches[1]; 'Activated existing Ultimate plan' } " +
                    "else { $out = powercfg /duplicatescheme " + UltimatePlanGuid + " 2>&1; " +
                    "if (\"$out\" -match $guidRx) { powercfg /setactive $Matches[1]; 'Created and activated Ultimate plan' } " +
                    "else { powercfg /setactive " + HighPerfPlanGuid + "; 'Ultimate not available on this Windows build - activated High performance instead' } }"),
            ],
            Undo: [new CommandAction("powercfg", "/setactive " + BalancedPlanGuid)],
            StateCheck: null)
        {
            // Power plan state lives in powercfg, not the registry. Matching the
            // High-performance GUID keeps the check locale-proof.
            CustomCheck = async () =>
            {
                var (_, output) = await ProcessRunner.RunCapturedAsync("powercfg", "/getactivescheme");
                return output.Contains("Ultimate", StringComparison.OrdinalIgnoreCase)
                    || output.Contains(HighPerfPlanGuid, StringComparison.OrdinalIgnoreCase);
            },
        },

        new(
            Id: "hags_on",
            NameEn: "Hardware-accelerated GPU scheduling",
            NameAr: "جدولة GPU المسرّعة عتاديًا",
            DescEn: "Let the GPU manage its own memory for lower latency (needs restart)",
            DescAr: "يخلي كرت الشاشة يدير ذاكرته بنفسه لتقليل التأخير (يحتاج ريستارت)",
            Category: "gaming",
            Apply: [new RegistryAction(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", RegistryValueKind.DWord, 2)],
            Undo:  [new RegistryAction(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2),
            RequiresRestart: true),

        new(
            Id: "mouse_accel_off",
            NameEn: "Disable mouse acceleration",
            NameAr: "إيقاف تسارع الماوس",
            DescEn: "Raw 1:1 mouse movement — same distance moves the same pixels, every time",
            DescAr: "حركة ماوس خام 1:1 — نفس المسافة تحرك نفس البكسلات كل مرة",
            Category: "gaming",
            Apply:
            [
                // These three are REG_SZ strings, not DWORDs.
                new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", RegistryValueKind.String, "0"),
                new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", RegistryValueKind.String, "0"),
                new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", RegistryValueKind.String, "0"),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", RegistryValueKind.String, "1"),
                new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", RegistryValueKind.String, "6"),
                new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", RegistryValueKind.String, "10"),
            ],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "0")),

        new(
            Id: "network_latency",
            NameEn: "Network & multimedia latency tweaks",
            NameAr: "تحسينات تأخير الشبكة والوسائط",
            DescEn: "Stop Windows throttling network/multimedia while gaming (advanced)",
            DescAr: "يمنع الويندوز من تحديد سرعة الشبكة والوسائط وأنت تلعب (متقدم)",
            Category: "gaming",
            Apply:
            [
                new RegistryAction(RegistryHive.LocalMachine, SystemProfile, "SystemResponsiveness", RegistryValueKind.DWord, 0),
                new RegistryAction(RegistryHive.LocalMachine, SystemProfile, "NetworkThrottlingIndex", RegistryValueKind.DWord, unchecked((int)0xFFFFFFFF)),
                new RegistryAction(RegistryHive.LocalMachine, GamesTasks, "GPU Priority", RegistryValueKind.DWord, 8),
                new RegistryAction(RegistryHive.LocalMachine, GamesTasks, "Priority", RegistryValueKind.DWord, 6),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.LocalMachine, SystemProfile, "SystemResponsiveness", RegistryValueKind.DWord, 20),
                new RegistryAction(RegistryHive.LocalMachine, SystemProfile, "NetworkThrottlingIndex", RegistryValueKind.DWord, 10),
                new RegistryAction(RegistryHive.LocalMachine, GamesTasks, "GPU Priority", RegistryValueKind.DWord, 8),
                new RegistryAction(RegistryHive.LocalMachine, GamesTasks, "Priority", RegistryValueKind.DWord, 2),
            ],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, SystemProfile, "SystemResponsiveness", 0)),
    ];
}
