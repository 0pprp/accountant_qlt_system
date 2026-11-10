namespace Application.SalesEmployees.Common;

public static class SalesEmployeeErrors
{
    public static readonly Error BranchForbidden = new(
        "لا صلاحية على الفرع المطلوب.",
        "SalesEmployee.BranchForbidden");

    public static readonly Error NoBranchScope = new(
        "لا فروع مصرّح بها لهذا الحساب.",
        "SalesEmployee.NoBranchScope");

    public static readonly Error NotFound = new(
        "موظف المبيعات غير موجود ضمن النطاق.",
        "SalesEmployee.NotFound");

    public static readonly Error InvalidShiftState = new(
        "حالة الدوام غير معروفة. المسموح: Active أو NoShift.",
        "SalesEmployee.InvalidShiftState");

    public static readonly Error InvalidSort = new(
        "حقل الترتيب غير مسموح.",
        "SalesEmployee.InvalidSort");

    public static readonly Error InvalidBucket = new(
        "فئة التقييم غير معروفة.",
        "SalesEmployee.InvalidBucket");
}
