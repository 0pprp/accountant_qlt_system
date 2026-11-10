using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementTasks.Queries.App.Inbox;

public class GetAppManagementTasksInboxHandler
    : IRequestHandler<GetAppManagementTasksInboxQuery, Result<PaginatedList<ManagementTaskListItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetAppManagementTasksInboxHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PaginatedList<ManagementTaskListItemDto>>> Handle(
        GetAppManagementTasksInboxQuery request,
        CancellationToken cancellationToken)
    {
        if (ManagementTaskActionHelper.CanUseAnyActionPermission(_currentUser) == false)
            return ManagementTaskErrors.ActionForbidden;

        var userId = _currentUser.UserId ?? 0;
        if (userId <= 0)
            return ManagementTaskErrors.ActionForbidden;

        var branches = ManagementTaskAccess.AuthorizedBranches(_currentUser);
        if (branches.Count == 0)
            return new PaginatedList<ManagementTaskListItemDto>(
                [], 0, request.Pagination.PageIndex, request.Pagination.PageSize);

        var query = _db.ManagementTasks
            .AsNoTracking()
            .Where(x => branches.Contains(x.BranchId)
                        && (x.CurrentHolderUserId == userId
                            || (x.AssigneeUserId == userId
                                && (x.Status == ManagementTaskStatus.Returned
                                    || x.Status == ManagementTaskStatus.AwaitingDelegatedConfirm))));

        if (request.Status is not null)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }
        else
        {
            query = query.Where(x => ManagementTaskInboxRules.IsDefaultOpenStatus(x.Status));
        }

        return await ManagementTaskListProjection.ToPageAsync(
            query,
            request.Pagination,
            _clock.UtcNowOffset,
            cancellationToken);
    }
}
