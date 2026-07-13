using WinMate.Models;
using Wpf.Ui.Controls;

namespace WinMate.Data;

public static class AppBundleCatalog
{
    public static readonly IReadOnlyList<AppBundle> All =
    [
        new(
            Id: "essentials",
            NameEn: "Essentials",
            NameAr: "الأساسيات",
            Icon: SymbolRegular.Star24,
            WingetIds: ["Google.Chrome", "7zip.7zip", "VideoLAN.VLC", "Bitwarden.Bitwarden", "Microsoft.PowerToys"]),

        new(
            Id: "gamer",
            NameEn: "Gamer pack",
            NameAr: "حزمة القيمر",
            Icon: SymbolRegular.XboxController24,
            WingetIds: ["Valve.Steam", "Discord.Discord", "EpicGames.EpicGamesLauncher", "GOG.Galaxy", "OBSProject.OBSStudio"]),

        new(
            Id: "developer",
            NameEn: "Developer pack",
            NameAr: "حزمة المطور",
            Icon: SymbolRegular.Code24,
            WingetIds: ["Microsoft.VisualStudioCode", "Git.Git", "OpenJS.NodeJS.LTS", "Python.Python.3.13", "Notepad++.Notepad++"]),
    ];
}
