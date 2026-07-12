using Microsoft.Win32;

namespace WinMate.Services;

public static class RegistryService
{
    // Always use the 64-bit registry view so writes don't get redirected
    // to the WOW6432Node shadow keys.
    private static RegistryKey OpenBase(RegistryHive hive) =>
        RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);

    public static object? GetValue(RegistryHive hive, string key, string name)
    {
        using var baseKey = OpenBase(hive);
        using var sub = baseKey.OpenSubKey(key);
        return sub?.GetValue(name);
    }

    public static void SetValue(RegistryHive hive, string key, string name, RegistryValueKind kind, object value)
    {
        using var baseKey = OpenBase(hive);
        using var sub = baseKey.CreateSubKey(key, writable: true)
            ?? throw new InvalidOperationException($"Cannot open or create {hive}\\{key}");
        sub.SetValue(name, value, kind);
    }

    public static void DeleteValue(RegistryHive hive, string key, string name)
    {
        using var baseKey = OpenBase(hive);
        using var sub = baseKey.OpenSubKey(key, writable: true);
        sub?.DeleteValue(name, throwOnMissingValue: false);
    }

    public static void DeleteKey(RegistryHive hive, string key)
    {
        using var baseKey = OpenBase(hive);
        baseKey.DeleteSubKeyTree(key, throwOnMissingSubKey: false);
    }
}
