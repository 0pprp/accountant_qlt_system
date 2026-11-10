using Application.ManagementSummary.Common;

namespace Application.Tests.ManagementSummary;

public class ManagementSummaryRulesTests
{
    [Theory]
    [InlineData("receipts", ManagementSummaryMetric.Receipts)]
    [InlineData("expenses", ManagementSummaryMetric.Expenses)]
    [InlineData("salePrice", ManagementSummaryMetric.SalePrice)]
    [InlineData("purchasePrice", ManagementSummaryMetric.PurchasePrice)]
    [InlineData("buys", ManagementSummaryMetric.Buys)]
    [InlineData("SALEPRICE", ManagementSummaryMetric.SalePrice)]
    public void TryParse_accepts_api_keys(string raw, ManagementSummaryMetric expected)
    {
        Assert.True(ManagementSummaryMetricParser.TryParse(raw, out var metric));
        Assert.Equal(expected, metric);
    }

    [Theory]
    [InlineData("sales")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("revenue")]
    public void TryParse_rejects_unknown(string? raw)
    {
        Assert.False(ManagementSummaryMetricParser.TryParse(raw, out _));
    }

    [Fact]
    public void PurchasePrice_api_key_differs_from_Buys()
    {
        Assert.NotEqual(
            ManagementSummaryMetricParser.ToApiKey(ManagementSummaryMetric.PurchasePrice),
            ManagementSummaryMetricParser.ToApiKey(ManagementSummaryMetric.Buys));
    }

    [Theory]
    [InlineData(2024, 2, 29, true)]
    [InlineData(2023, 2, 29, false)]
    [InlineData(2026, 11, 30, true)]
    [InlineData(2026, 11, 31, false)]
    [InlineData(2026, 0, 1, false)]
    public void IsValidDay_respects_calendar(int year, int month, int day, bool ok)
    {
        Assert.Equal(ok, ManagementSummaryRules.IsValidDay(year, month, day));
    }

    [Fact]
    public void MonthBounds_are_half_open_in_Baghdad()
    {
        var (start, end) = IraqCalendar.MonthBoundsUtc(2026, 11);
        Assert.True(end > start);
        Assert.Equal(30, (end - start).TotalDays);

        var localStart = TimeZoneInfo.ConvertTime(start, IraqCalendar.TimeZone);
        Assert.Equal(1, localStart.Day);
        Assert.Equal(11, localStart.Month);
        Assert.Equal(0, localStart.Hour);
    }

    [Fact]
    public void DayBounds_midnight_edge_exclusive_end()
    {
        var day = new DateOnly(2026, 11, 8);
        var (start, end) = IraqCalendar.DayBoundsUtc(day);
        Assert.Equal(1, (end - start).TotalDays);

        var next = IraqCalendar.DayBoundsUtc(day.AddDays(1));
        Assert.Equal(end, next.StartInclusive);
    }

    [Fact]
    public void PageSize_is_clamped()
    {
        Assert.Equal(ManagementSummaryRules.DefaultPageSize, ManagementSummaryRules.ClampPageSize(null));
        Assert.Equal(ManagementSummaryRules.MaxPageSize, ManagementSummaryRules.ClampPageSize(500));
        Assert.Equal(10, ManagementSummaryRules.ClampPageSize(10));
    }
}
