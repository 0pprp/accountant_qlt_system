namespace Application.SalesExcelSearch.Common;

/// <summary>
/// تطبيع عربي + مطابقة أسماء — منقول من BE_Company.Sales.Services.SalesArabicText / SalesCustomerNameMatch.
/// </summary>
public static class SalesArabicText
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value
            .Replace("أ", "ا", StringComparison.Ordinal)
            .Replace("إ", "ا", StringComparison.Ordinal)
            .Replace("آ", "ا", StringComparison.Ordinal)
            .Replace("ٱ", "ا", StringComparison.Ordinal)
            .Replace("ى", "ي", StringComparison.Ordinal)
            .Replace("ـ", "", StringComparison.Ordinal);

        var parts = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }
}

public static class SalesCustomerNameMatch
{
    public const int MinimumLength = 2;
    public const int MaxNames = 1500;
    public const int MaxMatchesPerName = 80;

    public static int WordCount(string? normalized) =>
        string.IsNullOrWhiteSpace(normalized)
            ? 0
            : normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    public static bool UsesFamilySearch(string? normalizedQuery) => WordCount(normalizedQuery) >= 3;

    public static string FamilySearchKey(string? normalizedQuery)
    {
        var normalized = SalesArabicText.Normalize(normalizedQuery);
        return UsesFamilySearch(normalized) ? DropFirstWord(normalized) : normalized;
    }

    public static bool IsMatch(string? storedName, string normalizedQuery)
    {
        if (string.IsNullOrWhiteSpace(normalizedQuery))
            return false;
        if (UsesFamilySearch(normalizedQuery))
            return IsFamilyMatch(storedName, FamilySearchKey(normalizedQuery));
        return IsLogicalMatch(storedName, normalizedQuery);
    }

    public static bool IsLogicalMatch(string? storedName, string normalizedQuery)
    {
        if (string.IsNullOrWhiteSpace(normalizedQuery))
            return false;

        var stored = SalesArabicText.Normalize(storedName);
        if (stored.Length == 0)
            return false;
        if (string.Equals(stored, normalizedQuery, StringComparison.Ordinal))
            return true;
        if (normalizedQuery.IndexOf(' ') < 0)
            return false;
        return stored.StartsWith(normalizedQuery + " ", StringComparison.Ordinal);
    }

    public static bool IsFamilyMatch(string? storedName, string familyKey)
    {
        if (string.IsNullOrWhiteSpace(familyKey) || familyKey.IndexOf(' ') < 0)
            return false;

        var storedFamily = DropFirstWord(SalesArabicText.Normalize(storedName));
        if (storedFamily.Length == 0)
            return false;
        if (string.Equals(storedFamily, familyKey, StringComparison.Ordinal))
            return true;
        return storedFamily.StartsWith(familyKey + " ", StringComparison.Ordinal);
    }

    public static string DropFirstWord(string? normalized)
    {
        var text = SalesArabicText.Normalize(normalized);
        var space = text.IndexOf(' ');
        return space < 0 ? string.Empty : text[(space + 1)..];
    }
}
