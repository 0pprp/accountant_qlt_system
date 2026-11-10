namespace Application.SalesRecords.Common;

public sealed class ListSalesRecordsResponse
{
    public required IReadOnlyList<SalesRecordRowDto> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required SalesRecordsTotalsDto Totals { get; init; }
    public required IReadOnlyList<int> BranchScope { get; init; }
    public required string CurrencyNote { get; init; }
}

public sealed class SalesRecordsTotalsDto
{
    public required double TotalSellAmount { get; init; }
    public required double TotalPrepaymentAmount { get; init; }
    public required double TotalReceipts { get; init; }
    public required double TotalRemaining { get; init; }
    public required string ScopeNote { get; init; }
}

public sealed class SalesRecordRowDto
{
    public required int OrderId { get; init; }
    public required DateOnly? SaleDate { get; init; }
    public required string ApprovalStatus { get; init; }
    public required int CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public int? SellerId { get; init; }
    public string? SellerName { get; init; }
    public required int BranchId { get; init; }
    public required string BranchName { get; init; }
    public required double SellAmount { get; init; }
    public required double PrepaymentAmount { get; init; }
    public required double DailyInstallmentAmount { get; init; }
    public required double ReceiptsTotal { get; init; }
    public required double RemainingAmount { get; init; }
    public int? LinkedSalesRequestId { get; init; }
}
