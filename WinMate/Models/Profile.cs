namespace WinMate.Models;

// A one-click preset: applies a named group of reversible tweaks together.
public record Profile(
    string Id,
    string NameEn,
    string NameAr,
    string DescEn,
    string DescAr,
    string Icon,
    string Tile,
    IReadOnlyList<string> TweakIds);
