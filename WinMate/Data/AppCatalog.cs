using WinMate.Models;

namespace WinMate.Data;

public static class AppCatalog
{
    // Category ids map to localization keys: Cat_browsers, Cat_dev, ...
    public static readonly string[] Categories = ["browsers", "dev", "gaming", "media", "utils"];

    public static readonly IReadOnlyList<AppItem> All =
    [
        new("Google.Chrome",                 "Google Chrome",       "قوقل كروم",          "browsers"),
        new("Mozilla.Firefox",               "Firefox",             "فايرفوكس",           "browsers"),
        new("Brave.Brave",                   "Brave",               "بريف",               "browsers"),
        new("Opera.OperaGX",                 "Opera GX",            "أوبرا GX",           "browsers"),

        new("Microsoft.VisualStudioCode",    "VS Code",             "VS Code",            "dev"),
        new("Git.Git",                       "Git",                 "Git",                "dev"),
        new("Python.Python.3.13",            "Python 3.13",         "بايثون 3.13",        "dev"),
        new("OpenJS.NodeJS.LTS",             "Node.js LTS",         "Node.js LTS",        "dev"),
        new("Notepad++.Notepad++",           "Notepad++",           "Notepad++",          "dev"),

        new("Valve.Steam",                   "Steam",               "ستيم",               "gaming"),
        new("EpicGames.EpicGamesLauncher",   "Epic Games",          "إيبك قيمز",          "gaming"),
        new("GOG.Galaxy",                    "GOG Galaxy",          "GOG Galaxy",         "gaming"),
        new("Discord.Discord",               "Discord",             "ديسكورد",            "gaming"),
        new("ElectronicArts.EADesktop",      "EA App",              "تطبيق EA",           "gaming"),

        new("VideoLAN.VLC",                  "VLC",                 "VLC",                "media"),
        new("Spotify.Spotify",               "Spotify",             "سبوتيفاي",           "media"),
        new("OBSProject.OBSStudio",          "OBS Studio",          "OBS Studio",         "media"),

        new("7zip.7zip",                     "7-Zip",               "7-Zip",              "utils"),
        new("Microsoft.PowerToys",           "PowerToys",           "PowerToys",          "utils"),
        new("qBittorrent.qBittorrent",       "qBittorrent",         "qBittorrent",        "utils"),
        new("Bitwarden.Bitwarden",           "Bitwarden",           "بت واردن",           "utils"),
        new("WinDirStat.WinDirStat",         "WinDirStat",          "WinDirStat",         "utils"),
    ];
}
