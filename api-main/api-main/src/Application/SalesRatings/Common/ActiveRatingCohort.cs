namespace Application.SalesRatings.Common;

/// <summary>
/// مجموعة العنوان + قوائم الفئات — يجب أن تطابق عدد الفئة تفاصيل الزبائن.
/// </summary>
public static class ActiveRatingCohort
{
    /// <param name="requireRecentPayment">
    /// true = رصيد مفتوح + آخر تسديد خلال 30 يومًا (لوحة الإدارة).
    /// false = رصيد مفتوح فقط (تطبيقات المندوب/المتابع/المبيعات).
    /// </param>
    public static IReadOnlyList<(RatingCustomerFact Fact, int Score, string Level)> Build(
        IReadOnlyList<RatingCustomerFact> openFacts,
        DateOnly asOf,
        bool usePhantomSaleDate = false,
        bool requireRecentPayment = true)
    {
        var matched = new List<(RatingCustomerFact, int, string)>();
        foreach (var fact in openFacts)
        {
            if (fact.HasOpenBalance == false)
                continue;
            if (requireRecentPayment
                && ActiveCustomerRules.IsActive(fact.LastPaymentDate, asOf) == false)
                continue;

            var score = CustomerRatingCalculator.ScoreFor(
                fact.AmountTotalSales,
                fact.ReceiptsTotal,
                fact.AmountRemaining,
                fact.SaleDate,
                fact.LastPaymentDate,
                asOf,
                usePhantomSaleDate);
            if (score is null)
                continue;

            matched.Add((fact, score.Value, CustomerRatingCalculator.LevelName(score.Value)));
        }

        return matched;
    }

    public static RatingAggregateResult Aggregate(
        IReadOnlyList<RatingCustomerFact> openFacts,
        DateOnly asOf,
        bool usePhantomSaleDate = false,
        bool requireRecentPayment = true)
    {
        var built = Build(openFacts, asOf, usePhantomSaleDate, requireRecentPayment);
        return RatingAggregator.Aggregate(built.Select(x => x.Score));
    }
}
