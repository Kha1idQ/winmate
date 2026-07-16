using Microsoft.Win32;
using WinMate.Models;

namespace WinMate.Data;

public static class TweakCatalog
{
    private const string ExplorerAdvanced = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string ClassicMenuClsid = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";

    // Category ids map to localization keys: TweakCat_explorer, TweakCat_privacy, TweakCat_performance, TweakCat_cleanup.
    public static readonly string[] Categories = ["explorer", "privacy", "performance", "cleanup"];

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
            DescEn: "Puts the .txt, .exe, .png ending back on every filename so you can tell what you're opening",
            DescAr: "يخلي نهاية كل ملف تبيّن نوعه، عشان تعرف وش تفتح قبل لا تضغط",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "HideFileExt", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "HideFileExt", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ExplorerAdvanced, "HideFileExt", 0))
        { RestartExplorer = true },

        new(
            Id: "hidden_files_show",
            NameEn: "Show hidden files",
            NameAr: "إظهار الملفات المخفية",
            DescEn: "Makes hidden files and folders show up in Explorer",
            DescAr: "يبيّن لك الملفات والمجلدات المخفية داخل المستكشف",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "Hidden", RegistryValueKind.DWord, 1)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced, "Hidden", RegistryValueKind.DWord, 2)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ExplorerAdvanced, "Hidden", 1))
        { RestartExplorer = true },

        new(
            Id: "taskbar_left",
            NameEn: "Taskbar icons on the left",
            NameAr: "أيقونات شريط المهام على اليسار",
            DescEn: "Moves the taskbar icons to the left, the way Windows 10 had them",
            DescAr: "يرجّع أيقونات الشريط لليسار مثل ويندوز 10 بدل المنتصف",
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
            DescEn: "Takes the weather and news Widgets button off the taskbar",
            DescAr: "يشيل زر الودجتس (طقس وأخبار) من شريط المهام",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", RegistryValueKind.DWord, null)],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0))
        { RestartExplorer = true },

        new(
            Id: "classic_context_menu",
            NameEn: "Classic right-click menu",
            NameAr: "قائمة الزر الأيمن الكلاسيكية",
            DescEn: "Gives you the full Windows 10 right-click menu back, without the extra 'Show more options' step",
            DescAr: "يرجّع قائمة الزر الأيمن كاملة مثل ويندوز 10، بدون ما تضغط \"إظهار خيارات إضافية\"",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ClassicMenuClsid + @"\InprocServer32", "", RegistryValueKind.String, "")],
            Undo:  [new RegistryDeleteKeyAction(RegistryHive.CurrentUser, ClassicMenuClsid)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ClassicMenuClsid + @"\InprocServer32", "", ""))
        { RestartExplorer = true },

        new(
            Id: "bing_start_off",
            NameEn: "Disable Bing in Start menu",
            NameAr: "إيقاف بحث بينق في قائمة ابدأ",
            DescEn: "Makes Start menu search look through your files only, without pulling in web results",
            DescAr: "يخلي بحث قائمة ابدأ يدوّر بملفاتك بس، بدون نتائج من النت",
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
            Id: "end_task_taskbar",
            NameEn: "'End Task' in taskbar right-click",
            NameAr: "خيار \"إنهاء المهمة\" بالزر الأيمن للتاسك بار",
            DescEn: "Lets you close a stuck app right from its taskbar button instead of opening Task Manager",
            DescAr: "يخليك تسكّر البرنامج المعلّق من زره في الشريط، من دون ما تفتح مدير المهام",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced + @"\TaskbarDeveloperSettings", "TaskbarEndTask", RegistryValueKind.DWord, 1)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, ExplorerAdvanced + @"\TaskbarDeveloperSettings", "TaskbarEndTask", RegistryValueKind.DWord, 0)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, ExplorerAdvanced + @"\TaskbarDeveloperSettings", "TaskbarEndTask", 1)),

        new(
            Id: "dark_mode_on",
            NameEn: "Dark mode everywhere",
            NameAr: "الوضع الداكن بكل مكان",
            DescEn: "Turns on the dark theme for Windows and its apps",
            DescAr: "يحوّل الويندوز والتطبيقات للثيم الداكن",
            Category: "explorer",
            Apply:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", RegistryValueKind.DWord, 0),
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", RegistryValueKind.DWord, 0),
            ],
            Undo:
            [
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", RegistryValueKind.DWord, 1),
                new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", RegistryValueKind.DWord, 1),
            ],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0)),

        new(
            Id: "sticky_keys_off",
            NameEn: "Disable Sticky Keys popup",
            NameAr: "إيقاف نافذة المفاتيح الثابتة",
            DescEn: "Stops the popup that shows up when you tap Shift 5 times, the one that yanks you out of your game",
            DescAr: "يوقف النافذة اللي تطلع لك إذا كبست شفت خمس مرات وأنت وسط اللعب",
            Category: "explorer",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", RegistryValueKind.String, "506")],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", RegistryValueKind.String, "510")],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "506")),

        new(
            Id: "copilot_off",
            NameEn: "Disable Copilot",
            NameAr: "إيقاف Copilot",
            DescEn: "Switches off the Copilot AI assistant",
            DescAr: "يطفّي مساعد Copilot الذكي",
            Category: "privacy",
            Apply:  [new RegistryAction(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", RegistryValueKind.DWord, 1)],
            Undo:   [new RegistryAction(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", RegistryValueKind.DWord, null)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1)),

        new(
            Id: "recall_off",
            NameEn: "Disable Windows Recall",
            NameAr: "إيقاف Windows Recall",
            DescEn: "Stops Windows from snapping constant screenshots of everything you do",
            DescAr: "يمنع الويندوز من تصوير شاشتك على طول وتحليل كل شي تسويه",
            Category: "privacy",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", RegistryValueKind.DWord, 1)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", RegistryValueKind.DWord, null)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1)),

        new(
            Id: "location_off",
            NameEn: "Disable location access",
            NameAr: "إيقاف الوصول للموقع",
            DescEn: "Keeps Windows and apps from reading your location",
            DescAr: "يمنع الويندوز والتطبيقات من الوصول لموقعك",
            Category: "privacy",
            Apply: [new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", RegistryValueKind.String, "Deny")],
            Undo:  [new RegistryAction(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", RegistryValueKind.String, "Allow")],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny")),

        new(
            Id: "advertising_id_off",
            NameEn: "Disable advertising ID",
            NameAr: "إيقاف معرّف الإعلانات",
            DescEn: "Stops apps from tracking you through your advertising ID",
            DescAr: "يمنع التطبيقات من استخدام معرّف الإعلانات عشان تتبعك",
            Category: "privacy",
            Apply: [new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", RegistryValueKind.DWord, 0)],
            Undo:  [new RegistryAction(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", RegistryValueKind.DWord, 1)],
            StateCheck: new RegistryCheck(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0)),

        new(
            Id: "suggestions_off",
            NameEn: "Disable Windows suggestions",
            NameAr: "إيقاف اقتراحات ويندوز",
            DescEn: "Clears the app suggestions and 'tips' out of the Start menu",
            DescAr: "يوقف اقتراحات التطبيقات و\"النصائح\" اللي تطلع في قائمة ابدأ",
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
            NameAr: "إيقاف التتبع والتجسس",
            DescEn: "Stops Windows from sending your usage data to Microsoft, through both the policy and the DiagTrack service",
            DescAr: "يوقف إرسال بيانات استخدامك لمايكروسوفت، عن طريق السياسة وخدمة DiagTrack",
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
            DescEn: "Turns off the Compatibility Appraiser and CEIP tasks that run in the background",
            DescAr: "يطفّي مهام تقييم التوافق وبرنامج تحسين التجربة اللي تشتغل بالخلفية",
            Category: "privacy",
            // Which of these telemetry tasks exist varies by Windows build, so we
            // only touch the ones actually present and never error on missing ones.
            Apply:
            [
                new PowerShellAction(
                    @"$defs=@(@('\Microsoft\Windows\Application Experience\','Microsoft Compatibility Appraiser'),@('\Microsoft\Windows\Application Experience\','ProgramDataUpdater'),@('\Microsoft\Windows\Customer Experience Improvement Program\','Consolidator'),@('\Microsoft\Windows\Customer Experience Improvement Program\','UsbCeip')); foreach($d in $defs){ $t=Get-ScheduledTask -TaskPath $d[0] -TaskName $d[1] -ErrorAction SilentlyContinue; if($t){ $null=Disable-ScheduledTask -TaskPath $d[0] -TaskName $d[1] -ErrorAction SilentlyContinue; Write-Output ('Disabled: '+$d[1]) } else { Write-Output ('Not on this PC, skipped: '+$d[1]) } }"),
            ],
            Undo:
            [
                new PowerShellAction(
                    @"$defs=@(@('\Microsoft\Windows\Application Experience\','Microsoft Compatibility Appraiser'),@('\Microsoft\Windows\Application Experience\','ProgramDataUpdater'),@('\Microsoft\Windows\Customer Experience Improvement Program\','Consolidator'),@('\Microsoft\Windows\Customer Experience Improvement Program\','UsbCeip')); foreach($d in $defs){ $t=Get-ScheduledTask -TaskPath $d[0] -TaskName $d[1] -ErrorAction SilentlyContinue; if($t){ $null=Enable-ScheduledTask -TaskPath $d[0] -TaskName $d[1] -ErrorAction SilentlyContinue; Write-Output ('Enabled: '+$d[1]) } }"),
            ],
            StateCheck: null)
        {
            // Applied = at least one of these tasks exists and every existing one is Disabled.
            CustomCheck = async () =>
            {
                const string script =
                    @"$defs=@(@('\Microsoft\Windows\Application Experience\','Microsoft Compatibility Appraiser'),@('\Microsoft\Windows\Application Experience\','ProgramDataUpdater'),@('\Microsoft\Windows\Customer Experience Improvement Program\','Consolidator'),@('\Microsoft\Windows\Customer Experience Improvement Program\','UsbCeip')); $states=foreach($d in $defs){ $t=Get-ScheduledTask -TaskPath $d[0] -TaskName $d[1] -ErrorAction SilentlyContinue; if($t){ $t.State } }; if($states -and -not ($states | Where-Object { $_ -ne 'Disabled' })){ 'APPLIED' } else { 'NOT' }";
                var (_, output) = await Services.ProcessRunner.RunCapturedAsync(
                    "powershell.exe", "-NoProfile -Command \"" + script + "\"");
                return output.Contains("APPLIED");
            },
        },

        new(
            Id: "hibernate_off",
            NameEn: "Disable hibernation",
            NameAr: "إيقاف السبات لتوفير المساحة",
            DescEn: "Frees up the gigabytes that hiberfil.sys reserves on your disk",
            DescAr: "يفكّ قيقات محجوزة على الدسك لملف السبات hiberfil.sys",
            Category: "performance",
            Apply: [new CommandAction("powercfg", "/hibernate off")],
            Undo:  [new CommandAction("powercfg", "/hibernate on")],
            StateCheck: new RegistryCheck(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0)),

        new(
            Id: "dns_cloudflare",
            NameEn: "Fast DNS (Cloudflare 1.1.1.1)",
            NameAr: "دي إن إس سريع من كلاود فلير",
            DescEn: "Speeds up website lookups and keeps them more private, set on every network adapter",
            DescAr: "يخلي فتح المواقع أسرع وأكثر خصوصية، ويطبّقه على كل كروت الشبكة",
            Category: "performance",
            Apply:
            [
                new PowerShellAction(
                    "Get-NetAdapter -Physical | Where-Object Status -eq 'Up' | ForEach-Object { " +
                    "Set-DnsClientServerAddress -InterfaceIndex $_.ifIndex -ServerAddresses '1.1.1.1','1.0.0.1'; " +
                    "Write-Output ('DNS set on: ' + $_.Name) }"),
            ],
            Undo:
            [
                new PowerShellAction(
                    "Get-NetAdapter -Physical | Where-Object Status -eq 'Up' | ForEach-Object { " +
                    "Set-DnsClientServerAddress -InterfaceIndex $_.ifIndex -ResetServerAddresses; " +
                    "Write-Output ('DNS reset on: ' + $_.Name) }"),
            ],
            StateCheck: null)
        {
            CustomCheck = async () =>
            {
                var (_, output) = await Services.ProcessRunner.RunCapturedAsync(
                    "powershell.exe",
                    "-NoProfile -Command \"(Get-DnsClientServerAddress -AddressFamily IPv4).ServerAddresses -contains '1.1.1.1'\"");
                return output.Contains("True");
            },
        },

        new(
            Id: "onedrive_remove",
            NameEn: "Uninstall OneDrive",
            NameAr: "حذف OneDrive",
            DescEn: "Removes OneDrive for good, and your local files stay where they are",
            DescAr: "يشيل OneDrive نهائيًا، وملفاتك اللي على الجهاز ما تنمس",
            Category: "cleanup",
            Apply:
            [
                new PowerShellAction(
                    "Stop-Process -Name OneDrive -Force -ErrorAction SilentlyContinue; " +
                    "winget uninstall --id Microsoft.OneDrive -e --silent --accept-source-agreements --disable-interactivity; " +
                    "Write-Output 'OneDrive uninstall finished'"),
            ],
            Undo: [],
            StateCheck: null),

        new(
            Id: "debloat_appx",
            NameEn: "Remove preinstalled bloatware",
            NameAr: "حذف برامج البلوت المثبتة مسبقًا",
            DescEn: "Removes News, Weather, Solitaire, Feedback Hub and the rest of the preinstalled apps",
            DescAr: "يشيل الأخبار والطقس وسوليتير ومركز الملاحظات وباقي البرامج المثبتة مسبقًا",
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
            DescEn: "Empties out your temp folder and the Windows temp folder",
            DescAr: "يفرّغ ملفاتك المؤقتة ومجلد المؤقتات حق الويندوز",
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
