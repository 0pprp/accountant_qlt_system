using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetAssignableLists;

public sealed record GetFollowerAssignableListsQuery(int BranchId) : IRequest<Result<List<FollowerAssignableListDto>>>;

public sealed record FollowerAssignableListDto(int ListId, string ListName, int FollowerId, string? FollowerName, string? DelegateName);

public sealed class GetFollowerAssignableListsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetFollowerAssignableListsQuery, Result<List<FollowerAssignableListDto>>>
{
    public async Task<Result<List<FollowerAssignableListDto>>> Handle(GetFollowerAssignableListsQuery request, CancellationToken ct)
    {
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.ManageFollowers) == false)
            return FollowerWorkErrors.ManageFollowersDenied;

        if (request.BranchId <= 0 || FollowerWorkAccess.CanAccessBranch(currentUser, request.BranchId) == false)
            return FollowerWorkErrors.BranchAccessDenied;

        return await db.OrderLists.AsNoTracking()
            .Where(l => l.BranchId == request.BranchId)
            .OrderBy(l => l.Name)
            .Select(l => new FollowerAssignableListDto(
                l.Id,
                l.Name,
                l.MotabaId,
                l.Motaba != null ? l.Motaba.FullName : null,
                l.Mandob != null ? l.Mandob.FullName : null))
            .ToListAsync(ct);
    }
}
