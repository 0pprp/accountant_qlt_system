using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Expenses.Queries.Admin.GetPaginated;

public record GetPaginatedExpensesQuery : IRequest<Result<PaginatedList<GetPaginatedExpensesResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedExpensesFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedExpensesFilter
{
    public int? BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
