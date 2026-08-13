namespace Application.Branches.Common;

public record GetBranchDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}