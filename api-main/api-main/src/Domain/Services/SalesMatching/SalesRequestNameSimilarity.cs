namespace Domain.Services.SalesMatching;

/// <summary>
/// تطبيع ومطابقة الأسماء للمقارنة فقط — لا يغيّر الاسم المخزّن.
/// </summary>
public static class SalesRequestNameSimilarity
{
    private static readonly HashSet<char> Diacritics =
    [
        '\u064B', '\u064C', '\u064D', '\u064E', '\u064F', '\u0650', '\u0651', '\u0652',
        '\u0670', '\u0653', '\u0654', '\u0655'
    ];

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = new List<char>(value.Length);
        foreach (var c in value.Trim())
        {
            if (Diacritics.Contains(c) || c == 'ـ')
                continue;
            if (char.IsPunctuation(c) || char.IsSymbol(c))
                continue;

            chars.Add(c switch
            {
                'أ' or 'إ' or 'آ' or 'ٱ' => 'ا',
                'ى' => 'ي',
                'ة' => 'ه',
                _ => c
            });
        }

        var text = new string(chars.ToArray());
        var parts = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }

    public static IReadOnlyList<string> MatchingTokens(string? value)
    {
        var normalized = Normalize(value);
        if (normalized.Length == 0)
            return [];

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (parts.Count > 0 && parts[^1].All(char.IsDigit))
            parts.RemoveAt(parts.Count - 1);

        return parts;
    }

    public static (string First, string Father, string Grandfather) TripleParts(string? value)
    {
        var tokens = MatchingTokens(value);
        return (
            tokens.ElementAtOrDefault(0) ?? string.Empty,
            tokens.ElementAtOrDefault(1) ?? string.Empty,
            tokens.ElementAtOrDefault(2) ?? string.Empty);
    }

    public static bool HasEnoughTripleName(string? value) => MatchingTokens(value).Count >= 3;

    public static bool HasEnoughFatherGrandfather(string? value)
    {
        var (_, father, grandfather) = TripleParts(value);
        return father.Length > 0 && grandfather.Length > 0;
    }

    public static bool IsTripleNameMatch(string? queryName, string? candidateName)
    {
        var q = MatchingTokens(queryName);
        var c = MatchingTokens(candidateName);
        if (q.Count < 3 || c.Count < 3)
            return false;

        return string.Equals(q[0], c[0], StringComparison.Ordinal)
               && string.Equals(q[1], c[1], StringComparison.Ordinal)
               && string.Equals(q[2], c[2], StringComparison.Ordinal);
    }

    public static bool IsFatherOrGrandfatherMatch(string? queryName, string? candidateName)
    {
        var (_, father, grandfather) = TripleParts(queryName);
        if (father.Length == 0 || grandfather.Length == 0)
            return false;

        var c = MatchingTokens(candidateName);
        if (c.Count < 3)
            return false;

        return string.Equals(father, c[1], StringComparison.Ordinal)
               && string.Equals(grandfather, c[2], StringComparison.Ordinal);
    }

    public static bool IsSonFatherMatch(string? queryName, string? candidateName)
    {
        var q = MatchingTokens(queryName);
        var c = MatchingTokens(candidateName);
        return IsSonFatherTokenMatch(q, c) || IsSonFatherTokenMatch(c, q);
    }

    private static bool IsSonFatherTokenMatch(IReadOnlyList<string> longer, IReadOnlyList<string> shorter)
    {
        if (longer.Count < 3 || shorter.Count < 2)
            return false;
        if (longer[1].Length == 0 || longer[2].Length == 0)
            return false;
        if (shorter[0].Length == 0 || shorter[1].Length == 0)
            return false;

        return string.Equals(longer[1], shorter[0], StringComparison.Ordinal)
               && string.Equals(longer[2], shorter[1], StringComparison.Ordinal);
    }
}
