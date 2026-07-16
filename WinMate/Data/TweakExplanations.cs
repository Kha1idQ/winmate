using WinMate.Models;

namespace WinMate.Data;

// Curated, honest explanations keyed by tweak id. The "performance" line tells
// the truth: several of these are about privacy or convenience and change
// nothing about speed, and a couple are near-placebo. Saying so is the point.
public static class TweakExplanations
{
    public static readonly IReadOnlyDictionary<string, Explanation> All = new Dictionary<string, Explanation>
    {
        // ---- Explorer & taskbar ----
        ["file_ext_show"] = new(
            WhatEn: "Tells File Explorer to always show the file type at the end of every name (.txt, .exe, .png).",
            WhatAr: "يخلي مستكشف الملفات يعرض نوع الملف في آخر كل اسم دايم (.txt، .exe، .png).",
            WhyEn: "It helps you catch a program dressed up to look like a document. A simple line of defense against malware that hides behind a fake extension.",
            WhyAr: "يساعدك تكشف برنامج متنكّر بشكل ملف عادي. خط دفاع بسيط ضد الفيروسات اللي تختبي وراء امتداد مزيّف.",
            PerfEn: "It won't change speed. This one is about safety and knowing what you're clicking.",
            PerfAr: "ما له دخل بالسرعة. هذا إعداد أمان وعشان تعرف على وش تضغط.",
            RiskEn: "None. All it does is change what shows up in the name.",
            RiskAr: "ما فيه مخاطر. كل اللي يسويه إنه يغيّر اللي يبان بالاسم."),

        ["hidden_files_show"] = new(
            WhatEn: "Shows the hidden files and folders that Explorer normally keeps out of sight.",
            WhatAr: "يوري الملفات والمجلدات المخفية اللي عادة المستكشف يخبيها.",
            WhyEn: "Handy when you need to get into app data or config folders that Windows tucks away by default.",
            WhyAr: "يفيدك لما تبي توصل مجلدات بيانات البرامج أو الإعدادات اللي ويندوز يخبيها افتراضي.",
            PerfEn: "It won't change speed.",
            PerfAr: "ما له دخل بالسرعة.",
            RiskEn: "Low. System files show up too now, so leave alone anything you don't recognize.",
            RiskAr: "بسيط. بتبين لك ملفات النظام بعد، فأي شي ما تعرفه لا تلمسه."),

        ["taskbar_left"] = new(
            WhatEn: "Moves the taskbar icons over to the left, the way Windows 10 had them.",
            WhatAr: "ينقل أيقونات شريط المهام لجهة اليسار، مثل ما كانت في ويندوز 10.",
            WhyEn: "Pure preference. Plenty of people find the Start button easier to hit down in the bottom-left corner.",
            WhyAr: "على ذوقك. ناس كثير يلقون زر ابدأ أسهل وهو بالزاوية اليسرى تحت.",
            PerfEn: "It won't change speed. This is just how it looks.",
            PerfAr: "ما له دخل بالسرعة. بس شكل.",
            RiskEn: "None.",
            RiskAr: "ما فيه مخاطر."),

        ["widgets_off"] = new(
            WhatEn: "Hides the Widgets button (the weather and news one) from the taskbar.",
            WhatAr: "يخفي زر الودجتس (حق الطقس والأخبار) من شريط المهام.",
            WhyEn: "Gets rid of a button most people never touch, and shuts off its news feed running in the background.",
            WhyAr: "يشيل زر أغلب الناس ما تفتحه، ويطفّي تغذية الأخبار اللي تشتغل بالخلفية.",
            PerfEn: "Tiny at best. The widget feed sips a little memory and network while it runs.",
            PerfAr: "بسيط جدًا بأحسن الأحوال. تغذية الودجتس تاخذ شوي ذاكرة وشبكة وهي شغّالة.",
            RiskEn: "None. Turn it back on whenever you want.",
            RiskAr: "ما فيه مخاطر. رجّعه متى ما بغيت."),

        ["classic_context_menu"] = new(
            WhatEn: "Brings back the full Windows 10 right-click menu so you skip the 'Show more options' extra step.",
            WhatAr: "يرجّع قائمة الزر الأيمن الكاملة من ويندوز 10، وتتخطى خطوة \"إظهار خيارات إضافية\".",
            WhyEn: "Windows 11 buries common actions behind one more click. This puts them all back in front of you.",
            WhyAr: "ويندوز 11 يدفن إجراءات شائعة خلف ضغطة زيادة. هذا يرجّعها كلها قدامك.",
            PerfEn: "It won't change speed, though the old menu tends to feel quicker to use.",
            PerfAr: "ما له دخل بالسرعة، بس القائمة القديمة تحسها أسرع بالاستخدام.",
            RiskEn: "None. You can undo it.",
            RiskAr: "ما فيه مخاطر. تقدر ترجّعه."),

        ["end_task_taskbar"] = new(
            WhatEn: "Adds an 'End Task' option to the right-click menu on the taskbar.",
            WhatAr: "يضيف خيار \"إنهاء المهمة\" لقائمة الزر الأيمن على شريط المهام.",
            WhyEn: "Lets you kill a frozen app right from the taskbar instead of opening Task Manager.",
            WhyAr: "يخليك تقتل برنامج معلّق من شريط المهام على طول بدل ما تفتح مدير المهام.",
            PerfEn: "It won't change speed. It's just there to save you a step.",
            PerfAr: "ما له دخل بالسرعة. بس يوفّر عليك خطوة.",
            RiskEn: "Low. Closing an app this way jumps past its save prompt, so unsaved work can be lost.",
            RiskAr: "بسيط. لما تسكّره كذا يتخطى رسالة الحفظ، فممكن يروح شغل ما حفظته."),

        ["dark_mode_on"] = new(
            WhatEn: "Switches Windows and its apps over to the dark theme.",
            WhatAr: "يحوّل ويندوز وتطبيقاته للثيم الداكن.",
            WhyEn: "Easier on the eyes in a dark room, and on OLED screens it saves a bit of battery.",
            WhyAr: "أريح للعين بالغرفة المظلمة، وعلى شاشات OLED يوفّر شوي بطارية.",
            PerfEn: "It won't change speed. The small battery saving only shows up on OLED displays.",
            PerfAr: "ما له دخل بالسرعة. توفير البطارية البسيط يبان بس على شاشات OLED.",
            RiskEn: "None.",
            RiskAr: "ما فيه مخاطر."),

        ["sticky_keys_off"] = new(
            WhatEn: "Stops the Sticky Keys popup from showing up when you tap Shift five times.",
            WhatAr: "يوقف نافذة المفاتيح الثابتة اللي تطلع لما تضغط Shift خمس مرات.",
            WhyEn: "That popup barges in during games and fast typing. Switching it off gets rid of the interruption.",
            WhyAr: "هالنافذة تقاطعك بالألعاب والكتابة السريعة. طفيها وتخلص من الإزعاج.",
            PerfEn: "It won't change speed.",
            PerfAr: "ما له دخل بالسرعة.",
            RiskEn: "None. It only kills the shortcut popup, not the Sticky Keys accessibility feature in Settings.",
            RiskAr: "ما فيه مخاطر. يطفّي نافذة الاختصار بس، مو ميزة المفاتيح الثابتة نفسها في الإعدادات."),

        // ---- Privacy ----
        ["bing_start_off"] = new(
            WhatEn: "Stops Start menu search from pulling in Bing web results, so it only looks through your files.",
            WhatAr: "يوقف بحث قائمة ابدأ عن جلب نتائج Bing، فيدوّر بس في ملفاتك.",
            WhyEn: "Searching locally is quicker and keeps what you type off Microsoft's servers.",
            WhyAr: "البحث المحلي أسرع ويخلي اللي تكتبه ما يوصل سيرفرات مايكروسوفت.",
            PerfEn: "Search feels a touch snappier since it's not waiting on the web anymore.",
            PerfAr: "البحث يحس أسرع شوي لأنه ما يستنى الإنترنت.",
            RiskEn: "None. You just don't get web results inside Start, and your browser handles those better anyway.",
            RiskAr: "ما فيه مخاطر. بس ما تجيك نتائج ويب داخل ابدأ، والمتصفح أصلًا يسويها أحسن."),

        ["copilot_off"] = new(
            WhatEn: "Turns off the Copilot AI assistant.",
            WhatAr: "يطفّي مساعد كوبايلوت الذكي.",
            WhyEn: "If Copilot isn't something you use, turning it off clears out a feature that just sits in the background.",
            WhyAr: "لو كوبايلوت ما تستخدمه، طفيه وتشيل ميزة قاعدة بالخلفية على الفاضي.",
            PerfEn: "Barely anything. A very small memory saving.",
            PerfAr: "شي ما يذكر. توفير ذاكرة بسيط جدًا.",
            RiskEn: "None. You can turn it back on.",
            RiskAr: "ما فيه مخاطر. تقدر ترجّعه."),

        ["recall_off"] = new(
            WhatEn: "Turns off Windows Recall, the feature that snaps screenshots of your screen every so often to make it searchable.",
            WhatAr: "يطفّي Windows Recall، الميزة اللي تصوّر شاشتك بين فترة وفترة عشان تخليها قابلة للبحث.",
            WhyEn: "This is a serious privacy control. Recall keeps a visual record of everything you do on screen, and a lot of people would rather it stayed off.",
            WhyAr: "هذا ضبط خصوصية مهم. Recall يحفظ سجل بصري لكل اللي تسويه على الشاشة، وكثير يبونه مطفّي.",
            PerfEn: "Can give back some disk space and background CPU, since Recall stops capturing and analyzing screenshots.",
            PerfAr: "يقدر يرجّع لك شوي مساحة دسك ومعالج بالخلفية، لأن Recall يوقف تصوير الشاشة وتحليلها.",
            RiskEn: "Nothing happens to your system. The only thing you lose is the Recall timeline.",
            RiskAr: "ما يصير شي لنظامك. الشي الوحيد اللي تخسره هو سجل Recall."),

        ["location_off"] = new(
            WhatEn: "Blocks Windows and your apps from reading your location.",
            WhatAr: "يمنع ويندوز والتطبيقات من قراءة موقعك.",
            WhyEn: "Privacy. Apps can't see where you are until you switch it back on.",
            WhyAr: "خصوصية. التطبيقات ما تشوف وينك إلا لو رجّعته.",
            PerfEn: "It won't change speed.",
            PerfAr: "ما له دخل بالسرعة.",
            RiskEn: "Low. Anything that leans on location (Maps, Find my device, local weather) stops working until you re-enable it.",
            RiskAr: "بسيط. أي شي يعتمد على الموقع (الخرائط، البحث عن الجهاز، طقس منطقتك) بيوقف لين ترجّعه."),

        ["advertising_id_off"] = new(
            WhatEn: "Turns off the advertising ID that apps lean on to profile you for targeted ads.",
            WhatAr: "يطفّي معرّف الإعلانات اللي التطبيقات تستخدمه تتبعك عشان الإعلانات الموجّهة.",
            WhyEn: "Privacy. Instead of one ID that trails you around, apps get a fresh one that can't be tied back to you.",
            WhyAr: "خصوصية. بدل معرّف واحد يتبعك وين ما رحت، التطبيقات تاخذ واحد جديد ما ينربط فيك.",
            PerfEn: "It won't change speed.",
            PerfAr: "ما له دخل بالسرعة.",
            RiskEn: "None.",
            RiskAr: "ما فيه مخاطر."),

        ["suggestions_off"] = new(
            WhatEn: "Turns off the app suggestions and 'tips' that show up in the Start menu.",
            WhatAr: "يطفّي اقتراحات التطبيقات و\"النصائح\" اللي تطلع في قائمة ابدأ.",
            WhyEn: "Clears out promoted apps and nags you never asked for.",
            WhyAr: "يشيل التطبيقات المروّجة والتنبيهات اللي ما طلبتها.",
            PerfEn: "No real change to speed. You mostly just end up with a cleaner Start menu and no ads.",
            PerfAr: "ما يغيّر السرعة فعليًا. بس تطلع لك قائمة ابدأ أنظف وبدون إعلانات.",
            RiskEn: "None.",
            RiskAr: "ما فيه مخاطر."),

        ["telemetry_off"] = new(
            WhatEn: "Sets telemetry to its lowest level through policy and shuts off the DiagTrack tracking service.",
            WhatAr: "يضبط التتبع لأدنى مستوى عن طريق سياسة، ويطفّي خدمة DiagTrack.",
            WhyEn: "Cuts down how much usage data Windows ships off to Microsoft in the background.",
            WhyAr: "يقلّل كمية بيانات الاستخدام اللي ويندوز يرسلها لمايكروسوفت بالخلفية.",
            PerfEn: "Small background gain. DiagTrack takes some CPU, disk, and network every now and then.",
            PerfAr: "مكسب بسيط بالخلفية. DiagTrack تاخذ شوي معالج ودسك وشبكة كل فترة.",
            RiskEn: "Low. Some Windows feedback and diagnostics send less data. You can put it back.",
            RiskAr: "بسيط. بعض ميزات الملاحظات والتشخيص بترسل بيانات أقل. تقدر ترجّعه."),

        ["sched_tasks_off"] = new(
            WhatEn: "Turns off the Compatibility Appraiser and CEIP scheduled tasks that gather usage data.",
            WhatAr: "يطفّي مهام تقييم التوافق و CEIP المجدولة اللي تجمع بيانات الاستخدام.",
            WhyEn: "These run in the background for one reason: collecting telemetry. Turn them off and that stops.",
            WhyAr: "هالمهام تشتغل بالخلفية لسبب واحد وهو جمع التتبع. طفيها ويوقف.",
            PerfEn: "Occasional background gain. They can spike CPU and disk for a moment while they run.",
            PerfAr: "مكسب متقطّع بالخلفية. تقدر ترفع المعالج والدسك لحظة وهي شغّالة.",
            RiskEn: "None for normal use. You can put it back.",
            RiskAr: "ما فيه مخاطر على الاستخدام العادي. تقدر ترجّعه."),

        // ---- Performance ----
        ["hibernate_off"] = new(
            WhatEn: "Turns off hibernation and removes the hidden hiberfil.sys file.",
            WhatAr: "يطفّي السبات ويشيل ملف hiberfil.sys المخفي.",
            WhyEn: "That file is as big as your RAM, often several GB. If you never hibernate, it's just space sitting there doing nothing.",
            WhyAr: "الملف بحجم رامك، غالبًا عدة قيقا. لو ما تستخدم السبات، فهو مساحة قاعدة على الفاضي.",
            PerfEn: "Frees up real disk space (several GB). It doesn't touch speed beyond that.",
            PerfAr: "يفرّغ مساحة دسك حقيقية (عدة قيقا). غير كذا ما يمس السرعة.",
            RiskEn: "Low. You lose Hibernate from the shutdown menu, and on some laptops it also switches off Fast Startup. You can put it back.",
            RiskAr: "بسيط. تخسر السبات من قائمة الإيقاف، وعلى بعض اللابتوبات يطفّي كمان Fast Startup. تقدر ترجّعه."),

        ["dns_cloudflare"] = new(
            WhatEn: "Points your network adapters at Cloudflare's DNS servers (1.1.1.1).",
            WhatAr: "يوجّه كروت الشبكة تستخدم خوادم DNS حق كلاود فلير (1.1.1.1).",
            WhyEn: "DNS is what turns a website name into an address. Cloudflare's is often quicker and more private than your ISP's.",
            WhyAr: "الـDNS هو اللي يحوّل اسم الموقع لعنوان. حق كلاود فلير غالبًا أسرع وأكثر خصوصية من حق مزوّدك.",
            PerfEn: "Can get websites starting to load a little sooner (name lookups are faster). It does not raise your download speed.",
            PerfAr: "يقدر يخلي المواقع تبدأ التحميل أسرع شوي (بحث الأسماء أسرع). لكنه ما يزيد سرعة التحميل نفسها.",
            RiskEn: "Low. If a network needs its own DNS (some workplaces do), the undo sets DNS back to automatic.",
            RiskAr: "بسيط. لو شبكة تحتاج DNS خاص فيها (بعض الشركات كذا)، التراجع يرجّع DNS للتلقائي."),

        // ---- Cleanup & debloat (one-shot) ----
        ["onedrive_remove"] = new(
            WhatEn: "Uninstalls OneDrive completely. Your local files stay right where they are.",
            WhatAr: "يحذف OneDrive بالكامل. ملفاتك المحلية تبقى مكانها.",
            WhyEn: "If you don't use Microsoft's cloud sync, OneDrive just runs in the background and keeps pestering you to sign in.",
            WhyAr: "لو ما تستخدم مزامنة مايكروسوفت السحابية، OneDrive بس يشتغل بالخلفية ويلح عليك تسجّل دخول.",
            PerfEn: "Small background gain. While it's running, OneDrive uses memory and syncs in the background.",
            PerfAr: "مكسب بسيط بالخلفية. وهو شغّال، OneDrive ياخذ ذاكرة ويزامن بالخلفية.",
            RiskEn: "Medium. Files that live ONLY in OneDrive's cloud stop syncing, so back up anything important first.",
            RiskAr: "متوسط. الملفات اللي موجودة بس في سحابة OneDrive بتوقف تتزامن، فاحفظ المهم قبل."),

        ["debloat_appx"] = new(
            WhatEn: "Removes a batch of preinstalled apps: News, Weather, Solitaire, Feedback Hub, and others.",
            WhatAr: "يحذف مجموعة برامج مثبتة مسبقًا: الأخبار، الطقس، سوليتير، مركز الملاحظات، وغيرها.",
            WhyEn: "These ship with Windows, hardly anyone opens them, and they crowd up the Start menu.",
            WhyAr: "هذي تجي مع ويندوز، ونادر أحد يفتحها، وتزحم قائمة ابدأ.",
            PerfEn: "Minor. Frees a little disk and trims a few Start-menu entries. It isn't a real speed change.",
            PerfAr: "بسيط. يفرّغ شوي دسك ويشيل بعض عناصر ابدأ. مو تغيير سرعة حقيقي.",
            RiskEn: "Low, but you can't undo it from here. The confirmation dialog spells out exactly what gets removed, and you can reinstall any of them from the Store.",
            RiskAr: "بسيط، بس ما يتراجع من هنا. نافذة التأكيد توري بالضبط وش بينحذف، وتقدر تعيد تثبيت أي واحد منها من المتجر."),

        ["clean_temp"] = new(
            WhatEn: "Empties out your user and Windows temporary folders.",
            WhatAr: "يفضّي مجلدات temp حقتك وحق الويندوز.",
            WhyEn: "Temp files are leftovers that programs are done with. Clearing them gives you back some space.",
            WhyAr: "ملفات temp بقايا خلصت منها البرامج. تنظيفها يرجّع لك شوي مساحة.",
            PerfEn: "Frees disk space. It won't make the PC faster, and any file that's in use gets skipped safely.",
            PerfAr: "يفرّغ مساحة دسك. ما يخلي الجهاز أسرع، وأي ملف مستخدم يتخطاه بأمان.",
            RiskEn: "Very low. Windows just rebuilds any temp file it needs. Runs once.",
            RiskAr: "بسيط جدًا. ويندوز يعيد بناء أي ملف مؤقت يحتاجه. يشتغل مرة وحدة."),

        // ---- Gaming ----
        ["game_mode_on"] = new(
            WhatEn: "Turns on Windows Game Mode.",
            WhatAr: "يشغّل وضع الألعاب في ويندوز.",
            WhyEn: "Game Mode tells Windows to put the game first and hold off on background work while you play.",
            WhyAr: "وضع الألعاب يقول لويندوز يعطي اللعبة الأولوية ويأجّل شغل الخلفية وأنت تلعب.",
            PerfEn: "Small and depends on the situation. It can smooth out frame dips on a busy system, but don't expect a big FPS jump.",
            PerfAr: "بسيط وعلى حسب الحالة. يقدر يقلّل تقطّع الفريمات على جهاز مزدحم، بس لا تتوقع قفزة FPS كبيرة.",
            RiskEn: "None. You can turn it back off.",
            RiskAr: "ما فيه مخاطر. تقدر تطفّيه."),

        ["gamedvr_off"] = new(
            WhatEn: "Turns off Xbox Game DVR, the recorder that runs in the background while you game.",
            WhatAr: "يطفّي Xbox Game DVR، المسجّل اللي يشتغل بالخلفية وأنت تلعب.",
            WhyEn: "Background recording is constantly capturing your gameplay, which eats GPU time even if you never touch the clips.",
            WhyAr: "التسجيل بالخلفية قاعد يصوّر لعبك على طول، فياكل من وقت الكرت حتى لو ما تفتح المقاطع أبدًا.",
            PerfEn: "This is one of the gaming tweaks that actually does something. It can lift frame rates by a noticeable amount, especially on mid-range PCs.",
            PerfAr: "هذا من تعديلات الألعاب اللي فعلًا لها أثر. يقدر يرفع الفريمات بشكل تحس فيه، خصوصًا على الأجهزة المتوسطة.",
            RiskEn: "Low. You lose the Win+G background recording, but you can still record with OBS or something similar. You can turn it back on.",
            RiskAr: "بسيط. تخسر تسجيل الخلفية بـWin+G، بس تقدر تسجّل بـOBS أو شي مثله. تقدر ترجّعه."),

        ["gamebar_popups_off"] = new(
            WhatEn: "Stops the Game Bar startup panel and the overlay popups that come with it.",
            WhatAr: "يوقف لوحة Game Bar عند التشغيل والنوافذ اللي تطلع فوق اللعبة.",
            WhyEn: "Gets rid of interruptions that pop over your full-screen games.",
            WhyAr: "يشيل المقاطعات اللي تطلع فوق ألعابك اللي بملء الشاشة.",
            PerfEn: "Barely touches performance. Mostly it just stops the distraction.",
            PerfAr: "أثره على الأداء ما يذكر. بس يوقف التشتيت.",
            RiskEn: "None. You can turn it back on.",
            RiskAr: "ما فيه مخاطر. تقدر ترجّعه."),

        ["ultimate_power"] = new(
            WhatEn: "Switches you to the highest-performance power plan your Windows has (Ultimate, or High performance).",
            WhatAr: "يحوّلك لأعلى خطة طاقة عندك في ويندوز (Ultimate أو High performance).",
            WhyEn: "It keeps Windows from throttling the CPU to save power, so it stays ready to run at full speed.",
            WhyAr: "يمنع ويندوز من إنه يخفّض المعالج عشان يوفّر طاقة، فيبقى جاهز يشتغل بكامل سرعته.",
            PerfEn: "Real but small. It mainly helps desktops. On a laptop it clearly raises power draw and heat, and on battery it's usually not worth it.",
            PerfAr: "حقيقي بس بسيط. يفيد الأجهزة المكتبية أكثر. على اللابتوب يرفع استهلاك الطاقة والحرارة بشكل واضح، ووأنت على البطارية غالبًا ما يستاهل.",
            RiskEn: "Low. More power draw and heat. The undo drops you back to Balanced.",
            RiskAr: "بسيط. استهلاك طاقة وحرارة أعلى. التراجع يرجّعك لـBalanced."),

        ["hags_on"] = new(
            WhatEn: "Turns on Hardware-accelerated GPU Scheduling, which lets the GPU handle its own memory.",
            WhatAr: "يشغّل جدولة GPU المسرّعة عتاديًا، اللي تخلي الكرت يدير ذاكرته بنفسه.",
            WhyEn: "Handing scheduling over to the GPU can trim latency and take a little load off the CPU.",
            WhyAr: "لما تسلّم الجدولة للكرت، يقدر يقلّل التأخير ويخفّف شوي عن المعالج.",
            PerfEn: "Mixed. Results swing a lot depending on your GPU and driver. Some people get smoother frames, others notice nothing or a slight drop. Worth trying and seeing for yourself.",
            PerfAr: "متفاوت. النتيجة تختلف كثير حسب كرتك والتعريف. ناس تجيهم فريمات أنعم، وناس ما يتغيّر عندهم شي أو ينزل شوي. يستاهل تجرّبه وتشوف بنفسك.",
            RiskEn: "Low. It needs a restart to kick in and another to reverse. If a game runs worse, just switch it back off.",
            RiskAr: "بسيط. يحتاج ريستارت عشان يشتغل وريستارت ثاني عشان يرجع. لو لعبة صارت أسوأ، طفّيه وخلاص."),

        ["mouse_accel_off"] = new(
            WhatEn: "Turns off mouse acceleration ('Enhance pointer precision') so you get raw 1:1 movement.",
            WhatAr: "يطفّي تسارع الماوس (\"تحسين دقة المؤشر\") فتصير حركتك خام 1:1.",
            WhyEn: "With acceleration on, the same hand movement can cover different distances depending on how fast you move. Off, the same movement always covers the same distance.",
            WhyAr: "مع التسارع، نفس حركة يدك تقطع مسافات مختلفة حسب سرعتك. بدونه، نفس الحركة تقطع نفس المسافة على طول.",
            PerfEn: "Doesn't touch frame rate, but it makes your aim consistent, which most gamers strongly prefer.",
            PerfAr: "ما يمس الفريمات، بس يخلي تصويبك ثابت، وهذا اللي أغلب القيمرز يفضّلونه بقوة.",
            RiskEn: "Low. If you're used to acceleration, your aim might feel off at first. You can turn it back on.",
            RiskAr: "بسيط. لو متعوّد على التسارع، بيحس تصويبك غريب بالبداية. تقدر ترجّعه."),

        ["network_latency"] = new(
            WhatEn: "Changes the multimedia and network throttling registry values (SystemResponsiveness, NetworkThrottlingIndex) and bumps up the priority of game tasks.",
            WhatAr: "يغيّر قيم تحديد الوسائط والشبكة في الريجستري (SystemResponsiveness، NetworkThrottlingIndex) ويرفع أولوية مهام الألعاب.",
            WhyEn: "They tell Windows to hold back less capacity for background multimedia while you're gaming.",
            WhyAr: "تخلي ويندوز يحجز سعة أقل للوسائط بالخلفية وأنت تلعب.",
            PerfEn: "Be straight with yourself on this one: on a modern PC the real-world effect is tiny to none, close to placebo. It will NOT lower your internet ping. That's why it's tagged 'Advanced'.",
            PerfAr: "كن صريح مع نفسك هنا: على جهاز حديث الأثر الفعلي بسيط جدًا لدرجة معدوم، قريب من الـplacebo. وما بيقلّل بينق الإنترنت. لهذا معلّم \"متقدم\".",
            RiskEn: "Low. It edits registry values, and the undo returns every one of them to Windows defaults.",
            RiskAr: "بسيط. يعدّل قيم ريجستري، والتراجع يرجّعها كلها لإعدادات ويندوز الافتراضية."),
    };
}
