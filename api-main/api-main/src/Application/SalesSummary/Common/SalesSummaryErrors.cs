namespace Application.SalesSummary.Common;

public static class SalesSummaryErrors
{
    public static readonly Error InvalidPeriod = new(
        "الشهر أو السنة غير صالحين.",
        "SalesSummary.InvalidPeriod");

    public static readonly Error InvalidDay = new(
        "اليوم خارج الشهر المحدد.",
        "SalesSummary.InvalidDay");

    public static readonly Error MetricRejected = new(
        "ملخص المبيعات يعرض سعر البيع فقط.",
        "SalesSummary.MetricRejected");

    public static readonly Error BranchForbidden = new(
        "لا صلاحية على الفرع المطلوب.",
        "SalesSummary.BranchForbidden");

    public static readonly Error ReadDenied = new(
        "غير مخول بقراءة ملخص المبيعات.",
        "SalesSummary.ReadDenied");

    public static readonly Error DrilldownDenied = new(
        "غير مخول بتفاصيل خلية ملخص المبيعات.",
        "SalesSummary.DrilldownDenied");
}
