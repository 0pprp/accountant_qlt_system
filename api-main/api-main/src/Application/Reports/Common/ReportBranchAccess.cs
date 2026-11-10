using Application.Common.Interfaces;

namespace Application.Reports.Common;

/// <summary>
/// تحقق نطاق الفرع لتقارير لوحة الإدارة.
/// وجود branchId في الطلب وحده لا يكفي — يجب أن يكون الفرع ضمن فروع المستخدم (أو دور Admin).
/// </summary>
public static class ReportBranchAccess
{
    public static Error? DenyIfOutOfScope(ICurrentUserService currentUser, int branchId)
    {
        if (currentUser.Role == Application.Common.SeedData.Roles.Admin.Name)
            return null;

        var userBranchIds = currentUser.BranchIds ?? [];
        if (userBranchIds.Contains(branchId) == false)
            return ReportErrors.BranchAccessDenied;

        return null;
    }
}
