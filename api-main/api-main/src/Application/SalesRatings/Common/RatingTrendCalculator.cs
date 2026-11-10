namespace Application.SalesRatings.Common;

/// <summary>
/// مطابق ListRatingTrendCalculator: مجموعة البيع في [saleFrom,saleTo] مقيَّمة لكل يوم حتى asOf.
/// </summary>
public static class RatingTrendCalculator
{
    public const int MaxTrendDays = 366;

    public static IEnumerable<RatingCustomerFact> FactsSoldInPeriod(
        IEnumerable<RatingCustomerFact> facts,
        DateOnly saleFrom,
        DateOnly saleTo,
        DateOnly asOf)
    {
        if (asOf < saleFrom)
            return [];

        var end = asOf <= saleTo ? asOf : saleTo;
        return facts.Where(f =>
            f.SaleDate is DateOnly sale && sale >= saleFrom && sale <= end);
    }

    public static RatingAggregateResult PeriodAggregate(
        IEnumerable<RatingCustomerFact> facts,
        DateOnly saleFrom,
        DateOnly saleTo,
        DateOnly asOf,
        bool usePhantomSaleDate = false)
    {
        var inPeriod = FactsSoldInPeriod(facts, saleFrom, saleTo, asOf).ToList();
        var scores = new List<int>(inPeriod.Count);
        foreach (var fact in inPeriod)
        {
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
            if (fact.HasOpenBalance == false)
                continue;
            scores.Add(score.Value);
        }

        return RatingAggregator.Aggregate(scores);
    }

    public static IReadOnlyList<(DateOnly Date, double Value)> DailyScores(
        IReadOnlyList<RatingCustomerFact> facts,
        DateOnly saleFrom,
        DateOnly saleTo,
        DateOnly chartFrom,
        DateOnly chartTo)
    {
        if (chartTo < saleFrom)
            chartTo = saleFrom;
        if (chartFrom < saleFrom)
            chartFrom = saleFrom;
        if (chartFrom > chartTo)
            chartFrom = chartTo;

        var points = new List<(DateOnly, double)>();
        for (var d = chartFrom; d <= chartTo; d = d.AddDays(1))
        {
            var agg = PeriodAggregate(facts, saleFrom, saleTo, d);
            points.Add((d, Math.Round(agg.AverageScore, 2, MidpointRounding.AwayFromZero)));
        }

        return points;
    }

    public static (DateOnly From, DateOnly To) ClampSaleWindow(DateOnly from, DateOnly to)
    {
        if (to < from)
            (from, to) = (to, from);
        if ((to.DayNumber - from.DayNumber) > MaxTrendDays - 1)
            from = to.AddDays(-(MaxTrendDays - 1));
        return (from, to);
    }
}
