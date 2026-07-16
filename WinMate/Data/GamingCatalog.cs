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
            NameEn: "Turn on Game Mode",
            NameAr: "تشغيل وضع الألعاب",
            DescEn: "Windows puts your game first and quiets background tasks while you play",
            DescAr: "ويندوز يخلي لعبتك بالأول ويهدّي المهام اللي تشتغل بالخلفية وانت تلعب",
            Category: "gaming",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, GameBar, "AutoGameModeEnabled", RegistryValueKind.DWord, 1)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, GameBar, "AutoGameModeEnabled", RegistryValueKind.DWord, 0)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, GameBar, "AutoGameModeEnabled", 1))
        { Icon = "gamepad" },

        new(
            Id: "gamedvr_off",
            NameEn: "Turn off Xbox Game DVR",
            NameAr: "إيقاف تسجيل الألعاب من إكس بوكس",
            DescEn: "Stops the background recording that quietly eats into your FPS",
            DescAr: "يوقف تسجيل اللعب بالخلفية، وهو من أكثر الأشياء اللي تنزّل الفريمات وانت ما تدري",
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
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, GameConfigStore, "GameDVR_Enabled", 0))
        { Icon = "record-off" },

        new(
            Id: "gamebar_popups_off",
            NameEn: "Turn off Game Bar popups",
            NameAr: "إيقاف نوافذ القيم بار المنبثقة",
            DescEn: "Stops the Game Bar panel and its overlay from popping up",
            DescAr: "يمنع لوحة القيم بار والنوافذ اللي تطلع فوق اللعبة",
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
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, GameBar, "UseNexusForGameBarEnabled", 0))
        { Icon = "message-off" },

        new(
            Id: "ultimate_power",
            NameEn: "Top performance power plan",
            NameAr: "خطة طاقة بأعلى أداء",
            DescEn: "Switches to Ultimate Performance if your Windows has it, or High performance if not",
            DescAr: "يشغّل أقوى خطة طاقة تدعمها نسختك، وإذا ما فيها يرجع للأداء العالي",
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
            Icon = "bolt",
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
            NameAr: "جدولة كرت الشاشة بتسريع عتادي",
            DescEn: "Lets the GPU handle its own memory to trim latency (needs a restart)",
            DescAr: "يخلي كرت الشاشة يدير ذاكرته بنفسه عشان يقل التأخير (يبي ريستارت)",
            Category: "gaming",
            Apply: [new RegistryAction(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", RegistryValueKind.DWord, 2)],
            Undo:  [new RegistryAction(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2),
            RequiresRestart: true)
        { Icon = "gpu" },

        new(
            Id: "mouse_accel_off",
            NameEn: "Turn off mouse acceleration",
            NameAr: "إيقاف تسارع الماوس",
            DescEn: "Straight 1:1 aim: move the mouse the same distance, get the same pixels every time",
            DescAr: "تصويب 1:1 بدون تدخل: نفس مسافة الحركة تعطيك نفس البكسلات كل مرة",
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
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "0"))
        { Icon = "mouse-target" },

        new(
            Id: "network_latency",
            NameEn: "Network & multimedia latency tweaks",
            NameAr: "تعديلات تأخير الشبكة والوسائط",
            DescEn: "Keeps Windows from throttling network and media while you play (advanced, small effect)",
            DescAr: "يمنع ويندوز من تحديد سرعة الشبكة والوسائط وانت تلعب (متقدم، فرقه بسيط)",
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
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, SystemProfile, "SystemResponsiveness", 0))
        { Icon = "network", IsAdvanced = true },
    ];
}
