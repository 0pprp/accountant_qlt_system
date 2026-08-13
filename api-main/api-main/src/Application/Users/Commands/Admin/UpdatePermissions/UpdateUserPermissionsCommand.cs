namespace Application.Users.Commands.Admin.UpdatePermissions;

public record UpdateUserPermissionsCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public required List<int> PermissionIds { get; set; }
}

public record UpdateUserPermissionsDto(List<int> PermissionIds);