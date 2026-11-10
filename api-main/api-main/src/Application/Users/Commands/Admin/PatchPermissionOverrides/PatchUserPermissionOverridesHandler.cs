using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.PatchPermissionOverrides;

public class PatchUserPermissionOverridesHandler
    : IRequestHandler<PatchUserPermissionOverridesCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public PatchUserPermissionOverridesHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(
        PatchUserPermissionOverridesCommand request,
        CancellationToken cancellationToken)
    {
        var auth = await UserPermissionGuard.EnsureRealSuperAdminCanManageAsync(
            _dbContext,
            _currentUserService,
            request.UserId,
            cancellationToken);
        if (auth.IsFailed)
            return auth;

        if (request.Changes.Count == 0)
            return Result.Success();

        var duplicateIds = request.Changes
            .GroupBy(c => c.PermissionId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateIds.Count > 0)
            return UserErrors.DuplicatePermissionOverride;

        if (request.Changes.Any(c => Enum.IsDefined(typeof(PermissionOverrideMode), c.Mode) == false))
            return UserErrors.InvalidPermissionOverrideMode;

        var permissionIds = request.Changes.Select(c => c.PermissionId).ToList();
        var existingCount = await _dbContext.Permissions
            .CountAsync(x => permissionIds.Contains(x.Id), cancellationToken);
        if (existingCount != permissionIds.Count)
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
                .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user is null)
                return UserErrors.UserNotFound;

            if (user.PermissionsVersion != request.ExpectedVersion)
                return UserErrors.PermissionsVersionConflict;

            var allows = user.UserPermissions.ToDictionary(x => x.PermissionId);
            var denials = user.UserPermissionDenials.ToDictionary(x => x.PermissionId);
            var changed = false;
            var auditParts = new List<string>();

            static string ModeOf(bool allow, bool deny) =>
                deny ? "deny" : allow ? "allow" : "inherit";

            foreach (var change in request.Changes)
            {
                var hadAllow = allows.ContainsKey(change.PermissionId);
                var hadDeny = denials.ContainsKey(change.PermissionId);
                var beforeMode = ModeOf(hadAllow, hadDeny);

                switch (change.Mode)
                {
                    case PermissionOverrideMode.Allow:
                        if (hadDeny)
                        {
                            _dbContext.UserPermissionDenials.Remove(denials[change.PermissionId]);
                            denials.Remove(change.PermissionId);
                            changed = true;
                        }

                        if (hadAllow == false)
                        {
                            var allow = new UserPermission(request.UserId, change.PermissionId);
                            _dbContext.UserPermissions.Add(allow);
                            allows[change.PermissionId] = allow;
                            changed = true;
                        }

                        break;

                    case PermissionOverrideMode.Deny:
                        if (hadAllow)
                        {
                            _dbContext.UserPermissions.Remove(allows[change.PermissionId]);
                            allows.Remove(change.PermissionId);
                            changed = true;
                        }

                        if (hadDeny == false)
                        {
                            var denial = new UserPermissionDenial(request.UserId, change.PermissionId);
                            _dbContext.UserPermissionDenials.Add(denial);
                            denials[change.PermissionId] = denial;
                            changed = true;
                        }

                        break;

                    case PermissionOverrideMode.Inherit:
                        if (hadAllow)
                        {
                            _dbContext.UserPermissions.Remove(allows[change.PermissionId]);
                            allows.Remove(change.PermissionId);
                            changed = true;
                        }

                        if (hadDeny)
                        {
                            _dbContext.UserPermissionDenials.Remove(denials[change.PermissionId]);
                            denials.Remove(change.PermissionId);
                            changed = true;
                        }

                        break;

                    default:
                        return UserErrors.InvalidPermissionOverrideMode;
                }

                var afterMode = ModeOf(
                    allows.ContainsKey(change.PermissionId),
                    denials.ContainsKey(change.PermissionId));
                if (beforeMode != afterMode)
                {
                    changed = true;
                    auditParts.Add($"p{change.PermissionId}:{beforeMode}->{afterMode}");
                }
            }

            if (changed == false || auditParts.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return Result.Success();
            }

            user.BumpPermissionsVersion();
            user.InvalidateSessions();
            _dbContext.UserLogins.RemoveRange(user.UserLogins);

            if (user.CreationStep == UserCreationStep.Permissions)
                user.CreationStep = UserCreationStep.Completed;

            var details = string.Join(';', auditParts);
            foreach (var userBranch in user.UserBranches)
            {
                await _activityLogService.AddAsync(
                    userBranch.BranchId,
                    ActivityType.UserPermissionsUpdated,
                    TargetEntityType.User,
                    request.UserId,
                    details: details);
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
