using Application.Common.AdminPanel;
using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.PatchTabAccess;

public class PatchUserTabAccessHandler : IRequestHandler<PatchUserTabAccessCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public PatchUserTabAccessHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(PatchUserTabAccessCommand request, CancellationToken cancellationToken)
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

        var duplicates = request.Changes
            .GroupBy(c => c.TabKey, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicates.Count > 0)
            return UserErrors.DuplicateTabKeyChange;

        foreach (var change in request.Changes)
        {
            if (string.IsNullOrWhiteSpace(change.TabKey)
                || AdminPanelTabs.TryGet(change.TabKey, out var tab) == false
                || tab.Ready == false)
            {
                return UserErrors.UnknownOrNotReadyTabKey;
            }
        }

        var permNames = request.Changes
            .Select(c => AdminPanelTabs.PermissionName(c.TabKey))
            .ToList();

        var permissions = await _dbContext.Permissions
            .Where(p => permNames.Contains(p.Name))
            .ToListAsync(cancellationToken);
        if (permissions.Count != permNames.Distinct(StringComparer.Ordinal).Count())
            return UserErrors.SomePermissionsNotFound;

        var permissionByTab = permissions.ToDictionary(
            p => AdminPanelTabs.TabKeyFromPermission(p.Name)!,
            StringComparer.Ordinal);

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
            var auditParts = new List<string>();

            static string ModeOf(bool allow, bool deny) =>
                deny ? "deny" : allow ? "allow" : "inherit";

            foreach (var change in request.Changes)
            {
                var permission = permissionByTab[change.TabKey];
                var permissionId = permission.Id;
                var hadAllow = allows.ContainsKey(permissionId);
                var hadDeny = denials.ContainsKey(permissionId);
                var beforeMode = ModeOf(hadAllow, hadDeny);

                // true=allow, false=deny, null=inherit
                if (change.Enabled == true)
                {
                    if (hadDeny)
                    {
                        _dbContext.UserPermissionDenials.Remove(denials[permissionId]);
                        denials.Remove(permissionId);
                    }

                    if (hadAllow == false)
                    {
                        var allow = new UserPermission(request.UserId, permissionId);
                        _dbContext.UserPermissions.Add(allow);
                        allows[permissionId] = allow;
                    }
                }
                else if (change.Enabled == false)
                {
                    if (hadAllow)
                    {
                        _dbContext.UserPermissions.Remove(allows[permissionId]);
                        allows.Remove(permissionId);
                    }

                    if (hadDeny == false)
                    {
                        var denial = new UserPermissionDenial(request.UserId, permissionId);
                        _dbContext.UserPermissionDenials.Add(denial);
                        denials[permissionId] = denial;
                    }
                }
                else
                {
                    if (hadAllow)
                    {
                        _dbContext.UserPermissions.Remove(allows[permissionId]);
                        allows.Remove(permissionId);
                    }

                    if (hadDeny)
                    {
                        _dbContext.UserPermissionDenials.Remove(denials[permissionId]);
                        denials.Remove(permissionId);
                    }
                }

                var afterMode = ModeOf(
                    allows.ContainsKey(permissionId),
                    denials.ContainsKey(permissionId));
                if (beforeMode != afterMode)
                    auditParts.Add($"{change.TabKey}:{beforeMode}->{afterMode}");
            }

            if (auditParts.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return Result.Success();
            }

            user.BumpPermissionsVersion();
            user.InvalidateSessions();
            _dbContext.UserLogins.RemoveRange(user.UserLogins);

            if (user.CreationStep == UserCreationStep.Permissions)
                user.CreationStep = UserCreationStep.Completed;

            var details = "tab-access;" + string.Join(';', auditParts);
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
