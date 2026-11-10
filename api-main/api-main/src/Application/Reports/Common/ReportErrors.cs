namespace Application.Reports.Common;

public static class ReportErrors
{
    public static Error BranchAccessDenied =
        new("ليس لديك صلاحية الوصول إلى بيانات هذا الفرع", "Report_Branch_Access_Denied");
}
