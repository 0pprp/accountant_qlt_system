namespace Application.SalesRatings.Common;

/// <summary>
/// مستمرين تطبيق المندوب: باقي &gt; 0 وليس قانوني وآخر تسديد خلال أقل من 365 يومًا.
/// </summary>
public static class DelegateContinuousCustomerRules
{
    public const int ContinuousWindowDays = 365;

    public static bool IsContinuous(
        bool isLegal,
        double amountRemaining,
        DateOnly? lastPaymentDate,
        DateOnly asOf)
    {
        if (isLegal)
            return false;
        if (amountRemaining <= 0)
            return false;
        if (lastPaymentDate is null)
            return false;

        var days = asOf.DayNumber - lastPaymentDate.Value.DayNumber;
        return days >= 0 && days < ContinuousWindowDays;
    }

    public static bool IsContinuous(RatingCustomerFact fact, DateOnly asOf) =>
        IsContinuous(fact.IsLegal, fact.AmountRemaining, fact.LastPaymentDate, asOf);
}
