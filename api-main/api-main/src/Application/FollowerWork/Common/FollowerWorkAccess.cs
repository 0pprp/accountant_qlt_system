using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Common;

/// <summary>
/// دليل المتابعين: FollowerWork.Read أو أي صلاحية قسم منفّذ يحتاج الدليل.
/// صلاحية قسم غير منفّذ لا تعلن القسم جاهزًا؛ تُستخدم فقط لفتح الحد الأدنى من الهوية عند التنفيذ.
/// </summary>
public static class FollowerWorkAccess
{
    /// <summary>صلاحيات أقسام تمنح جلب الهوية عند تنفيذ دفعتها (مسجّلة الآن).</summary>
    public static readonly string[] SectionDirectoryPermissions =
    [
        SeedPermissions.FollowerWork.SalesReviewRead,
        SeedPermissions.FollowerWork.DecliningRead,
        SeedPermissions.FollowerWork.NotesRead,
        SeedPermissions.FollowerWork.OldCustomersRead,
        SeedPermissions.FollowerWork.DelegateNotesRead,
        SeedPermissions.FollowerWork.CollectionRead,
        SeedPermissions.FollowerWork.LocationRead,
        SeedPermissions.FollowerWork.RouteRead,
    ];

    public static bool CanReadDirectory(ICurrentUserService user)
    {
        if (user.HasPermission(SeedPermissions.FollowerWork.Read))
            return true;

        foreach (var p in SectionDirectoryPermissions)
        {
            if (user.HasPermission(p))
                return true;
        }

        return false;
    }

    /// <summary>مؤشرات التقييم على البطاقة — فقط مع دليل FollowerWork.Read (ليست صلاحية قسم).</summary>
    public static bool CanReadCardMetrics(ICurrentUserService user)
        => user.HasPermission(SeedPermissions.FollowerWork.Read);

    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static Error? ValidateBranchOrAll(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
        {
            if (AuthorizedBranches(user).Count == 0)
                return FollowerWorkErrors.BranchAccessDenied;
            return null;
        }

        if (CanAccessBranch(user, branchId.Value) == false)
            return FollowerWorkErrors.BranchAccessDenied;
        return null;
    }

    public static IReadOnlyList<int> ResolveBranchScope(ICurrentUserService user, int? branchId)
    {
        var authorized = AuthorizedBranches(user);
        if (branchId is null or <= 0)
            return authorized;
        return authorized.Contains(branchId.Value) ? [branchId.Value] : [];
    }
}
