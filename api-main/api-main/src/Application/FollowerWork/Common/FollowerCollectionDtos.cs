namespace Application.FollowerWork.Common;

public sealed record FollowerCollectionListDto(
    int ListId,
    string ListName,
    int MandobUserId,
    string? MandobName,
    bool FollowerCollectionEnabled,
    DateTimeOffset? AssignedAtUtc,
    int CollectionToggleVersion,
    DateOnly? LastDelegatePaymentAt,
    double LastDelegatePaymentAmount,
    DateOnly? LastFollowerPaymentAt,
    double LastFollowerPaymentAmount);

public sealed record FollowerCollectionPaymentDto(
    int PaymentId,
    int OrderId,
    int CustomerId,
    string CustomerName,
    int ListId,
    string? ListName,
    double Amount,
    string Currency,
    DateOnly PaymentDate,
    int? CollectedByUserId,
    string? CollectedByName,
    string Source);

public sealed record FollowerCollectionTotalsDto(
    double PaymentsPageAmount,
    double PaymentsFilteredAmount,
    int PaymentsFilteredCount,
    string Currency);

public sealed record FollowerCollectionResponse(
    IReadOnlyList<FollowerCollectionListDto> Lists,
    IReadOnlyList<FollowerCollectionPaymentDto> Payments,
    FollowerCollectionTotalsDto Totals,
    int PaymentsTotalCount,
    int PaymentsPage,
    int PaymentsPageSize,
    int PaymentsTotalPages,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string TimeZoneId,
    DateTimeOffset GeneratedAtUtc,
    int FollowerId,
    string? FollowerName);

public sealed record SetFollowerCollectionEnabledResultDto(
    int ListId,
    string ListName,
    bool Enabled,
    DateTimeOffset? AssignedAtUtc,
    int CollectionToggleVersion,
    bool Changed);
