namespace Application.SalesSummary.Common;

public sealed record SalesSummaryBranchDto(int Id, string Name);

public sealed record SalesSummaryCellDto(int Day, int BranchId, decimal Amount);

public sealed record SalesSummaryDayTotalDto(int Day, decimal Amount);

public sealed record SalesSummaryBranchTotalDto(int BranchId, decimal Amount);

public sealed record SalesSummaryMatrixResponse(
    int Year,
    int Month,
    int DaysInMonth,
    string TimeZone,
    string Currency,
    string Metric,
    string MetricLabel,
    string DayBoundNote,
    IReadOnlyList<SalesSummaryBranchDto> Branches,
    IReadOnlyList<SalesSummaryCellDto> Cells,
    IReadOnlyList<SalesSummaryDayTotalDto> DayTotals,
    IReadOnlyList<SalesSummaryBranchTotalDto> BranchTotals,
    decimal MonthTotal,
    DateTimeOffset GeneratedAt);

public sealed record SalesSummaryDetailRowDto(
    int OrderId,
    string Title,
    string? Subtitle,
    decimal Amount,
    DateTimeOffset OccurredAt);

public sealed record SalesSummaryDetailsResponse(
    int Year,
    int Month,
    int Day,
    int BranchId,
    string BranchName,
    string Metric,
    string MetricLabel,
    decimal CellTotal,
    string Currency,
    string DayBoundNote,
    IReadOnlyList<SalesSummaryDetailRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    DateTimeOffset GeneratedAt);
