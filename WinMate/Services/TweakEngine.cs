using WinMate.Models;

namespace WinMate.Services;

public static class TweakEngine
{
    public static Task ApplyAsync(Tweak tweak) => RunActionsAsync(tweak, tweak.Apply);

    public static Task UndoAsync(Tweak tweak) => RunActionsAsync(tweak, tweak.Undo);

    // Reads the actual system state — the UI toggle must always reflect
    // this, never just what the user clicked.
    public static async Task<bool> IsAppliedAsync(Tweak tweak)
    {
        if (tweak.CustomCheck is not null)
            return await tweak.CustomCheck();

        if (tweak.StateCheck is null)
            return false;

        var check = tweak.StateCheck;
        var value = await Task.Run(() => RegistryService.GetValue(check.Hive, check.Key, check.Name));
        return value is not null && value.ToString() == check.ExpectedValue.ToString();
    }

    private static async Task RunActionsAsync(Tweak tweak, IReadOnlyList<TweakAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case RegistryAction { Value: null } reg:
                    await Task.Run(() => RegistryService.DeleteValue(reg.Hive, reg.Key, reg.Name));
                    LogService.Write($"[{tweak.Id}] reg delete {reg.Hive}\\{reg.Key} :: {reg.Name}");
                    break;

                case RegistryAction reg:
                    await Task.Run(() => RegistryService.SetValue(reg.Hive, reg.Key, reg.Name, reg.Kind, reg.Value!));
                    LogService.Write($"[{tweak.Id}] reg set {reg.Hive}\\{reg.Key} :: {reg.Name} = {reg.Value}");
                    break;

                case RegistryDeleteKeyAction del:
                    await Task.Run(() => RegistryService.DeleteKey(del.Hive, del.Key));
                    LogService.Write($"[{tweak.Id}] reg delete key {del.Hive}\\{del.Key}");
                    break;

                case PowerShellAction ps:
                    LogService.Write($"[{tweak.Id}] powershell ...");
                    await ProcessRunner.RunAsync("powershell.exe",
                        $"-NoProfile -ExecutionPolicy Bypass -Command \"{ps.Script.Replace("\"", "\\\"")}\"",
                        LogService.WriteRaw);
                    break;

                case CommandAction cmd:
                    LogService.Write($"[{tweak.Id}] {cmd.Exe} {cmd.Args}");
                    await ProcessRunner.RunAsync(cmd.Exe, cmd.Args, LogService.WriteRaw);
                    break;

                case ServiceAction svc:
                    LogService.Write($"[{tweak.Id}] service {svc.ServiceName} -> {svc.StartupType}");
                    var scStart = svc.StartupType switch
                    {
                        "Disabled" => "disabled",
                        "Manual" => "demand",
                        _ => "auto",
                    };
                    await ProcessRunner.RunAsync("sc.exe", $"config {svc.ServiceName} start= {scStart}", LogService.WriteRaw);
                    if (svc.StartupType == "Disabled")
                        await ProcessRunner.RunAsync("sc.exe", $"stop {svc.ServiceName}", LogService.WriteRaw);
                    break;
            }
        }
    }

    // Explorer must be restarted for some shell tweaks to take effect.
    public static Task RestartExplorerAsync()
    {
        LogService.Write("Restarting Windows Explorer...");
        return ProcessRunner.RunAsync("cmd.exe", "/c taskkill /f /im explorer.exe & start explorer.exe");
    }
}
