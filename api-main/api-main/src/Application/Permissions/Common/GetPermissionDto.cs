namespace Application.Permissions.Common;

public record GetPermissionDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}