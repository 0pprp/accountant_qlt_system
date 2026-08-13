using Application.Common.Models;

namespace Application.Users.Queries.Admin.GetPaginated;

public record GetPaginatedUsersResponse
{
    public required PaginatedList<Dictionary<string, object?>> PaginatedUsers { get; set; }
}