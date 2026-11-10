using Application.Common.Models;

namespace Application.SalesRequests.Queries.Admin.GetMatchHits;

public record GetSalesRequestMatchHitsQuery : IRequest<Result<PaginatedList<GetSalesRequestMatchHitItem>>>
{
    public int RequestId { get; init; }
    public required string Criterion { get; init; }
    public required Pagination Pagination { get; init; }
}

public record GetSalesRequestMatchHitItem
{
    public int CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public required string MatchReason { get; init; }
    public required List<string> HighlightedParts { get; init; }
    public string? Phone { get; init; }
    public string? BranchName { get; init; }
    public string? RatingLabel { get; init; }
    public int? Score { get; init; }
    public bool? IsLegal { get; init; }
    public double? SellTotal { get; init; }
    public double? PaidTotal { get; init; }
    public double? Remaining { get; init; }
    public int? PaymentCount { get; init; }
    public int? PaymentDays { get; init; }
    public DateOnly? SaleDate { get; init; }
    public bool CanOpenProfile { get; init; }
}
