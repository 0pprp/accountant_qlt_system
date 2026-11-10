namespace Application.SalesOverview.Common;

public sealed class SalesOverviewResponse
{
    public required SalesOverviewPeriodDto Period { get; init; }
    public required IReadOnlyList<SalesOverviewMetricDto> Metrics { get; init; }
    public required int? BranchId { get; init; }
    public required IReadOnlyList<int> BranchScope { get; init; }
}

public sealed class SalesOverviewPeriodDto
{
    /// <summary>يوم العمل ببغداد (قطع 03:00).</summary>
    public required string BusinessDateIraq { get; init; }
    public required string TimeZoneId { get; init; }
    public required DateTimeOffset WindowStartUtc { get; init; }
    public required DateTimeOffset WindowEndUtc { get; init; }
    public required DateTimeOffset GeneratedAtUtc { get; init; }
}

/// <summary>
/// مؤشر typed — عند Availability != Available لا تُعرض القيمة كصفر نشاط.
/// </summary>
public sealed class SalesOverviewMetricDto
{
    public required string Key { get; init; }
    public required string LabelAr { get; init; }
    public required string Unit { get; init; }
    public required string EntityOwner { get; init; }
    public required string Availability { get; init; }
    public int? Value { get; init; }
    public string? UnavailableReason { get; init; }
    public string? PeriodNote { get; init; }
    public SalesOverviewLinkDto? Link { get; init; }
}

public sealed class SalesOverviewLinkDto
{
    public required string RouteName { get; init; }
    public required string Path { get; init; }
    public required string RequiredPermission { get; init; }
    public required bool Allowed { get; init; }
    public IReadOnlyDictionary<string, string>? Query { get; init; }
}

public static class SalesOverviewMetricKeys
{
    public const string EmployeesOnShift = "employeesOnShift";
    public const string EmployeesOffShift = "employeesOffShift";
    public const string LiveLocations = "liveLocations";
    public const string SalesToday = "salesToday";
    public const string PendingSales = "pendingSales";
    public const string NewSalesRequests = "newSalesRequests";
}

public static class SalesOverviewAvailability
{
    public const string Available = "Available";
    public const string Unavailable = "Unavailable";
    public const string Forbidden = "Forbidden";
}
