using Application.Common.Interfaces;
using Application.Permissions.Common;
using Application.Users.Common;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Users.Queries.Admin.GetPermissionOverrides;

public class GetUserPermissionOverridesHandler
    : IRequestHandler<GetUserPermissionOverridesQuery, Result<GetUserPermissionOverridesResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUserPermissionOverridesHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetUserPermissionOverridesResponse>> Handle(
        GetUserPermissionOverridesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == request.UserId)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.PermissionsVersion,
                Role = x.UserRoles.Select(ur => new
                {
                    ur.RoleId,
                    Name = ur.Role!.Name,
                    DisplayName = ur.Role.DisplayName
                }).FirstOrDefault(),
                RolePermissionIds = x.UserRoles
                    .SelectMany(ur => ur.Role!.RolePermissions.Select(rp => rp.PermissionId))
                    .ToList(),
                AllowIds = x.UserPermissions.Select(up => up.PermissionId).ToList(),
                DenyIds = x.UserPermissionDenials.Select(d => d.PermissionId).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || user.Role is null)
            return UserErrors.UserNotFound;

        var allPermissions = await _dbContext.Permissions.AsNoTracking()
            .OrderBy(p => p.ScopeDisplayName)
            .ThenBy(p => p.DisplayName)
            .ToListAsync(cancellationToken);

        var roleIdSet = user.RolePermissionIds.ToHashSet();
        var allowSet = user.AllowIds.ToHashSet();
        var denySet = user.DenyIds.ToHashSet();

        var rolePermissions = allPermissions
            .Where(p => roleIdSet.Contains(p.Id))
            .Select(ToDto)
            .ToList();

        var allowOverrides = allPermissions
            .Where(p => allowSet.Contains(p.Id))
            .Select(ToDto)
            .ToList();

        var denyOverrides = allPermissions
            .Where(p => denySet.Contains(p.Id))
            .Select(ToDto)
            .ToList();

        var effective = EffectivePermissionCalculator.Compute(
            allPermissions.Where(p => roleIdSet.Contains(p.Id)),
            allPermissions.Where(p => allowSet.Contains(p.Id)),
            allPermissions.Where(p => denySet.Contains(p.Id)));

        var effectiveIds = effective.Select(p => p.Id).ToHashSet();

        var rows = allPermissions.Select(p =>
        {
            var mode = PermissionOverrideMode.Inherit;
            if (denySet.Contains(p.Id))
                mode = PermissionOverrideMode.Deny;
            else if (allowSet.Contains(p.Id))
                mode = PermissionOverrideMode.Allow;

            return new PermissionOverrideRowDto
            {
                PermissionId = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Scope = p.Scope,
                ScopeDisplayName = p.ScopeDisplayName,
                RoleAllows = roleIdSet.Contains(p.Id),
                Mode = mode,
                EffectiveAllowed = effectiveIds.Contains(p.Id)
            };
        }).ToList();

        var isProtected = user.Role.Name == SeedRoles.Admin.Name;
        var canEdit = false;
        if (_currentUserService.UserId is int actorId && isProtected == false && actorId != request.UserId)
        {
            canEdit = await _dbContext.UserRoles.AsNoTracking()
                .AnyAsync(
                    x => x.UserId == actorId && x.Role!.Name == SeedRoles.Admin.Name,
                    cancellationToken);
        }

        return new GetUserPermissionOverridesResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            RoleId = user.Role.RoleId,
            RoleName = user.Role.Name,
            RoleDisplayName = user.Role.DisplayName,
            PermissionsVersion = user.PermissionsVersion,
            IsProtectedAccount = isProtected,
            CanEditOverrides = canEdit,
            RolePermissions = rolePermissions,
            AllowOverrides = allowOverrides,
            DenyOverrides = denyOverrides,
            EffectivePermissions = effective.Select(ToDto).ToList(),
            Rows = rows
        };
    }

    private static GetPermissionDto ToDto(Domain.Entities.RoleAggregate.Permission p) => new()
    {
        Id = p.Id,
        Name = p.Name
    };
}
