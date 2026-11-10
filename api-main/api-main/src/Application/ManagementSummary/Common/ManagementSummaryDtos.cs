namespace Application.ManagementSummary.Common;

public sealed record ManagementSummaryBranchDto(int Id, string Name);

public sealed record ManagementSummaryCellDto(
    int Day,
    int BranchId,
    decimal Amount,
    string Availability);

public sealed record ManagementSummaryDayTotalDto(int Day, decimal Amount);

public sealed record ManagementSummaryBranchTotalDto(int BranchId, decimal Amount);

public sealed record ManagementSummaryBoxCardDto(
    string Key,
    string Label,
    decimal? Amount,
    string Availability,
    string? Reason);

public sealed record ManagementSummaryMatrixResponse(
    int Year,
    int Month,
    int DaysInMonth,
    string TimeZone,
    string Currency,
    string Metric,
    string MetricLabel,
    IReadOnlyList<ManagementSummaryBranchDto> Branches,
    IReadOnlyList<ManagementSummaryCellDto> Cells,
    IReadOnlyList<ManagementSummaryDayTotalDto> DayTotals,
    IReadOnlyList<ManagementSummaryBranchTotalDto> BranchTotals,
    decimal MonthTotal,
    IReadOnlyList<ManagementSummaryBoxCardDto> BoxCards,
    DateTimeOffset GeneratedAt);

public sealed record ManagementSummaryDetailRowDto(
    Guid Id,
    string Title,
    string? Subtitle,
    decimal Amount,
    DateTimeOffset OccurredAt,
    string? Reference);

public sealed record ManagementSummaryDetailsResponse(
    int Year,
    int Month,
    int Day,
    int BranchId,
    string BranchName,
    string Metric,
    string MetricLabel,
    decimal CellTotal,
    string Currency,
    IReadOnlyList<ManagementSummaryDetailRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    DateTimeOffset GeneratedAt);
