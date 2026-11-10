namespace Application.SalesEmployeeTracking.Common;

public sealed class SalesEmployeeRouteResponse
{
    public required int EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required DateOnly DateIraq { get; init; }
    public required DateTimeOffset FromUtc { get; init; }
    public required DateTimeOffset ToUtc { get; init; }
    public required IReadOnlyList<SalesEmployeeRoutePointDto> Points { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    public required bool Truncated { get; init; }
    public required string TimeZoneId { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
    /// <summary>فجوات >30 دقيقة بين نقاط متتالية (إعلان فقط — بلا خط وهمي).</summary>
    public required IReadOnlyList<SalesEmployeeRouteGapDto> Gaps { get; init; }
    public required string DayBoundNote { get; init; }
}

public sealed class SalesEmployeeRoutePointDto
{
    public required int PointId { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public double? AccuracyMeters { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public required DateTimeOffset ReceivedAtUtc { get; init; }
    public required bool IsLive { get; init; }
    public int? ShiftId { get; init; }
    public int? BranchId { get; init; }
}

public sealed class SalesEmployeeRouteGapDto
{
    public required int AfterPointId { get; init; }
    public required int BeforePointId { get; init; }
    public required int GapMinutes { get; init; }
}

public sealed class SalesEmployeeLiveLocationsResponse
{
    public required IReadOnlyList<SalesEmployeeLiveLocationRowDto> Items { get; init; }
    public required string TimeZoneId { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
    public required int LiveThresholdSeconds { get; init; }
    public required int StaleThresholdMinutes { get; init; }
}

public sealed class SalesEmployeeLiveLocationRowDto
{
    public required int EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required int BranchId { get; init; }
    public required string BranchName { get; init; }
    public required string ShiftStatus { get; init; }
    public required string LocationStatus { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? AccuracyMeters { get; init; }
    public DateTimeOffset? CapturedAtUtc { get; init; }
    public DateTimeOffset? ReceivedAtUtc { get; init; }
    public bool HasCoordinates { get; init; }
}
