namespace Application.Waivers.Common;

public sealed record WaiverSummaryDto(
    int Id,
    int CustomerId,
    string CustomerName,
    string? CustomerPhone,
    int BranchId,
    string? BranchName,
    int? OrderId,
    string Status,
    string StatusLabel,
    string RequestNote,
    string? RequesterName,
    double? ApprovedAmount,
    double? AppliedAmount,
    bool Applied,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DecidedAtUtc,
    DateTimeOffset? AppliedAtUtc,
    int ExpectedVersion);

public sealed record WaiverDetailDto(
    int Id,
    int CustomerId,
    string CustomerName,
    string? CustomerPhone,
    int BranchId,
    string? BranchName,
    int? OrderId,
    double? OrderSellAmount,
    double? OrderPaidTotal,
    double? OrderRemaining,
    string Status,
    string StatusLabel,
    string RequestNote,
    int RequesterUserId,
    string? RequesterName,
    string? DecisionNote,
    string? DecidedByName,
    double? ApprovedAmount,
    double? AppliedAmount,
    bool Applied,
    string? AppliedByName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DecidedAtUtc,
    DateTimeOffset? AppliedAtUtc,
    int ExpectedVersion);

public sealed record WaiverListResponse(
    IReadOnlyList<WaiverSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int PendingCount,
    int AwaitingAccountantCount);

public sealed record WaiverStatusCountsDto(int Pending, int AwaitingAccountant, int Approved, int Rejected);
