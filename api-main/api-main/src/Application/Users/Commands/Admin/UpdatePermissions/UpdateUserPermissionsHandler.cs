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

        var userPermissions = await _dbContext.UserPermissions
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var permissions = userRoles.SelectMany(x => x.Role!.RolePermissions).Select(x => x.PermissionId).ToList();
        var customPermissions = userPermissions.Select(x => x.PermissionId).ToList();

        var allPermissions = permissions.Union(customPermissions).ToList();

        var permissionsToAdd = request.PermissionIds.Except(allPermissions).ToList();

        var newUserPermissions = permissionsToAdd.Select(x => new UserPermission(request.UserId, x)).ToList();
        _dbContext.UserPermissions.AddRange(newUserPermissions);

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