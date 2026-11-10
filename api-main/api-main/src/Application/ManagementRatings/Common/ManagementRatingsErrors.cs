namespace Application.ManagementRatings.Common;

public static class ManagementRatingsErrors
{
    public static readonly Error AccessDenied = new(
        "ليس لديك صلاحية قراءة التقييمات.",
        "ManagementRatings.AccessDenied");

    public static readonly Error DetailsDenied = new(
        "ليس لديك صلاحية تفاصيل زبائن التقييم.",
        "ManagementRatings.DetailsDenied");

    public static readonly Error BranchAccessDenied = new(
        "الفرع المطلوب خارج نطاق صلاحياتك.",
        "ManagementRatings.BranchAccessDenied");

    public static readonly Error BranchRequired = new(
        "معرّف الفرع مطلوب.",
        "ManagementRatings.BranchRequired");

    public static readonly Error InvalidScope = new(
        "نطاق التقييم غير مسموح.",
        "ManagementRatings.InvalidScope");

    public static readonly Error InvalidResource = new(
        "المورد غير موجود أو خارج الفرع.",
        "ManagementRatings.InvalidResource");

    public static readonly Error InvalidLevel = new(
        "مستوى التقييم غير معروف.",
        "ManagementRatings.InvalidLevel");

    public static readonly Error InvalidPeriod = new(
        "فترة التاريخ غير صالحة.",
        "ManagementRatings.InvalidPeriod");
}
