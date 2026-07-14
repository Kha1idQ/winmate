using Microsoft.Win32;

namespace WinMate.Models;

// How we detect whether a tweak is currently applied: read one registry
// value and compare it to the expected value.
public record RegistryCheck(RegistryHive Hive, string Key, string Name, object ExpectedValue);

public record Tweak(
    string Id,
    string NameEn,
    string NameAr,
    string DescEn,
    string DescAr,
    string Category,
    IReadOnlyList<TweakAction> Apply,
    IReadOnlyList<TweakAction> Undo,
    RegistryCheck? StateCheck,
    bool RequiresRestart = false)
{
    // Escape hatch for tweaks whose state isn't a simple registry value
    // (e.g. active power plan checked via powercfg output).
    public Func<Task<bool>>? CustomCheck { get; init; }

    // Optional leading icon tile (icon-pack name, used on the Gaming page).
    public string? Icon { get; init; }

    // Shows an "Advanced" chip — power-user tweaks with wider side effects.
    public bool IsAdvanced { get; init; }

    // Shell tweaks (file extensions, classic context menu...) only take
    // effect after Windows Explorer restarts.
    public bool RestartExplorer { get; init; }

    // Irreversible tweaks (empty Undo) render as confirm-buttons, not toggles.
    public bool IsReversible => Undo.Count > 0;
}
