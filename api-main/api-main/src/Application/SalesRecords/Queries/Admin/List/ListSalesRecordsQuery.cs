using Application.SalesRecords.Common;

namespace Application.SalesRecords.Queries.Admin.List;

public sealed class ListSalesRecordsQuery : IRequest<Result<ListSalesRecordsResponse>>
{
    public int? BranchId { get; init; }
    public int? SellerId { get; init; }
    public string? Search { get; init; }
    public DateOnly? SaleDateFrom { get; init; }
    public DateOnly? SaleDateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Sort { get; init; }
}
