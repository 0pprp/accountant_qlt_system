namespace Application.SalesRatings.Common;

/// <summary>
/// مجموعة التقييم النشط للقائمة/المتابع/الفرع: رصيد مفتوح + آخر تسديد خلال 30 يومًا.
/// مستقل عن نشاط القائمة 40ي في كتالوج DelegateFollowup.
/// </summary>
public static class ActiveCustomerRules
{
    public const int MaxDaysWithoutPayment = 30;

    public static bool IsActive(DateOnly? lastPaymentDate, DateOnly asOf)
    {
        if (lastPaymentDate is not DateOnly paid)
            return false;

        return paid >= asOf.AddDays(-MaxDaysWithoutPayment);
    }
}
