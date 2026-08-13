namespace Application.Common.Models.TokenFactory;

public record CreateJwtDto
{
    public int Id { get; set; }
    public required List<string> Roles { get; set; }
    public required List<string> Permissions { get; set; }
    public Guid SecurityStamp { get; set; }
    public required List<int> BranchIds { get; set; }
}