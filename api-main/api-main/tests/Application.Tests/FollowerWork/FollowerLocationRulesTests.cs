using Application.FollowerWork.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.FollowerWork;

public class FollowerLocationRulesTests
{
    [Fact]
    public void Location_read_does_not_grant_route_read()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.LocationRead,
            SeedPermissions.FollowerWork.RouteRead);
    }

    [Theory]
    [InlineData(91, 44, false)]
    [InlineData(32, 181, false)]
    [InlineData(32, 44, true)]
    [InlineData(double.NaN, 44, false)]
    public void Coordinate_bounds(double lat, double lng, bool ok) =>
        Assert.Equal(ok, FollowerLocationRules.IsValidCoordinate(lat, lng));

    [Fact]
    public void Older_capture_does_not_replace_latest()
    {
        var newer = DateTimeOffset.Parse("2026-11-08T12:00:00Z");
        var older = DateTimeOffset.Parse("2026-11-08T11:00:00Z");
        Assert.False(FollowerLocationRules.ShouldReplaceLatest(newer, older));
        Assert.True(FollowerLocationRules.ShouldReplaceLatest(older, newer));
        Assert.True(FollowerLocationRules.ShouldReplaceLatest(null, newer));
    }

    [Fact]
    public void Status_live_within_150_seconds()
    {
        var now = DateTimeOffset.Parse("2026-11-08T12:00:00Z");
        var captured = now.AddSeconds(-100);
        Assert.Equal(
            FollowerLocationStatuses.Live,
            FollowerLocationRules.ResolveStatus(true, captured, now));
    }

    [Fact]
    public void Status_stale_after_threshold()
    {
        var now = DateTimeOffset.Parse("2026-11-08T12:00:00Z");
        var captured = now.AddMinutes(-10);
        Assert.Equal(
            FollowerLocationStatuses.Stale,
            FollowerLocationRules.ResolveStatus(true, captured, now));
    }

    [Fact]
    public void No_shift_without_active_duty()
    {
        Assert.Equal(
            FollowerLocationStatuses.NoShift,
            FollowerLocationRules.ResolveStatus(false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void No_location_when_shift_but_no_point()
    {
        Assert.Equal(
            FollowerLocationStatuses.NoLocation,
            FollowerLocationRules.ResolveStatus(true, null, DateTimeOffset.UtcNow));
    }
}
