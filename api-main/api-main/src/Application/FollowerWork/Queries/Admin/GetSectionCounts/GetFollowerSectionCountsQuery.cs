using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetSectionCounts;

public sealed record GetFollowerSectionCountsQuery(int FollowerId, int? BranchId) : IRequest<Result<FollowerSectionCountsDto>>;

public sealed record FollowerSectionCountsDto(
    int? PendingSales,
    int? Declining,
    int? Notes,
    int? OldCustomers,
    int? DelegateNotes,
    int? CollectionEnabledLists);

public sealed class GetFollowerSectionCountsHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    FollowerSalesReviewDataService scopeService,
    FollowerDecliningDataService declining,
    FollowerNotesDataService notes,
    FollowerOldCustomersDataService oldCustomers,
    FollowerDelegateNotesDataService delegateNotes)
    : IRequestHandler<GetFollowerSectionCountsQuery, Result<FollowerSectionCountsDto>>
{
    public async Task<Result<FollowerSectionCountsDto>> Handle(GetFollowerSectionCountsQuery request, CancellationToken ct)
    {
        if (FollowerWorkAccess.CanReadDirectory(currentUser) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        var branches = FollowerWorkAccess.ResolveBranchScope(currentUser, request.BranchId);
        var scope = await scopeService.ResolveFollowerScopeAsync(request.FollowerId, branches, request.BranchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;
        var listIds = scope.Data!.ListIds;

        int? pending = null;
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.SalesReviewRead))
        {
            var authenticated = db.FollowerSaleAuthentications.AsNoTracking().Select(a => a.OrderId);
            pending = await db.Orders.AsNoTracking()
                .CountAsync(o => o.OrderListId != null
                                 && listIds.Contains(o.OrderListId.Value)
                                 && o.ExecutionStatus == OrderExecutionStatus.NotStarted
                                 && authenticated.Contains(o.Id) == false, ct);
        }

        int? decliningCount = null;
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.DecliningRead))
        {
            var r = await declining.GetDecliningForAdminAsync(request.FollowerId, branches, request.BranchId, 1, 1, ct);
            decliningCount = r.IsSucceeded ? r.Data!.TotalCount : null;
        }

        int? notesCount = null;
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.NotesRead))
        {
            var r = await notes.GetNotesAsync(request.FollowerId, branches, request.BranchId, 1, 1, ct);
            notesCount = r.IsSucceeded ? r.Data!.TotalCount : null;
        }

        int? oldCount = null;
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.OldCustomersRead))
        {
            var r = await oldCustomers.GetForAdminAsync(request.FollowerId, branches, request.BranchId, 1, 1, ct);
            oldCount = r.IsSucceeded ? r.Data!.TotalCount : null;
        }

        int? delegateCount = null;
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.DelegateNotesRead))
        {
            var r = await delegateNotes.GetAsync(request.FollowerId, branches, request.BranchId, 1, 1, ct);
            delegateCount = r.IsSucceeded ? r.Data!.TotalCount : null;
        }

        int? collection = null;
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.CollectionRead))
        {
            collection = await db.OrderLists.AsNoTracking()
                .CountAsync(l => listIds.Contains(l.Id) && l.FollowerCollectionEnabled, ct);
        }

        return new FollowerSectionCountsDto(pending, decliningCount, notesCount, oldCount, delegateCount, collection);
    }
}
