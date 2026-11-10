using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ManagementTasks.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Queries.Admin.List;

public class GetManagementTasksHandler
    : IRequestHandler<GetManagementTasksQuery, Result<PaginatedList<ManagementTaskListItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetManagementTasksHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PaginatedList<ManagementTaskListItemDto>>> Handle(
        GetManagementTasksQuery request,
        CancellationToken cancellationToken)
    {
        var hasRead = _currentUser.HasPermission(SeedPermissions.ManagementTask.Read);
        if (hasRead == false && request.InboxForMe == false)
            return ManagementTaskErrors.ActionForbidden;

        var userId = _currentUser.UserId ?? 0;
        if (userId <= 0)
            return ManagementTaskErrors.ActionForbidden;

        if (request.BranchId is > 0 && ManagementTaskAccess.CanAccessBranch(_currentUser, request.BranchId.Value) == false)
            return ManagementTaskErrors.BranchForbidden;

        var branches = ManagementTaskAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (branches.Count == 0)
            return new PaginatedList<ManagementTaskListItemDto>(
                [], 0, request.Pagination.PageIndex, request.Pagination.PageSize);

        var query = _db.ManagementTasks
            .AsNoTracking()
            .Where(x => branches.Contains(x.BranchId));

        if (request.InboxForMe)
        {
            query = query.Where(x =>
                x.CurrentHolderUserId == userId
                || (x.AssigneeUserId == userId
                    && (x.Status == Domain.Entities.ManagementTaskAggregate.Enums.ManagementTaskStatus.Returned
                        || x.Status == Domain.Entities.ManagementTaskAggregate.Enums.ManagementTaskStatus.AwaitingDelegatedConfirm)));
        }

        if (request.Status is not null)
            query = query.Where(x => x.Status == request.Status.Value);
        else if (request.InboxForMe)
            query = query.Where(x => x.Status != Domain.Entities.ManagementTaskAggregate.Enums.ManagementTaskStatus.Completed);

        query = ManagementTaskListProjection.ApplySearch(query, request.Search);

        return await ManagementTaskListProjection.ToPageAsync(
            query,
            request.Pagination,
            _clock.UtcNowOffset,
            cancellationToken);
    }
}
