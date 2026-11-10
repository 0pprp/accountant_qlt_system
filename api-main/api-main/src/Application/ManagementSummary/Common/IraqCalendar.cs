namespace Application.ManagementSummary.Common;

public static class IraqCalendar
{
    private static readonly TimeZoneInfo Tz = Resolve();

    public static TimeZoneInfo TimeZone => Tz;

    public static DateOnly ToLocalDate(DateTimeOffset utcOrOffset)
    {
        var local = TimeZoneInfo.ConvertTime(utcOrOffset, Tz);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public static DateOnly ToLocalDate(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
            : utcDateTime.ToUniversalTime();
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, Tz);
        return DateOnly.FromDateTime(local);
    }

    /// <summary>بداية شاملة / نهاية غير شاملة لليوم المحلي ببغداد كـ UTC offset.</summary>
    public static (DateTimeOffset StartInclusive, DateTimeOffset EndExclusive) DayBoundsUtc(DateOnly localDay)
    {
        var startLocal = localDay.ToDateTime(TimeOnly.MinValue);
        var endLocal = localDay.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var start = new DateTimeOffset(startLocal, Tz.GetUtcOffset(startLocal));
        var end = new DateTimeOffset(endLocal, Tz.GetUtcOffset(endLocal));
        return (start.ToUniversalTime(), end.ToUniversalTime());
    }

    public static (DateTimeOffset StartInclusive, DateTimeOffset EndExclusive) MonthBoundsUtc(int year, int month)
    {
        var startLocal = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var endLocal = startLocal.AddMonths(1);
        var start = new DateTimeOffset(startLocal, Tz.GetUtcOffset(startLocal));
        var end = new DateTimeOffset(endLocal, Tz.GetUtcOffset(endLocal));
        return (start.ToUniversalTime(), end.ToUniversalTime());
    }

    public static int DaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

    private static TimeZoneInfo Resolve()
    {
        foreach (var id in new[] { "Asia/Baghdad", "Arabic Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("Iraq(+03)", TimeSpan.FromHours(3), "Iraq", "Iraq");
    }
}
