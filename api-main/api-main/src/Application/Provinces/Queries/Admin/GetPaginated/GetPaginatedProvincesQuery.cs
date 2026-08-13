using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Provinces.Queries.Admin.GetPaginated;

public record GetPaginatedProvincesQuery : IRequest<Result<PaginatedList<GetPaginatedProvincesResponse>>>
{
    public required Pagination Pagination { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}