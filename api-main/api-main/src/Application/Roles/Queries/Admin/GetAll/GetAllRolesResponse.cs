namespace Application.Roles.Queries.Admin.GetAll;

public record GetAllRolesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
}