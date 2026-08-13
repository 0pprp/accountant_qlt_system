using Application.Permissions.Common;

namespace Application.Roles.Queries.Admin.GetRolePermissions;

public record GetRolePermissionsResponse(Dictionary<string, List<GetPermissionDto>> Permissions);