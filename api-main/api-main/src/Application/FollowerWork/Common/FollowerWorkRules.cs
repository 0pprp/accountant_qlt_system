namespace Application.FollowerWork.Common;

public static class FollowerWorkRules
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 100;
    public const string TimeZoneId = "Asia/Baghdad";

    public static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "rating",
        "lists",
    };

    public static int ClampPage(int? page)
        => page is null or < 1 ? 1 : page.Value;

    public static int ClampPageSize(int? pageSize)
    {
        if (pageSize is null or < 1)
            return DefaultPageSize;
        return Math.Min(pageSize.Value, MaxPageSize);
    }

    public static bool TryParseSort(string? sort, out string normalized)
    {
        var s = string.IsNullOrWhiteSpace(sort) ? "name" : sort.Trim();
        if (AllowedSorts.Contains(s) == false)
        {
            normalized = "name";
            return false;
        }

        normalized = AllowedSorts.First(x => x.Equals(s, StringComparison.OrdinalIgnoreCase));
        return true;
    }

    public static string ComputeScoreTrend(double scoreToday, double? scorePrior)
    {
        if (scorePrior is null)
            return "flat";
        var delta = scoreToday - scorePrior.Value;
        if (Math.Abs(delta) < 0.01)
            return "flat";
        return delta > 0 ? "up" : "down";
    }

    public static string ComputeCountTrend(int countToday, int? countPrior)
    {
        if (countPrior is null)
            return "flat";
        var delta = countToday - countPrior.Value;
        if (delta == 0)
            return "flat";
        return delta > 0 ? "up" : "down";
    }

    public static DateOnly WeekAgo(DateOnly asOf) => asOf.AddDays(-7);
}
