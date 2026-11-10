using Application.Users.Common;

namespace Application.Users.Commands.Admin.PatchPermissionOverrides;

public record PatchUserPermissionOverridesCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public int ExpectedVersion { get; set; }
    public required List<PermissionOverrideChangeDto> Changes { get; set; }
}

public record PermissionOverrideChangeDto
{
    public int PermissionId { get; set; }
    public PermissionOverrideMode Mode { get; set; }
}

public record PatchUserPermissionOverridesDto(
    int ExpectedVersion,
    List<PermissionOverrideChangeDto> Changes);
