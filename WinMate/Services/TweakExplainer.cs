using Microsoft.Win32;
using WinMate.Data;
using WinMate.Models;

namespace WinMate.Services;

// Assembles the "Explain this" content: four curated lines from TweakExplanations
// plus three lines derived straight from the tweak's own actions, so admin/restart/
// undo can never disagree with what the tweak really does.
public static class TweakExplainer
{
    public static bool HasExplanation(Tweak tweak) => TweakExplanations.All.ContainsKey(tweak.Id);

    public record Section(string Question, string Answer);

    public static IReadOnlyList<Section> Build(Tweak tweak)
    {
        var e = TweakExplanations.All.GetValueOrDefault(tweak.Id);
        var sections = new List<Section>();

        if (e is not null)
        {
            sections.Add(new(L("What it changes", "ماذا يغيّر"), LocalizationService.Pick(e.WhatEn, e.WhatAr)));
            sections.Add(new(L("Why you might want it", "لماذا قد تحتاجه"), LocalizationService.Pick(e.WhyEn, e.WhyAr)));
            sections.Add(new(L("Does it really help performance?", "هل يحسّن الأداء فعلًا؟"), LocalizationService.Pick(e.PerfEn, e.PerfAr)));
            sections.Add(new(L("Risks", "المخاطر"), LocalizationService.Pick(e.RiskEn, e.RiskAr)));
        }

        sections.Add(new(L("Needs administrator?", "يحتاج صلاحية مدير؟"), NeedsAdmin(tweak)));
        sections.Add(new(L("Needs a restart?", "يحتاج إعادة تشغيل؟"), NeedsRestart(tweak)));
        sections.Add(new(L("How to undo it", "كيف تتراجع عنه"), UndoInfo(tweak)));

        return sections;
    }

    // Derived: any action that writes HKLM, changes a service, or runs schtasks/
    // powercfg needs elevation. Pure HKCU tweaks technically don't, but WinMate
    // always runs elevated anyway.
    private static string NeedsAdmin(Tweak tweak)
    {
        var all = tweak.Apply.Concat(tweak.Undo);
        var elevated = all.Any(a => a switch
        {
            RegistryAction r => r.Hive == RegistryHive.LocalMachine,
            RegistryDeleteKeyAction d => d.Hive == RegistryHive.LocalMachine,
            ServiceAction => true,
            CommandAction => true,
            PowerShellAction => true,
            _ => false,
        });

        return elevated
            ? L("Yes — it changes system-wide settings, so it runs with administrator rights (WinMate already has them).",
                "نعم — يغيّر إعدادات على مستوى النظام، فيشتغل بصلاحيات مدير (وين مايت عنده الصلاحية أصلًا).")
            : L("No — it only changes your own user settings.",
                "لا — يغيّر بس إعداداتك الشخصية.");
    }

    private static string NeedsRestart(Tweak tweak)
    {
        if (tweak.RequiresRestart)
            return L("Yes — restart your PC for this change to fully take effect.",
                     "نعم — أعد تشغيل الجهاز عشان يطبّق التغيير بالكامل.");
        if (tweak.RestartExplorer)
            return L("No full restart — WinMate briefly restarts File Explorer for you, so the screen flickers once.",
                     "ما يحتاج إعادة تشغيل كاملة — وين مايت يعيد تشغيل المستكشف لك، فالشاشة ترمش مرة.");
        return L("No — it takes effect right away.",
                 "لا — يطبّق فورًا.");
    }

    private static string UndoInfo(Tweak tweak)
    {
        if (tweak.IsReversible)
            return L("Just switch the toggle back off — WinMate restores the exact previous setting.",
                     "بس رجّع المفتاح للإيقاف — وين مايت يرجّع الإعداد السابق بالضبط.");
        return L("This is a one-time action and can't be undone from WinMate. You'll be asked to confirm before it runs.",
                 "هذا إجراء لمرة واحدة وما يتراجع من وين مايت. بيُطلب منك تأكيد قبل ما يشتغل.");
    }

    private static string L(string en, string ar) => LocalizationService.Pick(en, ar);
}
