namespace Application.FollowerWork.Common;

public static class FollowerLocationStatuses
{
    public const string Live = "Live";
    public const string Stale = "Stale";
    public const string NoLocation = "NoLocation";
    public const string NoShift = "NoShift";
}

public static class FollowerLocationRules
{
    public static readonly TimeSpan LiveThreshold = TimeSpan.FromSeconds(150);
    public static readonly TimeSpan StaleThreshold = TimeSpan.FromMinutes(3);
    public const int MaxBatchPoints = 500;
    public const int DefaultRoutePageSize = 500;
    public const int MaxRoutePageSize = 2000;
    public static readonly TimeSpan MaxFutureSkew = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan MaxPastBeforeShift = TimeSpan.FromMinutes(2);

    public static string ResolveStatus(bool hasActiveShift, DateTimeOffset? capturedAtUtc, DateTimeOffset utcNow)
    {
        if (hasActiveShift == false)
            return FollowerLocationStatuses.NoShift;
        if (capturedAtUtc is null)
            return FollowerLocationStatuses.NoLocation;

        var age = utcNow - capturedAtUtc.Value;
        if (age <= LiveThreshold)
            return FollowerLocationStatuses.Live;
        return FollowerLocationStatuses.Stale;
    }

    public static bool IsValidCoordinate(double lat, double lng) =>
        lat is >= -90 and <= 90
        && lng is >= -180 and <= 180
        && double.IsFinite(lat)
        && double.IsFinite(lng);

    /// <summary>نقطة قديمة متأخرة لا تستبدل آخر موقع أحدث التقاطًا.</summary>
    public static bool ShouldReplaceLatest(DateTimeOffset? currentCaptured, DateTimeOffset incomingCaptured) =>
        currentCaptured is null || incomingCaptured > currentCaptured.Value;
}
