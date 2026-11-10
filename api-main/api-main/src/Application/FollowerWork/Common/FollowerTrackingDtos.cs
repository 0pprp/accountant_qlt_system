namespace Application.FollowerWork.Common;

public sealed record FollowerLiveLocationRowDto(
    int FollowerId,
    string FollowerName,
    int? BranchId,
    string? BranchName,
    int? ShiftId,
    bool HasActiveShift,
    double? Latitude,
    double? Longitude,
    double? AccuracyMeters,
    DateTimeOffset? CapturedAtUtc,
    DateTimeOffset? ReceivedAtUtc,
    string LocationStatus);

public sealed record FollowerLiveLocationsResponse(
    IReadOnlyList<FollowerLiveLocationRowDto> Items,
    string TimeZoneId,
    DateTimeOffset GeneratedAtUtc,
    int LiveThresholdSeconds,
    int StaleThresholdMinutes);

public sealed record FollowerRoutePointDto(
    int PointId,
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    DateTimeOffset CapturedAtUtc,
    DateTimeOffset ReceivedAtUtc,
    bool IsLive,
    int? ShiftId);

public sealed record FollowerRouteResponse(
    int FollowerId,
    string? FollowerName,
    DateOnly DateIraq,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    IReadOnlyList<FollowerRoutePointDto> Points,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool Truncated,
    string TimeZoneId,
    DateTimeOffset GeneratedAtUtc);
