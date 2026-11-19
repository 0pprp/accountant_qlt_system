using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Common;

/// <summary>
/// نطاق الفرع يأتي من claims الجلسة لا من الـpayload. الوصول للخزينة مسموح فقط
/// إن كان فرعها داخل نطاق المستخدم.
/// </summary>
public static class SafeAccess
{
    public static Result<IReadOnlyList<int>> ResolveScope(ICurrentUserService currentUser)
    {
        var branchIds = currentUser.BranchIds;
        if (branchIds is null || branchIds.Count == 0)
            return SafeErrors.SafeAccessDenied;

        return branchIds.Distinct().ToList();
    }

    public static Result EnsureBranchInScope(ICurrentUserService currentUser, int branchId)
    {
        var scope = ResolveScope(currentUser);
        if (scope.IsFailed)
            return scope.Error;

        if (scope.Data!.Contains(branchId) == false)
            return SafeErrors.SafeAccessDenied;

        return Result.Success();
    }

    /// <summary>يتحقق من وجود الخزينة ومن أن فرعها داخل نطاق المستخدم، ويُرجع معرّف الفرع.</summary>
    public static async Task<Result<int>> EnsureSafeInScopeAsync(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser,
        int safeId,
        CancellationToken cancellationToken)
    {
        var branchId = await dbContext.Safes.AsNoTracking()
            .Where(x => x.Id == safeId)
            .Select(x => (int?)x.BranchId)
            .FirstOrDefaultAsync(cancellationToken);

        if (branchId is null)
            return SafeErrors.SafeNotFound;

        var scoped = EnsureBranchInScope(currentUser, branchId.Value);
        if (scoped.IsFailed)
            return scoped.Error;

        return branchId.Value;
    }
}
