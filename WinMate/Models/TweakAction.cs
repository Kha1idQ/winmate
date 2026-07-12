using Microsoft.Win32;

namespace WinMate.Models;

// One atomic step a tweak performs. A tweak is a list of these.
public abstract record TweakAction;

// Write a registry value. Value == null means delete the value.
public record RegistryAction(
    RegistryHive Hive,
    string Key,
    string Name,
    RegistryValueKind Kind,
    object? Value) : TweakAction;

// Delete an entire registry key (used by classic context menu undo).
public record RegistryDeleteKeyAction(RegistryHive Hive, string Key) : TweakAction;

// Run a PowerShell script (appx removal, temp cleaning, restore point).
public record PowerShellAction(string Script) : TweakAction;

// Run an external command (powercfg, schtasks, sc.exe).
public record CommandAction(string Exe, string Args) : TweakAction;

// Change a Windows service startup type: "Disabled" / "Manual" / "Automatic".
public record ServiceAction(string ServiceName, string StartupType) : TweakAction;
