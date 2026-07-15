using WinMate.Models;

namespace WinMate.Data;

// Curated, honest explanations keyed by tweak id. The "performance" line tells
// the truth — several of these are about privacy or convenience and change
// nothing about speed, and a couple are near-placebo; saying so is the point.
public static class TweakExplanations
{
    public static readonly IReadOnlyDictionary<string, Explanation> All = new Dictionary<string, Explanation>
    {
        // ---- Explorer & taskbar ----
        ["file_ext_show"] = new(
            WhatEn: "Makes File Explorer always show the file type at the end of every file name (.txt, .exe, .png).",
            WhatAr: "يخلي مستكشف الملفات يعرض نوع الملف في نهاية كل اسم دايمًا (.txt، .exe، .png).",
            WhyEn: "So you can tell a real document from a program pretending to be one — a basic protection against disguised malware.",
            WhyAr: "عشان تفرّق الملف الحقيقي عن برنامج يتظاهر إنه ملف — حماية أساسية من الفيروسات المتنكّرة.",
            PerfEn: "No effect on speed. This is a safety and clarity setting.",
            PerfAr: "ما يأثر على السرعة. هذا إعداد أمان ووضوح.",
            RiskEn: "None. It only changes what you see.",
            RiskAr: "لا مخاطر. يغيّر بس اللي تشوفه."),

        ["hidden_files_show"] = new(
            WhatEn: "Shows hidden files and folders in File Explorer.",
            WhatAr: "يظهر الملفات والمجلدات المخفية في المستكشف.",
            WhyEn: "Useful when you need to reach app data or config folders that Windows hides by default.",
            WhyAr: "مفيد لما تحتاج توصل لمجلدات بيانات البرامج أو الإعدادات اللي يخفيها ويندوز.",
            PerfEn: "No effect on speed.",
            PerfAr: "ما يأثر على السرعة.",
            RiskEn: "Low — you can now see system files, so avoid deleting things you don't recognize.",
            RiskAr: "بسيط — بتشوف ملفات النظام، فتجنّب حذف أشياء ما تعرفها."),

        ["taskbar_left"] = new(
            WhatEn: "Aligns the taskbar icons to the left, like Windows 10.",
            WhatAr: "يحاذي أيقونات شريط المهام لليسار، مثل ويندوز 10.",
            WhyEn: "Preference — many people find the Start button easier to hit in the bottom-left corner.",
            WhyAr: "تفضيل — كثير يشوفون زر ابدأ أسهل بالزاوية اليسرى.",
            PerfEn: "No effect on speed. Purely cosmetic.",
            PerfAr: "ما يأثر على السرعة. شكلي بحت.",
            RiskEn: "None.",
            RiskAr: "لا مخاطر."),

        ["widgets_off"] = new(
            WhatEn: "Hides the Widgets (weather/news) button from the taskbar.",
            WhatAr: "يخفي زر الودجتس (الطقس/الأخبار) من شريط المهام.",
            WhyEn: "Removes a button most people never use, and stops its background news feed.",
            WhyAr: "يشيل زر أغلب الناس ما تستخدمه، ويوقف تغذية الأخبار بالخلفية.",
            PerfEn: "Tiny gain at most — the widget feed uses a little memory and network in the background.",
            PerfAr: "مكسب بسيط جدًا — تغذية الودجتس تستهلك شوي ذاكرة وشبكة بالخلفية.",
            RiskEn: "None. You can re-enable it any time.",
            RiskAr: "لا مخاطر. تقدر ترجّعه بأي وقت."),

        ["classic_context_menu"] = new(
            WhatEn: "Brings back the full Windows 10 right-click menu, removing the 'Show more options' extra step.",
            WhatAr: "يرجّع قائمة الزر الأيمن الكاملة من ويندوز 10، ويشيل خطوة \"إظهار خيارات إضافية\".",
            WhyEn: "The Windows 11 menu hides common actions behind an extra click; this restores them all at once.",
            WhyAr: "قائمة ويندوز 11 تخفي إجراءات شائعة خلف ضغطة زيادة؛ هذا يرجّعها كلها دفعة وحدة.",
            PerfEn: "No effect on speed — but the classic menu often feels faster to use.",
            PerfAr: "ما يأثر على السرعة — بس القائمة الكلاسيكية تحس أسرع بالاستخدام.",
            RiskEn: "None. Fully reversible.",
            RiskAr: "لا مخاطر. قابل للتراجع بالكامل."),

        ["end_task_taskbar"] = new(
            WhatEn: "Adds an 'End Task' option to the taskbar right-click menu.",
            WhatAr: "يضيف خيار \"إنهاء المهمة\" لقائمة الزر الأيمن على شريط المهام.",
            WhyEn: "Lets you force-close a frozen app straight from the taskbar without opening Task Manager.",
            WhyAr: "يخليك تسكّر برنامج معلّق مباشرة من شريط المهام بدون ما تفتح مدير المهام.",
            PerfEn: "No effect on speed. It's a convenience option.",
            PerfAr: "ما يأثر على السرعة. خيار تسهيل.",
            RiskEn: "Low — ending an app this way skips its save prompt, so you can lose unsaved work.",
            RiskAr: "بسيط — إنهاء برنامج كذا يتخطى حفظ العمل، فممكن تخسر شغل غير محفوظ."),

        ["dark_mode_on"] = new(
            WhatEn: "Switches Windows and its apps to the dark theme.",
            WhatAr: "يحوّل ويندوز وتطبيقاته للثيم الداكن.",
            WhyEn: "Easier on the eyes in low light, and on OLED screens it can save a little battery.",
            WhyAr: "أريح للعين بالإضاءة الخافتة، وعلى شاشات OLED يوفّر شوي بطارية.",
            PerfEn: "No effect on speed. Slight battery saving only on OLED displays.",
            PerfAr: "ما يأثر على السرعة. توفير بطارية بسيط فقط على شاشات OLED.",
            RiskEn: "None.",
            RiskAr: "لا مخاطر."),

        ["sticky_keys_off"] = new(
            WhatEn: "Stops the Sticky Keys popup that appears when you press Shift five times.",
            WhatAr: "يوقف نافذة المفاتيح الثابتة اللي تطلع لما تضغط Shift خمس مرات.",
            WhyEn: "That popup interrupts games and fast typing; turning it off removes the annoyance.",
            WhyAr: "النافذة تقاطع الألعاب والكتابة السريعة؛ إيقافها يشيل الإزعاج.",
            PerfEn: "No effect on speed.",
            PerfAr: "ما يأثر على السرعة.",
            RiskEn: "None — this only disables the shortcut popup, not the accessibility feature itself in Settings.",
            RiskAr: "لا مخاطر — يوقف بس نافذة الاختصار، مو ميزة الوصول نفسها في الإعدادات."),

        // ---- Privacy ----
        ["bing_start_off"] = new(
            WhatEn: "Stops the Start menu search from showing Bing web results — it searches only your files.",
            WhatAr: "يوقف بحث قائمة ابدأ من عرض نتائج Bing — يبحث في ملفاتك فقط.",
            WhyEn: "Local search is faster and doesn't send what you type to Microsoft's servers.",
            WhyAr: "البحث المحلي أسرع وما يرسل اللي تكتبه لسيرفرات مايكروسوفت.",
            PerfEn: "Search feels a bit snappier since it no longer waits on the web.",
            PerfAr: "البحث يصير أسرع شوي لأنه ما ينتظر الإنترنت.",
            RiskEn: "None — you just lose web results inside Start, which the browser does better anyway.",
            RiskAr: "لا مخاطر — بس تخسر نتائج الويب داخل ابدأ، واللي المتصفح يسويها أفضل أصلًا."),

        ["copilot_off"] = new(
            WhatEn: "Turns off the Copilot AI assistant.",
            WhatAr: "يوقف مساعد كوبايلوت الذكي.",
            WhyEn: "If you don't use Copilot, disabling it removes a feature that sits in the background.",
            WhyAr: "لو ما تستخدم كوبايلوت، إيقافه يشيل ميزة قاعدة بالخلفية.",
            PerfEn: "Negligible — very small memory saving.",
            PerfAr: "لا يُذكر — توفير ذاكرة بسيط جدًا.",
            RiskEn: "None. It's fully reversible.",
            RiskAr: "لا مخاطر. قابل للتراجع بالكامل."),

        ["recall_off"] = new(
            WhatEn: "Disables Windows Recall, which periodically takes screenshots of your screen to make it searchable.",
            WhatAr: "يوقف Windows Recall، اللي يصوّر شاشتك دوريًا عشان يخليها قابلة للبحث.",
            WhyEn: "A major privacy control — Recall stores a visual history of everything you do. Many people prefer it off.",
            WhyAr: "ضبط خصوصية مهم — Recall يخزّن سجلًا بصريًا لكل اللي تسويه. كثير يفضّلونه مقفول.",
            PerfEn: "Can free up disk space and background CPU, since Recall no longer captures and analyzes screenshots.",
            PerfAr: "يوفّر مساحة دسك ومعالج بالخلفية، لأن Recall يوقف تصوير وتحليل الشاشة.",
            RiskEn: "None to your system — you only lose the Recall timeline feature.",
            RiskAr: "لا مخاطر على نظامك — بس تخسر ميزة سجل Recall."),

        ["location_off"] = new(
            WhatEn: "Blocks Windows and apps from accessing your location.",
            WhatAr: "يمنع ويندوز والتطبيقات من الوصول لموقعك.",
            WhyEn: "Privacy — stops apps from knowing where you are unless you turn it back on.",
            WhyAr: "خصوصية — يمنع التطبيقات من معرفة مكانك إلا لو رجّعته.",
            PerfEn: "No effect on speed.",
            PerfAr: "ما يأثر على السرعة.",
            RiskEn: "Low — location-based features (Maps, Find my device, weather-by-location) won't work until re-enabled.",
            RiskAr: "بسيط — الميزات المعتمدة على الموقع (الخرائط، البحث عن الجهاز، الطقس حسب موقعك) ما تشتغل إلا لو رجّعته."),

        ["advertising_id_off"] = new(
            WhatEn: "Turns off the advertising ID that apps use to profile you for targeted ads.",
            WhatAr: "يوقف معرّف الإعلانات اللي تستخدمه التطبيقات لتتبعك للإعلانات الموجّهة.",
            WhyEn: "Privacy — apps get a fresh, unlinkable ID instead of one that follows you around.",
            WhyAr: "خصوصية — التطبيقات تاخذ معرّفًا جديدًا غير قابل للربط بدل واحد يتبعك.",
            PerfEn: "No effect on speed.",
            PerfAr: "ما يأثر على السرعة.",
            RiskEn: "None.",
            RiskAr: "لا مخاطر."),

        ["suggestions_off"] = new(
            WhatEn: "Turns off app suggestions and 'tips' that appear in the Start menu.",
            WhatAr: "يوقف اقتراحات التطبيقات و\"النصائح\" اللي تطلع في قائمة ابدأ.",
            WhyEn: "Removes promoted apps and nags you didn't ask for.",
            WhyAr: "يشيل التطبيقات المروّجة والتنبيهات اللي ما طلبتها.",
            PerfEn: "No meaningful effect on speed — mostly a cleaner, ad-free Start menu.",
            PerfAr: "ما يأثر فعليًا على السرعة — بس قائمة ابدأ أنظف بدون إعلانات.",
            RiskEn: "None.",
            RiskAr: "لا مخاطر."),

        ["telemetry_off"] = new(
            WhatEn: "Sets telemetry to the minimum via policy and disables the DiagTrack tracking service.",
            WhatAr: "يضبط التتبع للحد الأدنى عبر سياسة ويوقف خدمة DiagTrack.",
            WhyEn: "Reduces how much usage data Windows sends to Microsoft in the background.",
            WhyAr: "يقلّل كمية بيانات الاستخدام اللي يرسلها ويندوز لمايكروسوفت بالخلفية.",
            PerfEn: "Small background gain — the DiagTrack service periodically uses CPU, disk and network.",
            PerfAr: "مكسب بسيط بالخلفية — خدمة DiagTrack تستهلك دوريًا معالج ودسك وشبكة.",
            RiskEn: "Low — some Windows feedback and diagnostics features send less data. Reversible.",
            RiskAr: "بسيط — بعض ميزات الملاحظات والتشخيص ترسل بيانات أقل. قابل للتراجع."),

        ["sched_tasks_off"] = new(
            WhatEn: "Disables the Compatibility Appraiser and CEIP scheduled tasks that collect usage data.",
            WhatAr: "يوقف مهام تقييم التوافق و CEIP المجدولة اللي تجمع بيانات الاستخدام.",
            WhyEn: "These run in the background purely to gather telemetry; disabling them stops that.",
            WhyAr: "تشتغل بالخلفية لجمع التتبع فقط؛ إيقافها يوقف ذلك.",
            PerfEn: "Occasional background gain — these tasks can briefly spike CPU/disk when they run.",
            PerfAr: "مكسب متقطّع بالخلفية — هالمهام ترفع المعالج/الدسك لحظيًا وقت تشتغل.",
            RiskEn: "None to normal use. Reversible.",
            RiskAr: "لا مخاطر على الاستخدام العادي. قابل للتراجع."),

        // ---- Performance ----
        ["hibernate_off"] = new(
            WhatEn: "Turns off hibernation and deletes the hidden hiberfil.sys file.",
            WhatAr: "يوقف السبات ويحذف ملف hiberfil.sys المخفي.",
            WhyEn: "That file is as large as your RAM (often several GB). If you never hibernate, it's wasted space.",
            WhyAr: "الملف بحجم رامك (غالبًا عدة قيقا). لو ما تستخدم السبات، فهو مساحة ضايعة.",
            PerfEn: "Frees real disk space (several GB). Does not affect speed otherwise.",
            PerfAr: "يحرّر مساحة دسك حقيقية (عدة قيقا). ما يأثر على السرعة غير ذلك.",
            RiskEn: "Low — you lose Hibernate as a shutdown option. On some laptops this also disables Fast Startup. Reversible.",
            RiskAr: "بسيط — تخسر السبات كخيار إيقاف. على بعض اللابتوبات يوقف أيضًا Fast Startup. قابل للتراجع."),

        ["dns_cloudflare"] = new(
            WhatEn: "Sets your network adapters to use Cloudflare's DNS servers (1.1.1.1).",
            WhatAr: "يضبط كروت الشبكة لتستخدم خوادم DNS من كلاود فلير (1.1.1.1).",
            WhyEn: "DNS turns website names into addresses. Cloudflare's is often faster and more private than your ISP's.",
            WhyAr: "الـDNS يحوّل أسماء المواقع لعناوين. حق كلاود فلير غالبًا أسرع وأكثر خصوصية من مزوّدك.",
            PerfEn: "Can make websites start loading a bit quicker (faster name lookups). It does not increase your download speed.",
            PerfAr: "يخلي المواقع تبدأ التحميل أسرع شوي (بحث أسماء أسرع). ما يزيد سرعة التحميل الفعلية.",
            RiskEn: "Low — if a network needs its own DNS (some workplaces), the reset (undo) restores automatic DNS.",
            RiskAr: "بسيط — لو شبكة تحتاج DNS خاص (بعض الشركات)، التراجع يرجّع DNS التلقائي."),

        // ---- Cleanup & debloat (one-shot) ----
        ["onedrive_remove"] = new(
            WhatEn: "Completely uninstalls OneDrive. Your local files are left untouched.",
            WhatAr: "يحذف OneDrive نهائيًا. ملفاتك المحلية ما تنلمس.",
            WhyEn: "If you don't use Microsoft's cloud sync, OneDrive just runs in the background and nags to sign in.",
            WhyAr: "لو ما تستخدم مزامنة مايكروسوفت السحابية، OneDrive بس يشتغل بالخلفية ويلح عليك تسجّل دخول.",
            PerfEn: "Small background gain — OneDrive uses memory and syncs in the background while running.",
            PerfAr: "مكسب بسيط بالخلفية — OneDrive يستهلك ذاكرة ويزامن بالخلفية وهو شغّال.",
            RiskEn: "Medium — files stored ONLY in OneDrive's cloud won't sync anymore. Back up anything important first.",
            RiskAr: "متوسط — الملفات المخزّنة فقط في سحابة OneDrive ما تتزامن بعدها. احفظ المهم أول."),

        ["debloat_appx"] = new(
            WhatEn: "Uninstalls a set of preinstalled apps: News, Weather, Solitaire, Feedback Hub, and more.",
            WhatAr: "يحذف مجموعة برامج مثبتة مسبقًا: الأخبار، الطقس، سوليتير، مركز الملاحظات، وغيرها.",
            WhyEn: "These are rarely-used apps that ship with Windows and clutter the Start menu.",
            WhyAr: "برامج نادرًا تُستخدم تجي مع ويندوز وتزحم قائمة ابدأ.",
            PerfEn: "Minor — frees a little disk and removes a few Start-menu entries. Not a big speed change.",
            PerfAr: "بسيط — يحرّر شوي دسك ويشيل بعض عناصر ابدأ. مو تغيير سرعة كبير.",
            RiskEn: "Low, but it can't be undone from here — the confirmation dialog lists exactly what will be removed. You can reinstall any of them from the Store.",
            RiskAr: "بسيط، بس ما يتراجع من هنا — نافذة التأكيد تعرض بالضبط اللي بينحذف. تقدر تعيد تثبيت أي منها من المتجر."),

        ["clean_temp"] = new(
            WhatEn: "Deletes the contents of your user and Windows temporary folders.",
            WhatAr: "يحذف محتويات مجلدات temp حقتك وحقت الويندوز.",
            WhyEn: "Temp files are leftovers programs no longer need; clearing them frees space.",
            WhyAr: "الملفات المؤقتة بقايا ما تحتاجها البرامج؛ حذفها يحرّر مساحة.",
            PerfEn: "Frees disk space. It does not make the PC faster, and files in use are safely skipped.",
            PerfAr: "يحرّر مساحة دسك. ما يخلي الجهاز أسرع، والملفات المستخدمة تُتخطى بأمان.",
            RiskEn: "Very low — Windows regenerates any temp file it needs. One-time action.",
            RiskAr: "بسيط جدًا — ويندوز يعيد إنشاء أي ملف مؤقت يحتاجه. إجراء لمرة واحدة."),

        // ---- Gaming ----
        ["game_mode_on"] = new(
            WhatEn: "Turns on Windows Game Mode.",
            WhatAr: "يفعّل وضع الألعاب في ويندوز.",
            WhyEn: "Game Mode tells Windows to prioritize the game and hold back background work while you play.",
            WhyAr: "وضع الألعاب يخلي ويندوز يعطي اللعبة الأولوية ويأجّل شغل الخلفية وأنت تلعب.",
            PerfEn: "Small and situational — it can smooth out frame dips on busy systems, but it's not a big FPS boost.",
            PerfAr: "بسيط وحسب الحالة — يقلّل تقطّع الفريمات على الأجهزة المزدحمة، بس مو قفزة FPS كبيرة.",
            RiskEn: "None. Reversible.",
            RiskAr: "لا مخاطر. قابل للتراجع."),

        ["gamedvr_off"] = new(
            WhatEn: "Disables Xbox Game DVR, the built-in background game recorder.",
            WhatAr: "يوقف Xbox Game DVR، مسجّل الألعاب المدمج اللي يشتغل بالخلفية.",
            WhyEn: "Background recording constantly captures your gameplay, which steals GPU time even if you never use the clips.",
            WhyAr: "التسجيل بالخلفية يصوّر لعبك باستمرار، فياخذ وقت الكرت حتى لو ما تستخدم المقاطع.",
            PerfEn: "One of the more genuinely useful gaming tweaks — it can measurably improve frame rates, especially on mid-range PCs.",
            PerfAr: "من أكثر تحسينات الألعاب فائدة فعلية — يقدر يحسّن الفريمات بشكل ملموس، خصوصًا على الأجهزة المتوسطة.",
            RiskEn: "Low — you lose the Win+G background recording. You can still record with OBS or other tools. Reversible.",
            RiskAr: "بسيط — تخسر تسجيل الخلفية بـWin+G. تقدر تسجّل بـOBS أو أدوات ثانية. قابل للتراجع."),

        ["gamebar_popups_off"] = new(
            WhatEn: "Stops the Game Bar startup panel and its overlay popups.",
            WhatAr: "يوقف لوحة Game Bar عند البدء ونوافذها اللي تطلع فوق اللعبة.",
            WhyEn: "Removes interruptions that pop over full-screen games.",
            WhyAr: "يشيل المقاطعات اللي تطلع فوق الألعاب بملء الشاشة.",
            PerfEn: "Negligible performance effect — mainly stops the distraction.",
            PerfAr: "أثر أداء لا يُذكر — بس يوقف التشتيت.",
            RiskEn: "None. Reversible.",
            RiskAr: "لا مخاطر. قابل للتراجع."),

        ["ultimate_power"] = new(
            WhatEn: "Activates the highest-performance power plan your Windows offers (Ultimate, or High performance).",
            WhatAr: "يفعّل أعلى خطة طاقة تدعمها نسخة ويندوزك (Ultimate أو High performance).",
            WhyEn: "It stops Windows from throttling the CPU to save power, keeping it ready at full speed.",
            WhyAr: "يمنع ويندوز من تخفيض المعالج لتوفير الطاقة، ويخليه جاهز بكامل سرعته.",
            PerfEn: "Real but small — mainly helps desktops. On laptops it noticeably increases power draw and heat, and often isn't worth it on battery.",
            PerfAr: "حقيقي بس بسيط — يفيد الأجهزة المكتبية أكثر. على اللابتوب يرفع استهلاك الطاقة والحرارة بشكل ملحوظ، وغالبًا ما يستاهل على البطارية.",
            RiskEn: "Low — higher power use and heat. Undo sets you back to Balanced.",
            RiskAr: "بسيط — استهلاك طاقة وحرارة أعلى. التراجع يرجّعك لـBalanced."),

        ["hags_on"] = new(
            WhatEn: "Enables Hardware-accelerated GPU Scheduling, letting the GPU manage its own memory.",
            WhatAr: "يفعّل جدولة GPU المسرّعة عتاديًا، فيخلي الكرت يدير ذاكرته بنفسه.",
            WhyEn: "Moving scheduling to the GPU can reduce latency and free the CPU a little.",
            WhyAr: "نقل الجدولة للكرت يقلّل التأخير ويفرّغ المعالج شوي.",
            PerfEn: "Mixed — results vary a lot by GPU and driver. Some see smoother frames, others no change or slightly worse. Worth trying and comparing.",
            PerfAr: "متفاوت — النتائج تختلف كثير حسب الكرت والتعريف. البعض يشوف فريمات أنعم، وآخرون ما يتغيّر أو أسوأ شوي. يستاهل تجربة ومقارنة.",
            RiskEn: "Low — needs a restart to take effect and to reverse. If a game behaves worse, just turn it back off.",
            RiskAr: "بسيط — يحتاج ريستارت عشان يطبّق ويتراجع. لو لعبة صارت أسوأ، بس طفّيه."),

        ["mouse_accel_off"] = new(
            WhatEn: "Turns off mouse acceleration ('Enhance pointer precision'), giving 1:1 raw movement.",
            WhatAr: "يوقف تسارع الماوس (\"تحسين دقة المؤشر\")، فيعطيك حركة خام 1:1.",
            WhyEn: "With acceleration on, the same physical move can travel different distances depending on speed. Off means the same move always goes the same distance.",
            WhyAr: "مع التسارع، نفس الحركة الفيزيائية تقطع مسافات مختلفة حسب السرعة. بدونه، نفس الحركة تقطع نفس المسافة دايمًا.",
            PerfEn: "No effect on frame rate — but it makes aiming consistent, which most gamers strongly prefer.",
            PerfAr: "ما يأثر على الفريمات — بس يخلي التصويب ثابت، واللي أغلب القيمرز يفضّلونه بقوة.",
            RiskEn: "Low — if you're used to acceleration, aiming may feel different at first. Reversible.",
            RiskAr: "بسيط — لو متعوّد على التسارع، التصويب يحس مختلف بالبداية. قابل للتراجع."),

        ["network_latency"] = new(
            WhatEn: "Changes multimedia and network throttling registry values (SystemResponsiveness, NetworkThrottlingIndex) and raises the priority of game tasks.",
            WhatAr: "يغيّر قيم تحديد الوسائط والشبكة في الريجستري (SystemResponsiveness، NetworkThrottlingIndex) ويرفع أولوية مهام الألعاب.",
            WhyEn: "These tell Windows to reserve less capacity for background multimedia while gaming.",
            WhyAr: "تخلي ويندوز يحجز سعة أقل للوسائط بالخلفية وأنت تلعب.",
            PerfEn: "Be honest with yourself here: on modern PCs the real-world effect is tiny to none — this is close to placebo. It will NOT lower your internet ping. Marked 'Advanced' for that reason.",
            PerfAr: "كن صريح مع نفسك هنا: على الأجهزة الحديثة الأثر الفعلي بسيط جدًا لمعدوم — قريب من placebo. ولن يقلّل بينق الإنترنت. مُعلّم \"متقدم\" لهذا السبب.",
            RiskEn: "Low — it edits registry values, all of which the undo restores to Windows defaults.",
            RiskAr: "بسيط — يعدّل قيم ريجستري، وكلها التراجع يرجّعها لإعدادات ويندوز الافتراضية."),
    };
}
