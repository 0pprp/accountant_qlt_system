namespace Application.Customers.Common;

/// <summary>
/// Customer rating label only (ممتاز/جيد/ضعيف/متلكئ/سيء) for Main Accountant UI.
/// Same day/rate bands as legacy phantom rating; no numeric score exposed.
/// </summary>
public static class CustomerRatingCalculator
{
    public const string Excellent = "ممتاز";
    public const string Good = "جيد";
    public const string Weak = "ضعيف";
    public const string Lagging = "متلكئ";
    public const string Bad = "سيء";

    private const int ExcellentMaxDays = 150;
    private const int GoodMaxDays = 160;
    private const int WeakMaxDaysExclusive = 180;
    private const int LaggingMaxDaysExclusive = 200;

    private const double ExcellentRate = 1000.0 / 150.0;
    private const double GoodRate = 6.25;
    private const double WeakRate = 1000.0 / 180.0;
    private const double LaggingMinRate = 1000.0 / 200.0;

    private static readonly DateOnly Floor = new(2026, 5, 1);
    private static readonly DateOnly BoostEnd = new(2026, 8, 31);

    public static DateOnly AdjustSaleDate(DateOnly d)
    {
        if (d < Floor) return Floor;
        if (d <= BoostEnd) return d.AddDays(30);
        return d;
    }

    public static int DiffDaysInclusive(DateOnly from, DateOnly to) =>
        to.DayNumber - from.DayNumber + 1;

    public static string ClassifySettledDays(int days)
    {
        if (days <= ExcellentMaxDays) return Excellent;
        if (days <= GoodMaxDays) return Good;
        if (days < WeakMaxDaysExclusive) return Weak;
        if (days < LaggingMaxDaysExclusive) return Lagging;
        return Bad;
    }

    public static string ClassifyActiveRate(double rate)
    {
        if (rate >= ExcellentRate) return Excellent;
        if (rate >= GoodRate) return Good;
        if (rate >= WeakRate) return Weak;
        if (rate >= LaggingMinRate) return Lagging;
        return Bad;
    }

    /// <summary>
    /// Returns Arabic label, or null when sale data is insufficient.
    /// </summary>
    public static string? EvaluateLabel(
        double amountTotalSales,
        double receiptsTotal,
        double amountRemaining,
        DateOnly? saleDate,
        DateOnly? lastPaymentDate,
        DateOnly asOf)
    {
        if (saleDate is null || amountTotalSales <= 0) return null;
        if (amountRemaining < 0) return Weak;

        var adjustedSale = AdjustSaleDate(saleDate.Value);

        if (amountRemaining == 0)
        {
            if (lastPaymentDate is null) return Weak;
            var days = DiffDaysInclusive(adjustedSale, lastPaymentDate.Value);
            if (days <= 0) return Weak;
            return ClassifySettledDays(days);
        }

        var daysSinceSale = DiffDaysInclusive(adjustedSale, asOf);
        if (daysSinceSale <= 0) return Weak;
        var rate = receiptsTotal * 1000.0 / (amountTotalSales * daysSinceSale);
        return ClassifyActiveRate(rate);
    }
}
