namespace Application.Common.Utilities;

public static class DateOnlyExtensions
{
    public static DateOnly GetWeekStartDate(this DateOnly dt)
    {
        const DayOfWeek startOfWeek = DayOfWeek.Saturday;
        
        var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff);
    }
}