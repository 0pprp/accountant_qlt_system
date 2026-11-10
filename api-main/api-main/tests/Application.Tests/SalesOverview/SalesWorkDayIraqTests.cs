using Application.SalesOverview.Common;

namespace Application.Tests.SalesOverview;

public class SalesWorkDayIraqTests
{
    [Fact]
    public void BusinessDate_before_cutoff_uses_previous_calendar_day()
    {
        // 2026-11-09 01:30 Baghdad = 2026-11-08 22:30 UTC
        var utc = new DateTimeOffset(2026, 11, 8, 22, 30, 0, TimeSpan.Zero);
        Assert.Equal(new DateOnly(2026, 11, 8), SalesWorkDayIraq.BusinessDate(utc));
    }

    [Fact]
    public void BusinessDate_after_cutoff_uses_current_calendar_day()
    {
        // 2026-11-09 04:00 Baghdad = 2026-11-09 01:00 UTC
        var utc = new DateTimeOffset(2026, 11, 9, 1, 0, 0, TimeSpan.Zero);
        Assert.Equal(new DateOnly(2026, 11, 9), SalesWorkDayIraq.BusinessDate(utc));
    }

    [Fact]
    public void TodayWindow_starts_at_baghdad_cutoff()
    {
        var utc = new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.Zero);
        var (start, end) = SalesWorkDayIraq.TodayWindowUtc(utc);
        Assert.Equal(utc, end);
        Assert.True(start < end);
        Assert.Equal(new DateOnly(2026, 11, 9), SalesWorkDayIraq.BusinessDate(utc));
    }
}
