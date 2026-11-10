namespace Application.SalesOverview.Common;

public static class SalesOverviewErrors
{
    public static readonly Error BranchForbidden = new(
        "لا صلاحية على الفرع المطلوب.",
        "SalesOverview.BranchForbidden");

    public static readonly Error NoBranchScope = new(
        "لا فروع مصرّح بها لهذا الحساب.",
        "SalesOverview.NoBranchScope");
}
