using Domain.Entities.ArchiveAggregate;

namespace Application.Tests.Archive;

/// <summary>
/// مجال الإدخال هنا منتهٍ وصغير، فالفحص الشامل على سنوات كاملة أقوى من عيّنة عشوائية
/// ولا يحتاج مكتبة خاصّيات. يغطّي المدى سنوات كبيسة وشباط بـ٢٨ و٢٩ يوماً.
/// </summary>
public class ArchivePeriodTests
{
    private static IEnumerable<DateOnly> EveryDay()
    {
        for (var year = 2023; year <= 2028; year++)
            for (var month = 1; month <= 12; month++)
                for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                    yield return new DateOnly(year, month, day);
    }

    [Fact]
    public void Every_day_falls_in_exactly_one_period()
    {
        foreach (var date in EveryDay())
        {
            var period = ArchivePeriod.For(date);

            Assert.True(period.Contains(date), $"{date:yyyy-MM-dd} لا تقع في عشرتها");

            var others = new[] { (byte)1, (byte)2, (byte)3 }
                .Where(i => i != period.Index)
                .Select(i => ArchivePeriod.For(date.Year, date.Month, i));

            foreach (var other in others)
                Assert.False(other.Contains(date), $"{date:yyyy-MM-dd} تقع في عشرتين");
        }
    }

    [Fact]
    public void The_three_periods_cover_the_month_without_gap_or_overlap()
    {
        for (var year = 2023; year <= 2028; year++)
            for (var month = 1; month <= 12; month++)
            {
                var first = ArchivePeriod.For(year, month, 1);
                var second = ArchivePeriod.For(year, month, 2);
                var third = ArchivePeriod.For(year, month, 3);

                Assert.Equal(new DateOnly(year, month, 1), first.Start);
                Assert.Equal(first.End.AddDays(1), second.Start);
                Assert.Equal(second.End.AddDays(1), third.Start);
                Assert.Equal(new DateOnly(year, month, DateTime.DaysInMonth(year, month)), third.End);
            }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 1)]
    [InlineData(11, 2)]
    [InlineData(20, 2)]
    [InlineData(21, 3)]
    [InlineData(28, 3)]
    public void Boundary_days_land_in_the_documented_period(int day, byte expectedIndex)
    {
        Assert.Equal(expectedIndex, ArchivePeriod.For(new DateOnly(2026, 2, day)).Index);
    }

    [Fact]
    public void Third_period_absorbs_the_month_tail()
    {
        Assert.Equal(new DateOnly(2026, 2, 28), ArchivePeriod.For(2026, 2, 3).End);
        Assert.Equal(new DateOnly(2028, 2, 29), ArchivePeriod.For(2028, 2, 3).End);
        Assert.Equal(new DateOnly(2026, 1, 31), ArchivePeriod.For(2026, 1, 3).End);
        Assert.Equal(new DateOnly(2026, 4, 30), ArchivePeriod.For(2026, 4, 3).End);
    }

    [Fact]
    public void Period_closes_only_after_its_last_day_has_passed()
    {
        var period = ArchivePeriod.For(2026, 11, 1);

        Assert.False(period.IsClosedOn(new DateOnly(2026, 11, 9)));
        Assert.False(period.IsClosedOn(new DateOnly(2026, 11, 10)));
        Assert.True(period.IsClosedOn(new DateOnly(2026, 11, 11)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Invalid_period_index_is_rejected(byte index)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ArchivePeriod.For(2026, 11, index));
    }
}
