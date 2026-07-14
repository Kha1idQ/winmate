namespace WinMate.Models;

// One row from "winget list": an app already on this machine.
public record InstalledApp(
    string Name,
    string Id,
    string Version,
    string Available,
    string Source)
{
    // winget only fills the Available column when a newer version exists.
    public bool HasUpdate => !string.IsNullOrWhiteSpace(Available);

    // Packages winget doesn't know a source for can't be updated or repaired.
    public bool IsManageable => !string.IsNullOrWhiteSpace(Source);
}
