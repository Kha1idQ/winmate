namespace WinMate.Services;

public static class SearchMatch
{
    // Case-insensitive "contains" across any of the given fields.
    // Empty query matches everything.
    public static bool Matches(string query, params string[] fields)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        return fields.Any(f =>
            f?.Contains(query, StringComparison.OrdinalIgnoreCase) == true);
    }
}
