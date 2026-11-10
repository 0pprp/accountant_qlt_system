namespace Application.SalesFollowup.Common;

/// <summary>
/// قواعد لوحة متابعة المبيعات — مقياس النظام الحالي 10/5/0/-5/-10.
/// Deploy marker: SalesFollowup_V1_BranchScoped_BatchedDetail
/// </summary>
public static class SalesFollowupRules
{
    public const string DeployMarker = "SalesFollowup_V1_BranchScoped_BatchedDetail";
    public const int HighMinScore = 5;
    public const int LowMaxScore = 0;
    public const int DefaultHistoryLimit = 30;
    public const int SystemAverageHistoryDays = 30;

    public static bool IsHigh(int score) => score >= HighMinScore;
    public static bool IsLow(int score) => score <= LowMaxScore;

    public static SalesFollowupRatingRowDto Anonymize(SalesFollowupRatingRowDto row) =>
        new()
        {
            Score = row.Score,
            Note = row.Note,
            RatingDate = row.RatingDate,
            Kind = row.Kind,
        };

    public static IReadOnlyList<SalesFollowupRatingRowDto> AnonymizeLowOnly(
        IEnumerable<SalesFollowupRatingRowDto> rows,
        int limit = DefaultHistoryLimit) =>
        rows
            .Where(r => IsLow(r.Score))
            .OrderByDescending(r => r.RatingDate)
            .ThenByDescending(r => r.Score)
            .Take(limit)
            .Select(Anonymize)
            .ToList();

    /// <summary>
    /// مصفّر: غير قانوني + بيع فعلي + الواصل يغطي السعر + الباقي ≤ 0.
    /// IsLegal غير موجود في المخطط → يُعامل كـ false حتى يُضاف العمود.
    /// </summary>
    public static bool IsZeroedCustomer(SalesFollowupCustomerFact f) =>
        !f.IsLegal
        && f.AmountTotalSales > 0
        && f.ReceiptsTotal >= f.AmountTotalSales
        && f.AmountRemaining <= 0;

    public static int CountZeroedCustomers(IEnumerable<SalesFollowupCustomerFact> facts) =>
        facts.Count(IsZeroedCustomer);

    public static string AverageRatingLabel(double averageScore)
    {
        if (averageScore >= 7.5) return "ممتاز";
        if (averageScore >= 2.5) return "جيد";
        if (averageScore >= -2.5) return "ضعيف";
        if (averageScore >= -7.5) return "متلكئ";
        return "سيء";
    }
}
