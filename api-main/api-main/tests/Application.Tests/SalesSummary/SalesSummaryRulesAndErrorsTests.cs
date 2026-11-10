using Application.ManagementSummary.Common;
using Application.SalesSummary.Common;

namespace Application.Tests.SalesSummary;

public class SalesSummaryRulesAndErrorsTests
{
    [Theory]
    [InlineData(2026, 2, 28, true)]
    [InlineData(2026, 2, 29, false)]
    [InlineData(2024, 2, 29, true)]
    [InlineData(2026, 11, 30, true)]
    [InlineData(2026, 11, 31, false)]
    [InlineData(1999, 1, 1, false)]
    public void Day_bounds_follow_baghdad_calendar_month(int year, int month, int day, bool expected)
    {
        Assert.Equal(expected, ManagementSummaryRules.IsValidDay(year, month, day));
    }

    [Fact]
    public void Period_rejects_out_of_range_month()
    {
        Assert.False(ManagementSummaryRules.IsValidPeriod(2026, 0));
        Assert.False(ManagementSummaryRules.IsValidPeriod(2026, 13));
        Assert.True(ManagementSummaryRules.IsValidPeriod(2026, 11));
    }

    [Fact]
    public void Error_codes_are_stable_and_distinct()
    {
        Assert.Equal("SalesSummary.ReadDenied", SalesSummaryErrors.ReadDenied.Code);
        Assert.Equal("SalesSummary.DrilldownDenied", SalesSummaryErrors.DrilldownDenied.Code);
        Assert.Equal("SalesSummary.MetricRejected", SalesSummaryErrors.MetricRejected.Code);
        Assert.Equal("SalesSummary.InvalidDay", SalesSummaryErrors.InvalidDay.Code);
        Assert.NotEqual(SalesSummaryErrors.ReadDenied.Code, SalesSummaryErrors.DrilldownDenied.Code);
    }

    [Fact]
    public void Page_size_is_clamped_for_details()
    {
        Assert.Equal(25, ManagementSummaryRules.ClampPageSize(null));
        Assert.Equal(100, ManagementSummaryRules.ClampPageSize(500));
        Assert.Equal(1, ManagementSummaryRules.ClampPage(0));
    }
}
