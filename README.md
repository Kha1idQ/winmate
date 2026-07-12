# WinMate 🔧

**A modern Windows utility — install apps, debloat, and optimize for gaming. Bilingual (English / العربية).**

**أداة ويندوز حديثة — ثبّت برامجك، نظّف النظام، وحسّنه للقيمنق. ثنائية اللغة (عربي / إنجليزي).**

![.NET 9](https://img.shields.io/badge/.NET-9.0-gold) ![WPF](https://img.shields.io/badge/UI-WPF--UI%20Fluent-black)

---

## Install / التثبيت

Run this one line in PowerShell / شغّل هذا السطر في PowerShell:

```powershell
irm https://raw.githubusercontent.com/OWNER/winmate/main/scripts/install.ps1 | iex
```

Or download `WinMate.exe` from [Releases](../../releases) — no installation needed, just run it.

أو نزّل `WinMate.exe` من صفحة الإصدارات — ما يحتاج تثبيت، شغّله مباشرة.

## Features / المميزات

| | English | عربي |
|---|---|---|
| 📦 | **Apps** — install 22+ popular apps via winget with one click, with live progress log | **البرامج** — ثبّت أشهر البرامج بضغطة واحدة عبر winget مع سجل مباشر |
| 🛠️ | **Tweaks** — disable telemetry, debloat preinstalled apps, classic context menu, privacy switches — **every switch is reversible** | **التحسينات** — إيقاف التتبع، حذف البلوت، القائمة الكلاسيكية، مفاتيح الخصوصية — **كل مفتاح قابل للتراجع** |
| 🎮 | **Gaming** — Game Mode, disable Xbox DVR, max performance power plan, GPU scheduling, raw mouse | **الألعاب** — وضع الألعاب، إيقاف Xbox DVR، خطة الطاقة القصوى، جدولة GPU، ماوس خام |
| 🛡️ | **Safety first** — offers a System Restore Point before your first change | **الأمان أولًا** — يعرض إنشاء نقطة استعادة قبل أول تعديل |
| 🌐 | **Arabic + English** — live language toggle with full RTL support | **عربي + إنجليزي** — تبديل حي مع دعم كامل للاتجاه من اليمين لليسار |

## Why WinMate? / ليش وين مايت؟

- Every toggle reads the **real system state** from the registry — not just what you clicked.
- Reversibility is a core feature: applied a tweak and changed your mind? Flip the switch back.
- Irreversible actions (debloat, temp clean) always show a confirmation with the exact list of what will happen.

- كل مفتاح يقرأ **حالة النظام الحقيقية** من الريجستري — مو مجرد شكل.
- القابلية للتراجع ميزة أساسية: فعّلت تويك وغيّرت رأيك؟ رجّع المفتاح وخلاص.
- الإجراءات النهائية (حذف البلوت، التنظيف) تعرض تأكيدًا بقائمة واضحة قبل التنفيذ.

## Requirements / المتطلبات

- Windows 10/11 (64-bit)
- Administrator rights (UAC prompt on launch) / صلاحيات مدير
- [winget](https://aka.ms/getwinget) for the Apps tab / لتبويب البرامج

## Build from source / البناء من المصدر

```powershell
git clone https://github.com/OWNER/winmate.git
cd winmate/WinMate
dotnet run
```

## License

MIT
