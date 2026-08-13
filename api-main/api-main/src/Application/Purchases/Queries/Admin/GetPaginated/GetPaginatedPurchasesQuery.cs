using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Purchases.Queries.Admin.GetPaginated;

public record GetPaginatedPurchasesQuery : IRequest<Result<PaginatedList<GetPaginatedPurchasesResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedPurchasesFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedPurchasesFilter
{
    public int? BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}