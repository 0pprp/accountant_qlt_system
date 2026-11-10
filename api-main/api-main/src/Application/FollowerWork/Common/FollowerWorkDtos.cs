namespace Application.FollowerWork.Common;

public sealed record FollowerWorkBranchRefDto(int BranchId, string BranchName);

public sealed record FollowerWorkCardMetricsDto(
    double? PhantomRating,
    string? PhantomRatingLabel,
    double? PhantomRatingWeekAgo,
    string? ScoreTrend,
    int? RatedCustomerCount,
    int? RatedCustomerCountWeekAgo,
    string? CountTrend,
    string Availability);

public sealed record FollowerWorkFollowerCardDto(
    int FollowerId,
    string FollowerName,
    IReadOnlyList<FollowerWorkBranchRefDto> Branches,
    int AssignedListCount,
    FollowerWorkCardMetricsDto? Metrics);

public sealed record FollowerWorkFollowersResponse(
    IReadOnlyList<FollowerWorkFollowerCardDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    string TimeZone,
    string AsOfLocalDate,
    DateTimeOffset GeneratedAt);
