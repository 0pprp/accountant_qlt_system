using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;

namespace Application.Common.Models.SignIn;

public record SignInRequest
{
    public int Id { get; init; }
    public Guid SecurityStamp { get; init; }
    public required List<string> Roles { get; init; }
    public required List<Permission> UserPermissions { get; init; }
    public required List<Permission> RolePermissions { get; init; }
    public List<Permission> Permissions => UserPermissions.Union(RolePermissions).ToList();
    public required List<Branch> Branches { get; init; }
}