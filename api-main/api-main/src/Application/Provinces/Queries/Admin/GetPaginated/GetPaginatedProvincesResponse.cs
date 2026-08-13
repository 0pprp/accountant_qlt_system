namespace Application.Provinces.Queries.Admin.GetPaginated;

public record GetPaginatedProvincesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
}