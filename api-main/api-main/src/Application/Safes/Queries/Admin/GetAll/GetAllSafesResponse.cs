using Domain.Entities.SafeAggregate.Enums;

namespace Application.Safes.Queries.Admin.GetAll;

public record GetAllSafesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public SafeKind Kind { get; set; }
    public int BranchId { get; set; }
    public int? OrderListId { get; set; }
    public string? OrderListName { get; set; }
    public int? UserId { get; set; }
    public string? UserFullName { get; set; }
    public double Balance { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int MovementsCount { get; set; }
}
