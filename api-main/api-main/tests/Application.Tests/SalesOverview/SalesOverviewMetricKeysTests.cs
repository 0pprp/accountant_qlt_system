using Application.SalesOverview.Common;

namespace Application.Tests.SalesOverview;

public class SalesOverviewMetricKeysTests
{
    [Fact]
    public void Six_source_metrics_are_defined_without_invented_keys()
    {
        var keys = new[]
        {
            SalesOverviewMetricKeys.EmployeesOnShift,
            SalesOverviewMetricKeys.EmployeesOffShift,
            SalesOverviewMetricKeys.LiveLocations,
            SalesOverviewMetricKeys.SalesToday,
            SalesOverviewMetricKeys.PendingSales,
            SalesOverviewMetricKeys.NewSalesRequests,
        };
        Assert.Equal(6, keys.Distinct().Count());
        Assert.DoesNotContain("completedRequestsAsSales", keys);
    }
}
