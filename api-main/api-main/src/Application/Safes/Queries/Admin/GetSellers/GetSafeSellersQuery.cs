using Application.Common.Models;

namespace Application.Safes.Queries.Admin.GetSellers;

public record GetSafeSellersQuery : IRequest<Result<PaginatedList<GetSafeSellersResponse>>>
{
    public int SafeId { get; set; }
    public required Pagination Pagination { get; set; }
    public required GetSafeSellersFilter Filter { get; set; }
}

public record GetSafeSellersFilter
{
    public string? SearchTerm { get; set; }
}