namespace Application.SalesRecords.Common;

public static class SalesRecordsErrors
{
    public static readonly Error BranchForbidden = new(
        "لا صلاحية على الفرع المطلوب.",
        "SalesRecords.BranchForbidden");

    public static readonly Error NoBranchScope = new(
        "لا فروع مصرّح بها لهذا الحساب.",
        "SalesRecords.NoBranchScope");

    public static readonly Error InvalidSort = new(
        "حقل الترتيب غير مسموح.",
        "SalesRecords.InvalidSort");

    public static readonly Error NotFound = new(
        "البيع غير موجود ضمن النطاق.",
        "SalesRecords.NotFound");
}
