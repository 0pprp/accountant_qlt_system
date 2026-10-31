namespace Application.SalesRatings.Common;

public static class CustomerRatingCalculator
{
    public const int ExcellentMaxDays = 150;
    public const int GoodMaxDays = 160;
    public const int WeakMaxDaysExclusive = 180;
    public const int LaggingMaxDaysExclusive = 200;

    public const double ExcellentRate = 1000.0 / 150.0;
    public const double GoodRate = 6.25;
    public const double WeakRate = 1000.0 / 180.0;
    public const double LaggingMinRate = 1000.0 / 200.0;

    private static readonly DateOnly Floor = new(2026, 5, 1);
    private static readonly DateOnly BoostEnd = new(2026, 8, 31);

    public static DateOnly AdjustSaleDate(DateOnly d)
    {
        if (d < Floor)
            return Floor;
        if (d <= BoostEnd)
            return d.AddDays(30);
        return d;
    }

    public static int DiffDaysInclusive(DateOnly from, DateOnly to) => to.DayNumber - from.DayNumber + 1;

    public static int ClassifySettledDays(int days)
    {
        if (days <= ExcellentMaxDays)
            return 10;
        if (days <= GoodMaxDays)
            return 5;
        if (days < WeakMaxDaysExclusive)
            return 0;
        if (days < LaggingMaxDaysExclusive)
            return -5;
        return -10;
    }

    public static int ClassifyActiveRate(double rate)
    {
        if (rate >= ExcellentRate)
            return 10;
        if (rate >= GoodRate)
            return 5;
        if (rate >= WeakRate)
            return 0;
        if (rate >= LaggingMinRate)
            return -5;
        return -10;
    }

    public static int? ScoreFor(
        double amountTotalSales,
        double receiptsTotal,
        double amountRemaining,
        DateOnly? dateSaleDevice,
        DateOnly? lastPaymentDate,
        DateOnly asOf)
    {
        if (dateSaleDevice is null || amountTotalSales <= 0)
            return null;
        if (amountRemaining < 0)
            return 0;

        var saleDate = AdjustSaleDate(dateSaleDevice.Value);

        if (amountRemaining == 0)
        {
            if (lastPaymentDate is null)
                return 0;
            var days = DiffDaysInclusive(saleDate, lastPaymentDate.Value);
            if (days <= 0)
                return 0;
            return ClassifySettledDays(days);
        }

        var daysSinceSale = DiffDaysInclusive(saleDate, asOf);
        if (daysSinceSale <= 0)
            return 0;

        var rate = receiptsTotal * 1000.0 / (amountTotalSales * daysSinceSale);
        return ClassifyActiveRate(rate);
    }

    public static string LevelName(int score) => score switch
    {
        10 => "ممتاز",
        5 => "جيد",
        0 => "ضعيف",
        -5 => "متلكئ",
        _ => "سيء"
    };

    public static bool IsCardLevel(string level) =>
        level is "ضعيف" or "متلكئ" or "سيء" or "weak" or "lagging" or "bad";

    public static string NormalizeCardLevel(string level) => level.Trim().ToLowerInvariant() switch
    {
        "ضعيف" or "weak" => "ضعيف",
        "متلكئ" or "lagging" => "متلكئ",
        "سيء" or "bad" => "سيء",
        _ => level
    };
}
