using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.UpdatePermissions;

/// <summary>
/// مسار قديم: يضبط السماحات الخاصة فقط. لا يمسح المنع.
/// يتطلب ManagePermissions + سوبر أدمن + expectedVersion إلزامي.
/// </summary>
public class UpdateUserPermissionsHandler : IRequestHandler<UpdateUserPermissionsCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public UpdateUserPermissionsHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateUserPermissionsCommand request, CancellationToken cancellationToken)
    {
        var auth = await UserPermissionGuard.EnsureRealSuperAdminCanManageAsync(
            _dbContext,
            _currentUserService,
            request.UserId,
            cancellationToken);
        if (auth.IsFailed)
            return auth;

        var requestedPermissionIds = request.PermissionIds.Distinct().ToList();

        var existingPermissionsCount = await _dbContext.Permissions
            .CountAsync(x => requestedPermissionIds.Contains(x.Id), cancellationToken);
        if (existingPermissionsCount != requestedPermissionIds.Count)
            return UserErrors.SomePermissionsNotFound;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _dbContext.Users
                .Include(x => x.UserPermissions)
                .Include(x => x.UserPermissionDenials)
                .Include(x => x.UserBranches)
                .Include(x => x.UserLogins)
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .ThenInclude(x => x!.RolePermissions)
                .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user is null || user.UserRoles.Count == 0)
                return UserErrors.UserNotFound;

            if (user.PermissionsVersion != request.ExpectedVersion)
                return UserErrors.PermissionsVersionConflict;

            var rolePermissionIds = user.UserRoles
                .SelectMany(x => x.Role!.RolePermissions)
                .Select(x => x.PermissionId)
                .ToHashSet();

            var currentUserPermissionIds = user.UserPermissions
                .Select(x => x.PermissionId)
                .ToHashSet();

            var requestedExtraPermissionIds = requestedPermissionIds
                .Where(id => rolePermissionIds.Contains(id) == false)
                .ToHashSet();

            var permissionsToAdd = requestedExtraPermissionIds
                .Where(id => currentUserPermissionIds.Contains(id) == false)
                .Select(id => new UserPermission(request.UserId, id))
                .ToList();

            var permissionsToRemove = user.UserPermissions
                .Where(x => requestedExtraPermissionIds.Contains(x.PermissionId) == false)
                .ToList();

            var changed = permissionsToAdd.Count > 0 || permissionsToRemove.Count > 0;

            if (changed == false)
            {
                await transaction.CommitAsync(cancellationToken);
                return Result.Success();
            }

            var before = string.Join(',', currentUserPermissionIds.OrderBy(x => x));
            _dbContext.UserPermissions.AddRange(permissionsToAdd);
            _dbContext.UserPermissions.RemoveRange(permissionsToRemove);

            var after = string.Join(',', requestedExtraPermissionIds.OrderBy(x => x));

            user.BumpPermissionsVersion();
            user.InvalidateSessions();
            _dbContext.UserLogins.RemoveRange(user.UserLogins);

            if (user.CreationStep == UserCreationStep.Permissions)
                user.CreationStep = UserCreationStep.Completed;

            foreach (var userBranch in user.UserBranches)
            {
                await _activityLogService.AddAsync(
                    userBranch.BranchId,
                    ActivityType.UserPermissionsUpdated,
                    TargetEntityType.User,
                    request.UserId,
                    details: $"allows before=[{before}] after=[{after}]; denials unchanged");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserErrors.PermissionsVersionConflict;
        }
    }
}
