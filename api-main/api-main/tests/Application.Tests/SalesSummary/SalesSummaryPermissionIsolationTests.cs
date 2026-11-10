using Application.ManagementSummary.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesSummary;

public class SalesSummaryPermissionIsolationTests
{
    [Fact]
    public void SalesSummary_permissions_are_independent_from_management_summary()
    {
        Assert.Equal("SalesSummary.Read", SeedPermissions.SalesSummary.Read);
        Assert.Equal("SalesSummary.Drilldown", SeedPermissions.SalesSummary.Drilldown);
        Assert.NotEqual(SeedPermissions.SalesSummary.Read, SeedPermissions.ManagementSummary.Read);
        Assert.NotEqual(SeedPermissions.SalesSummary.Drilldown, SeedPermissions.ManagementSummary.Drilldown);
    }

    [Fact]
    public void Sale_price_metric_label_matches_management_rule()
    {
        Assert.Equal("سعر البيع", ManagementSummaryRules.MetricLabel(ManagementSummaryMetric.SalePrice));
        Assert.True(ManagementSummaryRules.IsValidPeriod(2026, 11));
        Assert.False(ManagementSummaryRules.IsValidDay(2026, 11, 31));
    }
}
