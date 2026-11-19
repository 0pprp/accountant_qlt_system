using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Archive.Common;

/// <summary>
/// المرحلة ليست قابلة للاشتقاق من الدور وحده في هذا النظام: موظف المبيعات ليس دوراً
/// بل <c>Mandob</c> مع تكليف ساري، وإدارة الفرع والمتابع قد يحملهما أكثر من مستخدم
/// في الفرع الواحد. فالعميل يصرّح بالمرحلة، والخادم يتحقّق من صلاحيتها ومن النطاق.
///
/// القاعدة: <b>لا ترسل من مرحلة إلا إن كنت تستطيع الاستلام فيها</b> — إلا مرحلة
/// المصدر، فشرطها ملكية الحزمة.
/// </summary>
public static class ArchiveAccess
{
    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static IReadOnlyList<int> ResolveBranchScope(ICurrentUserService user, int? branchId)
    {
        var authorized = AuthorizedBranches(user);
        if (branchId is null or <= 0)
            return authorized;
        return authorized.Contains(branchId.Value) ? [branchId.Value] : [];
    }

    public static string? ReceivePermission(ArchiveStage stage) => stage switch
    {
        ArchiveStage.BranchManagement => SeedPermissions.Archive.ReceiveBranch,
        ArchiveStage.Follower => SeedPermissions.Archive.ReceiveFollower,
        ArchiveStage.Archive => SeedPermissions.Archive.ReceiveArchive,
        _ => null
    };

    public static bool CanReceiveAt(ICurrentUserService user, ArchiveStage stage)
    {
        var permission = ReceivePermission(stage);
        return permission is not null && user.HasPermission(permission);
    }

    public static Error? EnsureCanSend(ICurrentUserService user, ArchiveStage stage, ArchiveBundle bundle)
    {
        if (user.HasPermission(SeedPermissions.Archive.Send) == false)
            return ArchiveErrors.ActionForbidden;

        if (CanAccessBranch(user, bundle.BranchId) == false)
            return ArchiveErrors.BranchForbidden;

        if (stage == ArchiveStage.SalesEmployee)
        {
            return bundle.SalesEmployeeUserId == (user.UserId ?? 0)
                ? null
                : ArchiveErrors.NotBundleOwner;
        }

        return CanReceiveAt(user, stage) ? null : ArchiveErrors.StageForbidden;
    }

    public static Error? EnsureCanReceive(ICurrentUserService user, ArchiveStage stage, int branchId)
    {
        if (CanReceiveAt(user, stage) == false)
            return ArchiveErrors.StageForbidden;

        return CanAccessBranch(user, branchId) ? null : ArchiveErrors.BranchForbidden;
    }

    public static bool HasOversight(ICurrentUserService user)
        => user.HasPermission(SeedPermissions.Archive.Oversight);

    /// <summary>
    /// شريحة العرض الافتراضية: من يملك الإشراف يرى كل حزم فروعه،
    /// ومن لا يملكه يرى حزمه هو ثم ما وصل مرحلته.
    /// </summary>
    public static ArchiveStage? DefaultStageFor(ICurrentUserService user)
    {
        if (CanReceiveAt(user, ArchiveStage.Archive)) return ArchiveStage.Archive;
        if (CanReceiveAt(user, ArchiveStage.Follower)) return ArchiveStage.Follower;
        if (CanReceiveAt(user, ArchiveStage.BranchManagement)) return ArchiveStage.BranchManagement;
        if (user.HasPermission(SeedPermissions.Archive.Send)) return ArchiveStage.SalesEmployee;
        return null;
    }
}
