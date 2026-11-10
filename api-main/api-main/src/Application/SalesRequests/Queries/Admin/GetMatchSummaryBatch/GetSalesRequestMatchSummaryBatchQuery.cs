using Application.SalesRequests.Common;

namespace Application.SalesRequests.Queries.Admin.GetMatchSummaryBatch;

public record GetSalesRequestMatchSummaryBatchQuery(List<int> RequestIds)
    : IRequest<Result<GetSalesRequestMatchSummaryBatchResponse>>;

public record GetSalesRequestMatchSummaryBatchResponse
{
    public required List<SalesMatchSummaryItemDto> Items { get; init; }
}
