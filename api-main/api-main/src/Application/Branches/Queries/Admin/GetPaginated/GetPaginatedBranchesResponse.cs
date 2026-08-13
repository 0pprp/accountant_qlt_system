namespace Application.Branches.Queries.Admin.GetPaginated;

public record GetPaginatedBranchesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required GetProvinceDto Province { get; set; }
}

public record GetProvinceDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}