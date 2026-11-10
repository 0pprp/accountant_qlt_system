namespace Application.FollowerWork.Common;

public sealed record FollowerOldCustomerRowDto(
    int Id,
    int? CustomerId,
    int? SalesRequestId,
    string CustomerName,
    string? Phone,
    string? Address,
    string Status,
    string StatusLabel,
    string? Note,
    string? CompletionNote,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    bool IsDone,
    bool CanCancel,
    int RowVersion);

public sealed record FollowerOldCustomersResponse(
    IReadOnlyList<FollowerOldCustomerRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    string DefinitionHint,
    string TimeZoneId,
    DateTimeOffset GeneratedAtUtc,
    int FollowerId,
    string? FollowerName);
