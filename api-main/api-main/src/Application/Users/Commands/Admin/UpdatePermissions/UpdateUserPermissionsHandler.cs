using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.UpdatePermissions;

public class UpdateUserPermissionsHandler : IRequestHandler<UpdateUserPermissionsCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateUserPermissionsHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateUserPermissionsCommand request, CancellationToken cancellationToken)
    {
        var userRoles = await _dbContext.UserRoles
            .Include(x => x.User)
            .ThenInclude(x => x!.UserBranches)
            .Include(x => x.Role)
            .ThenInclude(x => x!.RolePermissions)
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (userRoles.Count == 0)
            return UserErrors.UserNotFound;

        var user = userRoles.First().User!;

        // Super Admin permissions come from the Admin role and must not be changed from this screen.
        if (userRoles.Any(x => x.Role!.Name == global::Application.Common.SeedData.Roles.Admin.Name))
            return UserErrors.CannotUpdateAdminPermissions;

        var requestedPermissionIds = request.PermissionIds.Distinct().ToList();

        var existingPermissionsCount = await _dbContext.Permissions
            .CountAsync(x => requestedPermissionIds.Contains(x.Id), cancellationToken);
        if (existingPermissionsCount != requestedPermissionIds.Count)
            return UserErrors.SomePermissionsNotFound;

        var currentUserPermissions = await _dbContext.UserPermissions
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var rolePermissionIds = userRoles
            .SelectMany(x => x.Role!.RolePermissions)
            .Select(x => x.PermissionId)
            .ToHashSet();

        var currentUserPermissionIds = currentUserPermissions
            .Select(x => x.PermissionId)
            .ToHashSet();

        // Incoming IDs are the displayed set. RolePermissions stay on the role and are never copied or deleted here.
        var requestedExtraPermissionIds = requestedPermissionIds
            .Where(id => rolePermissionIds.Contains(id) == false)
            .ToHashSet();

        // Add extras that are requested and not already stored as UserPermissions.
        var permissionsToAdd = requestedExtraPermissionIds
            .Where(id => currentUserPermissionIds.Contains(id) == false)
            .Select(id => new UserPermission(request.UserId, id))
            .ToList();

        // Remove extras that were turned off (no longer present in the incoming IDs).
        var permissionsToRemove = currentUserPermissions
            .Where(x => requestedPermissionIds.Contains(x.PermissionId) == false)
            .ToList();

        _dbContext.UserPermissions.AddRange(permissionsToAdd);
        _dbContext.UserPermissions.RemoveRange(permissionsToRemove);

        if (user.CreationStep == UserCreationStep.Permissions)
            user.CreationStep = UserCreationStep.Completed;

        foreach (var userBranch in user.UserBranches)
        {
            await _activityLogService.AddAsync(
                userBranch.BranchId,
                ActivityType.UserPermissionsUpdated,
                TargetEntityType.User,
                request.UserId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
