namespace Application.Users.Queries.Admin.GetTabAccess;

public sealed class GetUserTabAccessResponse
{
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string RoleDisplayName { get; init; } = string.Empty;
    public bool IsProtectedAccount { get; init; }
    public int PermissionsVersion { get; init; }
    public bool CanEdit { get; init; }
    public required List<UserTabAccessRowDto> Tabs { get; init; }
}

public sealed class UserTabAccessRowDto
{
    public string TabKey { get; init; } = string.Empty;
    public string? ParentKey { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool RoleDefaultEnabled { get; init; }
    public bool EffectiveEnabled { get; init; }
    /// <summary>allow | deny | null (inherit).</summary>
    public string? Override { get; init; }
}
