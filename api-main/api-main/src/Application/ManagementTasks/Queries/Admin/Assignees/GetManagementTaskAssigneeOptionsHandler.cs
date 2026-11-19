using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.SalesRequests.Common;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.ManagementTasks.Queries.Admin.Assignees;

public class GetManagementTaskAssigneeOptionsHandler
    : IRequestHandler<GetManagementTaskAssigneeOptionsQuery, Result<List<ManagementTaskAssigneeOptionDto>>>
{
    public const int MaxResults = 100;
    public const string BranchManagerAudience = "branchManager";

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

        var audience = request.Audience?.Trim();
        var branchManagersOnly = string.Equals(audience, BranchManagerAudience, StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(audience) == false && branchManagersOnly == false)
            return ManagementTaskErrors.InvalidAudience;

        if (request.BranchId is > 0 && ManagementTaskAccess.CanAccessBranch(_currentUser, request.BranchId.Value) == false)
            return ManagementTaskErrors.BranchForbidden;

        var branches = ManagementTaskAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (branches.Count == 0)
            return new List<ManagementTaskAssigneeOptionDto>();

        var query = _db.Users
            .AsNoTracking()
            .Where(x => x.CreationStep == UserCreationStep.Completed
                        && x.UserBranches.Any(b => branches.Contains(b.BranchId)));

        if (branchManagersOnly)
            query = query.Where(x => x.UserRoles.Any(r => r.Role != null && r.Role.Name == SeedRoles.BranchManager.Name));

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
