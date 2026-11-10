using Application.ManagementSummary.Common;
using Application.SalesOverview.Common;
using Domain.Entities.SalesLocationAggregate.Enums;

namespace Application.Tests.SalesEmployeeTracking;

public class SalesRouteDayBoundsTests
{
    [Fact]
    public void Route_day_bounds_use_baghdad_midnight_not_sales_cutoff()
    {
        var day = new DateOnly(2026, 11, 9);
        var (from, to) = IraqCalendar.DayBoundsUtc(day);

        Assert.Equal(new DateOnly(2026, 11, 9), IraqCalendar.ToLocalDate(from));
        Assert.Equal(new DateOnly(2026, 11, 10), IraqCalendar.ToLocalDate(to));

        // قطع بيع المبيعات 03:00 ≠ حدود مسار GPS التقويمية
        var work = SalesWorkDayIraq.BusinessDate(
            new DateTimeOffset(2026, 11, 8, 22, 30, 0, TimeSpan.Zero)); // 01:30 بغداد
        Assert.Equal(new DateOnly(2026, 11, 8), work);
        var workWindow = SalesWorkDayIraq.TodayWindowUtc(
            new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.Zero));
        Assert.NotEqual(from, workWindow.StartInclusive);
    }

    [Fact]
    public void SalesRoute_permission_string_is_independent()
    {
        Assert.Equal("SalesRoute.Read", Application.Common.SeedData.Permissions.SalesRoute.Read);
        Assert.NotEqual(
            Application.Common.SeedData.Permissions.SalesLocation.Read,
            Application.Common.SeedData.Permissions.SalesRoute.Read);
        Assert.NotEqual(
            Application.Common.SeedData.Permissions.SalesEmployee.Read,
            Application.Common.SeedData.Permissions.SalesRoute.Read);
    }

    [Fact]
    public void ActorKind_sales_employee_isolated_from_follower()
    {
        Assert.NotEqual(SalesLocationActorKind.SalesEmployee, SalesLocationActorKind.Follower);
    }
}
