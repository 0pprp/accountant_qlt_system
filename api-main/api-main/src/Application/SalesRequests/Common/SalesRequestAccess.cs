using Application.Common.Interfaces;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Common;

public static class SalesRequestAccess
{
    public static IQueryable<SalesRequest> UnassignedInScope(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branchIds)
    {
        return db.SalesRequests.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Where(x => branchIds.Contains(x.BranchId))
            .Where(x => x.Status != SalesRequestStatus.Rejected
                        && x.Status != SalesRequestStatus.Completed
                        && x.Status != SalesRequestStatus.LegalFollowup)
            .Where(x => x.ExceptionHoldStatus == null
                        || (x.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Approved
                            && x.ExceptionConsumedAtUtc != null)
                        || (x.TargetEmployeeId != null && x.TargetEmployeeId > 0))
            .Where(x => x.Status == SalesRequestStatus.New
                        || x.TargetEmployeeId == null
                        || x.TargetEmployeeId <= 0);
    }

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => user.BranchIds is { Count: > 0 } && user.BranchIds.Contains(branchId);

    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => user.BranchIds ?? [];
}
