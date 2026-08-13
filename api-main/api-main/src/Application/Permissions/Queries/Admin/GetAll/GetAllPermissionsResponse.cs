namespace Application.Permissions.Queries.Admin.GetAll;

public record GetAllPermissionsResponse(Dictionary<string, List<GetAllPermissionsItem>> Permissions);

public record GetAllPermissionsItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Scope { get; set; }
    public required string ScopeDisplayName { get; set; }
}