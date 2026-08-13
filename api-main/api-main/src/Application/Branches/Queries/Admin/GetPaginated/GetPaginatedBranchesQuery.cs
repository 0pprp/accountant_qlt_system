using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Branches.Queries.Admin.GetPaginated;

public record GetPaginatedBranchesQuery : IRequest<Result<PaginatedList<GetPaginatedBranchesResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedBranchesFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedBranchesFilter
{
    public string? SearchTerm { get; set; }
    public int? ProvinceId { get; set; }
}