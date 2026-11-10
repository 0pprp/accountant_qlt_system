using Application.Permissions.Common;
using Application.Users.Common;

namespace Application.Users.Queries.Admin.GetPermissionOverrides;

public record GetUserPermissionOverridesResponse
{
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public string RoleDisplayName { get; init; } = string.Empty;
    public int PermissionsVersion { get; init; }
    public bool IsProtectedAccount { get; init; }
    public bool CanEditOverrides { get; init; }
    public required List<GetPermissionDto> RolePermissions { get; init; }
    public required List<GetPermissionDto> AllowOverrides { get; init; }
    public required List<GetPermissionDto> DenyOverrides { get; init; }
    public required List<GetPermissionDto> EffectivePermissions { get; init; }
    public required List<PermissionOverrideRowDto> Rows { get; init; }
}

public record PermissionOverrideRowDto
{
    public int PermissionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string ScopeDisplayName { get; init; } = string.Empty;
    public bool RoleAllows { get; init; }
    public PermissionOverrideMode Mode { get; init; }
    public bool EffectiveAllowed { get; init; }
}
