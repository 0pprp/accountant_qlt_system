using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;

namespace Application.Common.Models.SignIn;

public record UserLoginDto
{
    public int UserId { get; init; }
    public Guid UserSecurityStamp { get; init; }
    public required string RefreshToken { get; set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public required List<string> Roles { get; init; }
    public required List<Permission> UserPermissions { get; init; }
    public required List<Permission> RolePermissions { get; init; }
    public List<Permission> Permissions => UserPermissions.Union(RolePermissions).ToList();
    public required List<Branch> Branches { get; init; }
}