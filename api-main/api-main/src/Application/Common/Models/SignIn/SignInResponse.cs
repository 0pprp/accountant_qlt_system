using Application.Branches.Common;
using Application.Permissions.Common;

namespace Application.Common.Models.SignIn;

public record SignInResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required Dictionary<string, List<GetPermissionDto>> Permissions { get; set; }
    public required List<GetBranchDto> Branches { get; set; }
    public required List<string> Roles { get; set; }
}