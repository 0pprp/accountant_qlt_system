namespace Application.Safes.Queries.Admin.GetAll;

public record GetAllSafesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? BranchId { get; set; }
}