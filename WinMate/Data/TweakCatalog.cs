using Microsoft.Win32;
using WinMate.Models;

namespace WinMate.Data;

public static class TweakCatalog
{
    private const string ExplorerAdvanced = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string ClassicMenuClsid = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";

    // Category ids map to localization keys: TweakCat_explorer, TweakCat_privacy, TweakCat_cleanup.
    public static readonly string[] Categories = ["explorer", "privacy", "cleanup"];

    // Preinstalled apps removed by the debloat action (shown in its confirm dialog).
    public static readonly string[] BloatApps =
    [
        "Microsoft.BingNews", "Microsoft.BingWeather", "Microsoft.GetHelp",
        "Microsoft.Getstarted", "Microsoft.MicrosoftSolitaireCollection", "Microsoft.People",
        "Microsoft.WindowsFeedbackHub", "Microsoft.ZuneMusic", "Microsoft.ZuneVideo",
        "Microsoft.MicrosoftOfficeHub", "Clipchamp.Clipchamp",
    ];

    public static readonly IReadOnlyList<Tweak> All =
    [
        new(
            Id: "file_ext_show",
            NameEn: "Show file extensions",
            NameAr: "إظهار امتدادات الملفات",
            DescEn: "Always show .txt, .exe, .png endings so you know what a file really is",
            DescAr: "يظهر نهايات الملفات (.txt .exe .png) دايمًا عشان تعرف حقيقة أي ملف",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "HideFileExt", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "HideFileExt", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ExplorerAdvanced, "HideFileExt", 0))
        { RestartExplorer = true },

        new(
            Id: "hidden_files_show",
            NameEn: "Show hidden files",
            NameAr: "إظهار الملفات المخفية",
            DescEn: "See hidden files and folders in Explorer",
            DescAr: "يخليك تشوف الملفات والمجلدات المخفية في المستكشف",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "Hidden", RegistryValueKind.DWord, 1)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "Hidden", RegistryValueKind.DWord, 2)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ExplorerAdvanced, "Hidden", 1))
        { RestartExplorer = true },

        new(
            Id: "taskbar_left",
            NameEn: "Taskbar icons on the left",
            NameAr: "أيقونات شريط المهام على اليسار",
            DescEn: "Classic Windows 10-style taskbar alignment",
            DescAr: "محاذاة كلاسيكية مثل ويندوز 10 بدل المنتصف",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "TaskbarAl", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "TaskbarAl", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ExplorerAdvanced, "TaskbarAl", 0)),

        // Windows 11 ACL-protects the TaskbarDa value, so the supported way to
        // hide Widgets is the machine policy (needs admin + explorer restart).
        new(
            Id: "widgets_off",
            NameEn: "Hide Widgets button",
            NameAr: "إخفاء زر الودجتس",
            DescEn: "Remove the weather/news Widgets button from the taskbar",
            DescAr: "يشيل زر الطقس والأخبار من شريط المهام",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", RegistryValueKind.DWord, null)],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0))
        { RestartExplorer = true },

        new(
            Id: "classic_context_menu",
            NameEn: "Classic right-click menu",
            NameAr: "قائمة الزر الأيمن الكلاسيكية",
            DescEn: "Bring back the full Windows 10 right-click menu (no 'Show more options')",
            DescAr: "يرجّع قائمة ويندوز 10 الكاملة بدون \"إظهار خيارات إضافية\"",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ClassicMenuClsid + @"\InprocServer32", "", RegistryValueKind.String, "")],
            Undo:  [new RegistryDeleteKeyAction(RegistryHive.CurrentUser, ClassicMenuClsid)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ClassicMenuClsid + @"\InprocServer32", "", ""))
        { RestartExplorer = true },

        new(
            Id: "bing_start_off",
            NameEn: "Disable Bing in Start menu",
            NameAr: "إيقاف Bing في قائمة ابدأ",
            DescEn: "Start menu search shows your files only, no web results",
            DescAr: "بحث قائمة ابدأ يعرض ملفاتك فقط بدون نتائج إنترنت",
            Category: "privacy",
            Apply:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", RegistryValueKind.DWord, 1),
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", RegistryValueKind.DWord, 0),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", RegistryValueKind.DWord, null),
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", RegistryValueKind.DWord, 1),
            ],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1))
        { RestartExplorer = true },

        new(
            Id: "advertising_id_off",
            NameEn: "Disable advertising ID",
            NameAr: "إيقاف معرّف الإعلانات",
            DescEn: "Stop apps from using your advertising ID to profile you",
            DescAr: "يمنع التطبيقات من استخدام معرّفك الإعلاني لتتبعك",
            Category: "privacy",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0)),

        new(
            Id: "suggestions_off",
            NameEn: "Disable Windows suggestions",
            NameAr: "إيقاف اقتراحات ويندوز",
            DescEn: "No more app suggestions and 'tips' in the Start menu",
            DescAr: "يوقف اقتراحات التطبيقات و\"النصائح\" في قائمة ابدأ",
            Category: "privacy",
            Apply:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", RegistryValueKind.DWord, 0),
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", RegistryValueKind.DWord, 0),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", RegistryValueKind.DWord, 1),
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", RegistryValueKind.DWord, 1),
            ],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0)),

        new(
            Id: "telemetry_off",
            NameEn: "Disable telemetry",
            NameAr: "إيقاف التتبع (Telemetry)",
            DescEn: "Stop Windows from sending your usage data to Microsoft (policy + DiagTrack service)",
            DescAr: "يوقف إرسال بيانات استخدامك لمايكروسوفت (سياسة + خدمة DiagTrack)",
            Category: "privacy",
            Apply:
            [
                new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", RegistryValueKind.DWord, 0),
                new ServiceAction("DiagTrack", "Disabled"),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", RegistryValueKind.DWord, null),
                new ServiceAction("DiagTrack", "Automatic"),
            ],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0)),

        new(
            Id: "sched_tasks_off",
            NameEn: "Disable telemetry scheduled tasks",
            NameAr: "إيقاف مهام التتبع المجدولة",
            DescEn: "Stop the Compatibility Appraiser and CEIP background tasks",
            DescAr: "يوقف مهام تقييم التوافق وبرنامج تحسين التجربة اللي تشتغل بالخلفية",
            Category: "privacy",
            Apply:
            [
                new CommandAction("schtasks", "/Change /TN \"\\Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /Disable"),
                new CommandAction("schtasks", "/Change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Disable"),
            ],
            Undo:
            [
                new CommandAction("schtasks", "/Change /TN \"\\Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser\" /Enable"),
                new CommandAction("schtasks", "/Change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Enable"),
            ],
            StateCheck: null)
        {
            // Scheduled-task state isn't in the registry — ask PowerShell (prints the State enum, e.g. "Disabled").
            CustomCheck = async () =>
            {
                var (_, output) = await Services.ProcessRunner.RunCapturedAsync(
                    "powershell.exe",
                    "-NoProfile -Command \"(Get-ScheduledTask -TaskName 'Microsoft Compatibility Appraiser' -TaskPath '\\Microsoft\\Windows\\Application Experience\\' -ErrorAction SilentlyContinue).State\"");
                return output.Contains("Disabled");
            },
        },

        new(
            Id: "debloat_appx",
            NameEn: "Remove preinstalled bloatware",
            NameAr: "حذف برامج البلوت المثبتة مسبقًا",
            DescEn: "Uninstall News, Weather, Solitaire, Feedback Hub and other preinstalled apps",
            DescAr: "يحذف الأخبار والطقس وسوليتير ومركز الملاحظات وغيرها من البرامج المثبتة مسبقًا",
            Category: "cleanup",
            Apply:
            [
                new PowerShellAction(
                    "$apps = 'Microsoft.BingNews','Microsoft.BingWeather','Microsoft.GetHelp','Microsoft.Getstarted'," +
                    "'Microsoft.MicrosoftSolitaireCollection','Microsoft.People','Microsoft.WindowsFeedbackHub'," +
                    "'Microsoft.ZuneMusic','Microsoft.ZuneVideo','Microsoft.MicrosoftOfficeHub','Clipchamp.Clipchamp'; " +
                    "foreach ($a in $apps) { Get-AppxPackage $a -AllUsers -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue; Write-Output ('Processed: ' + $a) }"),
            ],
            Undo: [],
            StateCheck: null),

        new(
            Id: "clean_temp",
            NameEn: "Clean temporary files",
            NameAr: "تنظيف الملفات المؤقتة",
            DescEn: "Delete the contents of the user and Windows temp folders",
            DescAr: "يحذف محتويات مجلدات temp حقتك وحقت الويندوز",
            Category: "cleanup",
            Apply:
            [
                new PowerShellAction(
                    "$before = (Get-PSDrive C).Free; " +
                    "Remove-Item ($env:TEMP + '\\*') -Recurse -Force -ErrorAction SilentlyContinue; " +
                    "Remove-Item ($env:WINDIR + '\\Temp\\*') -Recurse -Force -ErrorAction SilentlyContinue; " +
                    "$freed = [math]::Round(((Get-PSDrive C).Free - $before) / 1MB); " +
                    "Write-Output ('Freed ~' + $freed + ' MB')"),
            ],
            Undo: [],
            StateCheck: null),
    ];
}
