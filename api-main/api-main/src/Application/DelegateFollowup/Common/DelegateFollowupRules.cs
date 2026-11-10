namespace Application.DelegateFollowup.Common;

public static class DelegateFollowupRules
{
    public const int MaxDaysListActivity = 40;
    public const int CollectedHistoryDays = 30;
    public const int BoardOperatorScoreMin = 0;
    public const int BoardOperatorScoreMax = 10;
    public const int NewSaleUnpaidMinDays = 3;

    public const double RemainingBand100k = 100_000;
    public const double RemainingBand200k = 200_000;

    public const string LabelExcellent = "ممتاز";
    public const string LabelGood = "جيد";
    public const string LabelWeak = "ضعيف";
    public const string LabelLagging = "متلكئ";
    public const string LabelBad = "سيء";

    public const string StatusLegal = "قانونية";
    public const string StatusUnknown = "غير شيء";

    public const string TrendUp = "up";
    public const string TrendDown = "down";
    public const string TrendFlat = "flat";

    public const string UnresolvedNote = "غير محسوم";

    public readonly record struct RemainingBandCounts(int Lt100k, int From100To200k, int Ge100k);

    public readonly record struct ConversionCounts(int Improved, int Declined, int Unchanged);

    public static readonly IReadOnlyList<string> TierLabels =
    [
        LabelExcellent,
        LabelGood,
        LabelWeak,
        LabelLagging,
        LabelBad
    ];

    public static bool IsListActive(DateOnly? lastPaymentDate, DateOnly asOf)
    {
        if (lastPaymentDate is not DateOnly paid)
            return false;

        return paid >= asOf.AddDays(-MaxDaysListActivity);
    }

    public static bool IsRequiredDueEligible(double amountRemaining, bool isLegal) =>
        amountRemaining > 0 && isLegal == false;

    public static double SumRequiredDueInstallment(
        IEnumerable<(double AmountDaySales, double AmountRemaining, bool IsLegal)> rows)
    {
        double sum = 0;
        foreach (var row in rows)
        {
            if (IsRequiredDueEligible(row.AmountRemaining, row.IsLegal))
                sum += row.AmountDaySales;
        }

        return Math.Round(sum, 0, MidpointRounding.AwayFromZero);
    }

    public static DateOnly LastFridayBefore(DateOnly asOf)
    {
        var daysBack = ((int)asOf.DayOfWeek - (int)DayOfWeek.Friday + 7) % 7;
        if (daysBack == 0)
            daysBack = 7;
        return asOf.AddDays(-daysBack);
    }

    public static IReadOnlyList<DateOnly> LastNCalendarDaysEndingYesterday(DateOnly asOf, int days)
    {
        if (days <= 0)
            return [];

        var yesterday = asOf.AddDays(-1);
        var list = new List<DateOnly>(days);
        for (var i = 0; i < days; i++)
            list.Add(yesterday.AddDays(-i));
        return list;
    }

    public static IReadOnlyList<DateOnly> LastNFridays(DateOnly asOf, int count)
    {
        if (count <= 0)
            return [];

        var friday = LastFridayBefore(asOf);
        var list = new List<DateOnly>(count);
        for (var i = 0; i < count; i++)
            list.Add(friday.AddDays(-7 * i));
        return list;
    }

    public static int RatingTierRank(string? label) => label?.Trim() switch
    {
        LabelExcellent => 4,
        LabelGood => 3,
        LabelWeak => 2,
        LabelLagging => 1,
        LabelBad => 0,
        _ => -1
    };

    public static string NormalizeStatusLabel(string? label)
    {
        var s = (label ?? "").Trim();
        if (string.IsNullOrEmpty(s))
            return StatusUnknown;
        if (s is LabelExcellent or LabelGood or LabelWeak or LabelLagging or LabelBad or StatusLegal)
            return s;
        return StatusUnknown;
    }

    public static ConversionCounts CountConversions(
        IEnumerable<(string? PreviousLabel, string? CurrentLabel)> pairs)
    {
        var improved = 0;
        var declined = 0;
        var unchanged = 0;
        foreach (var (prev, cur) in pairs)
        {
            var a = RatingTierRank(prev);
            var b = RatingTierRank(cur);
            if (a < 0 || b < 0)
            {
                unchanged++;
                continue;
            }

            if (b > a)
                improved++;
            else if (b < a)
                declined++;
            else
                unchanged++;
        }

        return new ConversionCounts(improved, declined, unchanged);
    }

    public static Dictionary<string, List<(string From, int Count)>> BuildArrivalBreakdown(
        IEnumerable<(string? PreviousLabel, string? CurrentLabel)> pairs,
        IReadOnlyList<string> targets)
    {
        var result = targets.ToDictionary(
            t => t,
            _ => new Dictionary<string, int>(StringComparer.Ordinal));

        foreach (var (prevRaw, curRaw) in pairs)
        {
            var cur = NormalizeStatusLabel(curRaw);
            var prev = NormalizeStatusLabel(prevRaw);
            if (result.ContainsKey(cur) == false)
                continue;
            if (prev == cur)
                continue;
            var bag = result[cur];
            bag[prev] = bag.TryGetValue(prev, out var n) ? n + 1 : 1;
        }

        return result.ToDictionary(
            kv => kv.Key,
            kv => kv.Value
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => (From: x.Key, Count: x.Value))
                .ToList());
    }

    public static bool IsValidBoardOperatorScore(int score) =>
        score >= BoardOperatorScoreMin && score <= BoardOperatorScoreMax;

    public static bool IsRemainingLt100k(double amountRemaining) =>
        amountRemaining > 0 && amountRemaining < RemainingBand100k;

    public static bool IsRemaining100To200k(double amountRemaining) =>
        amountRemaining >= RemainingBand100k && amountRemaining < RemainingBand200k;

    public static bool IsRemainingGe100k(double amountRemaining) =>
        amountRemaining >= RemainingBand100k;

    public static RemainingBandCounts CountRemainingBands(IEnumerable<double> amountRemainings)
    {
        var lt100 = 0;
        var from100To200 = 0;
        var ge100 = 0;
        foreach (var amount in amountRemainings)
        {
            if (IsRemainingLt100k(amount))
                lt100++;
            if (IsRemaining100To200k(amount))
                from100To200++;
            if (IsRemainingGe100k(amount))
                ge100++;
        }

        return new RemainingBandCounts(lt100, from100To200, ge100);
    }

    public static string ClassifyAverageLabel(double averageScore)
    {
        if (averageScore >= 7)
            return LabelExcellent;
        if (averageScore >= 3)
            return LabelGood;
        if (averageScore >= 0)
            return LabelWeak;
        if (averageScore > -7.5)
            return LabelLagging;
        return LabelBad;
    }

    public static string ComputeScoreTrend(double scoreToday, double? scorePrior)
    {
        if (scorePrior is null)
            return TrendFlat;

        var delta = scoreToday - scorePrior.Value;
        if (Math.Abs(delta) < 0.01)
            return TrendFlat;

        return delta > 0 ? TrendUp : TrendDown;
    }

    public static DateOnly WeekAgoDate(DateOnly asOf) => asOf.AddDays(-7);

    public static int DaysSinceSaleInclusive(DateOnly? saleDate, DateOnly asOf) =>
        saleDate is null ? 0 : asOf.DayNumber - saleDate.Value.DayNumber + 1;

    public static bool IsNewSaleUnpaid3Days(
        double amountRemaining,
        double amountTotalSales,
        double receiptsTotal,
        int paymentCount,
        DateOnly? saleDate,
        DateOnly asOf,
        bool isLegal = false)
    {
        if (isLegal)
            return false;
        if (amountRemaining <= 0)
            return false;
        if (amountTotalSales <= 0)
            return false;
        if (receiptsTotal > 0 || paymentCount > 0)
            return false;
        return DaysSinceSaleInclusive(saleDate, asOf) >= NewSaleUnpaidMinDays;
    }

    /// <summary>اختَر تقييم اليوم إن وُجد، وإلا أحدث يوم في النافذة (مطابقة PickLatest بالمصدر).</summary>
    public static T? PickLatestDayRating<T>(
        IEnumerable<T> rows,
        Func<T, DateOnly> dateSelector,
        DateOnly preferDate)
        where T : class
    {
        T? preferred = null;
        T? latest = null;
        foreach (var row in rows)
        {
            var d = dateSelector(row);
            if (d == preferDate)
                preferred = row;
            if (latest is null || d > dateSelector(latest))
                latest = row;
        }

        return preferred ?? latest;
    }

    public static string? NormalizeNote(string? note)
    {
        var trimmed = note?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
