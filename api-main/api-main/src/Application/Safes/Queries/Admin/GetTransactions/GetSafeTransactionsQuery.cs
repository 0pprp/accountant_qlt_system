using Application.Common.Models;

namespace Application.Safes.Queries.Admin.GetTransactions;

public record GetSafeTransactionsQuery : IRequest<Result<PaginatedList<GetSafeTransactionsResponse>>>
{
    public int SafeId { get; set; }
    public required Pagination Pagination { get; set; }
    public required GetSafeTransactionsFilter Filter { get; set; }
}

public record GetSafeTransactionsFilter
{
}