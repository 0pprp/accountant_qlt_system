using Application.Branches.Common;
using Application.Permissions.Common;

namespace Application.Users.Commands.Common.Login;

public record LoginResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required Dictionary<string, List<GetPermissionDto>> Permissions { get; set; }
    public required List<GetBranchDto> Branches { get; set; }
    public required List<string> Roles { get; set; }
    public bool IsSuperAdmin { get; set; }
}