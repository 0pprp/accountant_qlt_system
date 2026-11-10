namespace Application.ManagementSummary.Common;

public static class ManagementSummaryErrors
{
    public static readonly Error AccessDenied = new(
        "ليس لديك صلاحية قراءة الملخص الإداري.",
        "ManagementSummary.AccessDenied");

    public static readonly Error DrilldownDenied = new(
        "ليس لديك صلاحية فتح تفاصيل خلية الملخص.",
        "ManagementSummary.DrilldownDenied");

    public static readonly Error BranchAccessDenied = new(
        "الفرع المطلوب خارج نطاق صلاحياتك.",
        "ManagementSummary.BranchAccessDenied");

    public static readonly Error InvalidMetric = new(
        "المقياس غير معروف أو غير مسموح.",
        "ManagementSummary.InvalidMetric");

    public static readonly Error InvalidPeriod = new(
        "السنة أو الشهر غير صالح.",
        "ManagementSummary.InvalidPeriod");

    public static readonly Error InvalidDay = new(
        "اليوم غير صالح ضمن الشهر المحدد.",
        "ManagementSummary.InvalidDay");

    public static readonly Error BranchRequired = new(
        "معرّف الفرع مطلوب لتفاصيل الخلية.",
        "ManagementSummary.BranchRequired");
}
