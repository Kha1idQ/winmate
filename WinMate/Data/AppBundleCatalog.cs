using WinMate.Models;

namespace WinMate.Data;

public static class AppBundleCatalog
{
    public static readonly IReadOnlyList<AppBundle> All =
    [
        new(
            Id: "essentials",
            NameEn: "Essentials",
            NameAr: "الأساسيات",
            Icon: "star",
            WingetIds: ["Google.Chrome", "7zip.7zip", "VideoLAN.VLC", "Bitwarden.Bitwarden", "Microsoft.PowerToys"]),

        new(
            Id: "gamer",
            NameEn: "Gamer Pack",
            NameAr: "حزمة القيمر",
            Icon: "gamepad",
            WingetIds: ["Valve.Steam", "Discord.Discord", "EpicGames.EpicGamesLauncher", "GOG.Galaxy", "OBSProject.OBSStudio"]),

        new(
            Id: "developer",
            NameEn: "Developer Pack",
            NameAr: "حزمة المطور",
            Icon: "code",
            WingetIds: ["Microsoft.VisualStudioCode", "Git.Git", "OpenJS.NodeJS.LTS", "Python.Python.3.13", "Notepad++.Notepad++"]),
    ];
}
