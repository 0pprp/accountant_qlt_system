namespace Domain.Entities.ArchiveAggregate;

/// <summary>
/// عشرة الشهر التي تنتمي إليها المعاملة: ١-١٠، ١١-٢٠، ٢١-آخر الشهر.
/// دالّة صرفة بلا حالة ولا ساعة — الوقت يأتي وسيطاً دائماً.
/// </summary>
public readonly record struct ArchivePeriod(
    int Year,
    int Month,
    byte Index,
    DateOnly Start,
    DateOnly End)
{
    public const byte FirstIndex = 1;
    public const byte LastIndex = 3;

    public static ArchivePeriod For(DateOnly date)
    {
        var index = IndexFor(date.Day);
        var start = new DateOnly(date.Year, date.Month, StartDayFor(index));
        var end = new DateOnly(date.Year, date.Month, EndDayFor(date.Year, date.Month, index));
        return new ArchivePeriod(date.Year, date.Month, index, start, end);
    }

    public static ArchivePeriod For(int year, int month, byte index)
    {
        if (index < FirstIndex || index > LastIndex)
            throw new ArgumentOutOfRangeException(nameof(index), index, "رقم العشرة يجب أن يكون بين 1 و 3");

        return new ArchivePeriod(
            year,
            month,
            index,
            new DateOnly(year, month, StartDayFor(index)),
            new DateOnly(year, month, EndDayFor(year, month, index)));
    }

    /// <summary>العشرة انتهت حين يتجاوز يوم بغداد آخر أيامها — فاليوم الأخير يُحتسب كاملاً.</summary>
    public bool IsClosedOn(DateOnly baghdadToday) => baghdadToday > End;

    public bool Contains(DateOnly date) =>
        date.Year == Year && date.Month == Month && date >= Start && date <= End;

    private static byte IndexFor(int day) => day switch
    {
        <= 10 => 1,
        <= 20 => 2,
        _ => 3
    };

    private static int StartDayFor(byte index) => index switch
    {
        1 => 1,
        2 => 11,
        _ => 21
    };

    private static int EndDayFor(int year, int month, byte index) => index switch
    {
        1 => 10,
        2 => 20,
        _ => DateTime.DaysInMonth(year, month)
    };
}
