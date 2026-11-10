namespace Application.CustomerDecisions.Common;

public sealed record LegalNameRowDto(
    int CustomerId,
    string CustomerName,
    string? Phone,
    int BranchId,
    string? BranchName,
    int? OrderListId,
    string? OrderListName,
    double SaleTotal,
    double WeekPaid,
    double Remaining,
    double PaidPercent,
    DateOnly? LastPaymentDate,
    int DaysSinceLastPayment);

public sealed record LegalNamesResponse(
    IReadOnlyList<LegalNameRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record CustomerDecisionRowDto(
    int Id,
    int CustomerId,
    string CustomerName,
    string? Phone,
    int BranchId,
    string DecisionType,
    string DecisionTypeLabel,
    string Note,
    string? ActorName,
    double? WeekPaid,
    double? AmountTotalSales,
    double? PaidPercent,
    DateTimeOffset? SnoozeUntil,
    DateTimeOffset CreatedAtUtc,
    bool CustomerIsLegal,
    bool CustomerIsFakeSale);

public sealed record CustomerDecisionListResponse(
    IReadOnlyList<CustomerDecisionRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record PostBranchDecisionResultDto(
    int DecisionId,
    string DecisionType,
    string DecisionTypeLabel,
    int? WaiverRequestId,
    int? LegalConversionId);
