using Application.Common.Models;
using Application.Common.Models.Sorting;
using Domain.Entities.SafeAggregate.Enums;

namespace Application.Safes.Queries.Admin.GetMovements;

public record GetSafeMovementsQuery : IRequest<Result<PaginatedList<GetSafeMovementsResponse>>>
{
    public int SafeId { get; set; }
    public required Pagination Pagination { get; set; }
    public required GetSafeMovementsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetSafeMovementsFilter
{
    public SafeMovementKind? Kind { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? SearchTerm { get; set; }
}
