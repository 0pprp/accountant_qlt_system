namespace Application.Branches.Queries.Admin.GetBranchWarehouses;

public record GetBranchWarehousesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
}