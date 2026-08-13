using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Users.Queries.Admin.GetPaginated;

public record GetPaginatedUsersQuery : IRequest<Result<GetPaginatedUsersResponse>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedUsersFilterDto Filter { get; set; }
    public List<string>? SelectedColumns { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedUsersFilterDto
{
    public int BranchId { get; set; }
    public int[]? RoleIds { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}