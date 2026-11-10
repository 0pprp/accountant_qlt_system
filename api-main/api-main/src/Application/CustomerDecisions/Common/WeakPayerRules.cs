namespace Application.CustomerDecisions.Common;

/// <summary>قواعد طابور الأسماء القانونية من Customers_GetWeakWeekPayers.</summary>
public static class WeakPayerRules
{
    public const double MaxWeekPaidPercent = 3.8;
    public const int MinDaysSinceLastPayment = 40;

    public static bool Matches(
        double saleTotal,
        double weekPaid,
        double remaining,
        DateOnly? lastPaymentDate,
        DateOnly today,
        bool isLegal,
        bool isFakeSale,
        bool hasActiveContinueSnooze,
        bool hasPendingLegalConversion,
        bool hasOpenWaiver)
    {
        if (isLegal || isFakeSale || hasActiveContinueSnooze || hasPendingLegalConversion || hasOpenWaiver)
            return false;
        if (remaining <= 0 || saleTotal <= 0)
            return false;
        var pct = saleTotal <= 0 ? 0 : weekPaid * 100.0 / saleTotal;
        if (pct > MaxWeekPaidPercent)
            return false;
        if (lastPaymentDate is null)
            return true;
        var days = today.DayNumber - lastPaymentDate.Value.DayNumber;
        return days >= MinDaysSinceLastPayment;
    }
}
