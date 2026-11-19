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
    int ExpectedVersion,
    WaiverAccountDto? Account = null,
    IReadOnlyList<WaiverRelatedOrderDto>? RelatedOrders = null,
    IReadOnlyList<WaiverRelatedCustomerDto>? RelatedCustomers = null);

public sealed record WaiverAccountDto(
    double CustomerSellTotal,
    double CustomerPaidTotal,
    double CustomerRemaining,
    double CustomerRemainingRounded);

public sealed record WaiverRelatedOrderDto(
    int OrderId,
    DateOnly? SaleDate,
    double SellAmount,
    double PaidTotal,
    double Remaining,
    string ApprovalStatus,
    bool IsWaiverOrder);

public sealed record WaiverRelatedCustomerDto(
    int CustomerId,
    string FullName,
    string? Phone,
    IReadOnlyList<string> MatchReasons,
    double SellTotal,
    double PaidTotal,
    double RemainingRounded);

public sealed record WaiverListResponse(
    IReadOnlyList<WaiverSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int PendingCount,
    int AwaitingAccountantCount,
    WaiverStatusCountsDto? Counts = null);

public sealed record WaiverStatusCountsDto(int Pending, int AwaitingAccountant, int Approved, int Rejected);
