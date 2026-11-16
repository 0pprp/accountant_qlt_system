using Application.Common.AdminPanel;
using Application.Common.Interfaces;
using Application.Users.Common;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Users.Queries.Admin.GetTabAccess;

public class GetUserTabAccessHandler
    : IRequestHandler<GetUserTabAccessQuery, Result<GetUserTabAccessResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUserTabAccessHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetUserTabAccessResponse>> Handle(
        GetUserTabAccessQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == request.UserId)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.PermissionsVersion,
                RoleName = x.UserRoles.Select(ur => ur.Role!.Name).FirstOrDefault(),
                RolePermissionIds = x.UserRoles
                    .SelectMany(ur => ur.Role!.RolePermissions.Select(rp => rp.PermissionId))
                    .ToList(),
                AllowIds = x.UserPermissions.Select(up => up.PermissionId).ToList(),
                DenyIds = x.UserPermissionDenials.Select(d => d.PermissionId).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || user.RoleName is null)
            return UserErrors.UserNotFound;

        var panelPerms = await _dbContext.Permissions.AsNoTracking()
            .Where(p => p.Name.StartsWith(AdminPanelTabs.PermissionPrefix))
            .ToListAsync(cancellationToken);

        var byName = panelPerms.ToDictionary(p => p.Name, StringComparer.Ordinal);
        var roleIds = user.RolePermissionIds.ToHashSet();
        var allowIds = user.AllowIds.ToHashSet();
        var denyIds = user.DenyIds.ToHashSet();

        var isProtected = user.RoleName == SeedRoles.Admin.Name;
        var canEdit = false;
        if (_currentUserService.UserId is int actorId && isProtected == false && actorId != request.UserId)
        {
            canEdit = await _dbContext.UserRoles.AsNoTracking()
                .AnyAsync(
                    x => x.UserId == actorId && x.Role!.Name == SeedRoles.Admin.Name,
                    cancellationToken);
        }

        var tabs = AdminPanelTabs.ReadyTabs.Select(tab =>
        {
            var permName = AdminPanelTabs.PermissionName(tab.TabKey);
            byName.TryGetValue(permName, out var perm);
            var permId = perm?.Id ?? 0;
            var roleDefault = permId > 0 && roleIds.Contains(permId);
            string? ov = null;
            if (permId > 0 && denyIds.Contains(permId))
                ov = "deny";
            else if (permId > 0 && allowIds.Contains(permId))
                ov = "allow";

            var effective = ov switch
            {
                "deny" => false,
                "allow" => true,
                _ => roleDefault
            };

            return new UserTabAccessRowDto
            {
                TabKey = tab.TabKey,
                ParentKey = tab.ParentKey,
                DisplayName = tab.DisplayNameAr,
                RoleDefaultEnabled = roleDefault,
                EffectiveEnabled = effective,
                Override = ov
            };
        }).ToList();

        return new GetUserTabAccessResponse
        {
            UserId = user.Id,
            FullName = user.FullName ?? string.Empty,
            RoleDisplayName = user.RoleName ?? string.Empty,
            IsProtectedAccount = isProtected,
            PermissionsVersion = user.PermissionsVersion,
            CanEdit = canEdit,
            Tabs = tabs
        };
    }
}
