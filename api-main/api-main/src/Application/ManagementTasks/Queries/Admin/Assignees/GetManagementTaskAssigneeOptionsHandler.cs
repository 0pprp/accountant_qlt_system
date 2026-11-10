using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.SalesRequests.Common;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Queries.Admin.Assignees;

public class GetManagementTaskAssigneeOptionsHandler
    : IRequestHandler<GetManagementTaskAssigneeOptionsQuery, Result<List<ManagementTaskAssigneeOptionDto>>>
{
    public const int MaxResults = 100;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetManagementTaskAssigneeOptionsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ManagementTaskAssigneeOptionDto>>> Handle(
        GetManagementTaskAssigneeOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var allowed = _currentUser.HasPermission(SeedPermissions.ManagementTask.Create)
                      || _currentUser.HasPermission(SeedPermissions.ManagementTask.Assign)
                      || _currentUser.HasPermission(SeedPermissions.ManagementTask.Transfer);
        if (allowed == false)
            return ManagementTaskErrors.ActionForbidden;

        if (request.BranchId is > 0 && ManagementTaskAccess.CanAccessBranch(_currentUser, request.BranchId.Value) == false)
            return ManagementTaskErrors.BranchForbidden;

        var branches = ManagementTaskAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (branches.Count == 0)
            return new List<ManagementTaskAssigneeOptionDto>();

        var query = _db.Users
            .AsNoTracking()
            .Where(x => x.CreationStep == UserCreationStep.Completed
                        && x.UserBranches.Any(b => branches.Contains(b.BranchId)));

        if (string.IsNullOrWhiteSpace(request.Search) == false)
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.FullName.Contains(term) || x.Username.Contains(term));
        }

        var rows = await query
            .OrderBy(x => x.FullName)
            .Take(MaxResults)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.Username,
                BranchIds = x.UserBranches
                    .Where(b => branches.Contains(b.BranchId))
                    .Select(b => b.BranchId)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new ManagementTaskAssigneeOptionDto(x.Id, x.FullName, x.Username, x.BranchIds))
            .ToList();
    }
}
