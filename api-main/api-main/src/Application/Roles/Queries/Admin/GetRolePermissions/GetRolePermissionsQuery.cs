namespace Application.Roles.Queries.Admin.GetRolePermissions;

public record GetRolePermissionsQuery(int RoleId) : IRequest<Result<GetRolePermissionsResponse>>;