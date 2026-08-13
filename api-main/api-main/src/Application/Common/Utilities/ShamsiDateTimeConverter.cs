using System.Globalization;

namespace Application.Common.Utilities;

public static class ShamsiDateTimeConverter
{
    private static readonly PersianCalendar PersianCalendar = new();
    public static DateOnly ConvertFromShamsiToMiladiDateOnly(string value)
    {
        var dateParts = value.Split('/');

        if (dateParts.Length != 3)
        {
            throw new FormatException($"invalid shamsi date | value : {value} | require format : YYYY/MM/DD");
        }

        var year = short.Parse(dateParts[0]);
        var month = byte.Parse(dateParts[1]);
        var day = byte.Parse(dateParts[2]);
        var date = PersianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0, 0);

        return DateOnly.FromDateTime(date);
    }
    public static string ConvertFromMiladiToShamsiDateString(DateOnly date)
        => ConvertFromMiladiToShamsiDateString(date.ToDateTime(TimeOnly.MinValue));
    public static string ConvertFromMiladiToShamsiDateString(DateTime dateTime)
    {
        var year = PersianCalendar.GetYear(dateTime);
        var month = PersianCalendar.GetMonth(dateTime);
        var day = PersianCalendar.GetDayOfMonth(dateTime);

        return $"{year}/{month:D2}/{day:D2}";
    }
}
