using Application.ManagementSummary.Common;

namespace Application.SalesOverview.Common;

/// <summary>
/// يوم العمل ببغداد بقطع 03:00 — مطابق IraqTimeService.CutoffTime في المصدر.
/// </summary>
public static class SalesWorkDayIraq
{
    public static readonly TimeSpan CutoffTime = TimeSpan.FromHours(3);
    public const string TimeZoneId = "Asia/Baghdad";

    public static DateOnly BusinessDate(DateTimeOffset utcNow)
    {
        var local = TimeZoneInfo.ConvertTime(utcNow, IraqCalendar.TimeZone);
        var localDt = local.DateTime;
        if (localDt.TimeOfDay < CutoffTime)
            return DateOnly.FromDateTime(localDt.Date.AddDays(-1));
        return DateOnly.FromDateTime(localDt.Date);
    }

    /// <summary>بداية شاملة لنافذة «اليوم» حتى الآن (UTC).</summary>
    public static (DateTimeOffset StartInclusive, DateTimeOffset EndExclusive) TodayWindowUtc(DateTimeOffset utcNow)
    {
        var business = BusinessDate(utcNow);
        var startLocal = business.ToDateTime(TimeOnly.FromTimeSpan(CutoffTime));
        var start = new DateTimeOffset(startLocal, IraqCalendar.TimeZone.GetUtcOffset(startLocal)).ToUniversalTime();
        return (start, utcNow);
    }
}
