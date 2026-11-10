using Application.DelegateFollowup.Common;

namespace Application.Tests.DelegateFollowup;

public class DelegateFollowupRulesTests
{
    [Fact]
    public void IsListActive_Within40Days_True()
    {
        var asOf = new DateOnly(2026, 11, 8);
        Assert.True(DelegateFollowupRules.IsListActive(asOf.AddDays(-40), asOf));
        Assert.False(DelegateFollowupRules.IsListActive(asOf.AddDays(-41), asOf));
        Assert.False(DelegateFollowupRules.IsListActive(null, asOf));
    }

    [Fact]
    public void BoardOperatorScore_Bounds()
    {
        Assert.True(DelegateFollowupRules.IsValidBoardOperatorScore(0));
        Assert.True(DelegateFollowupRules.IsValidBoardOperatorScore(10));
        Assert.False(DelegateFollowupRules.IsValidBoardOperatorScore(-1));
        Assert.False(DelegateFollowupRules.IsValidBoardOperatorScore(11));
    }

    [Fact]
    public void RemainingBands_MatchSource()
    {
        var bands = DelegateFollowupRules.CountRemainingBands([50_000, 100_000, 150_000, 200_000, 0, -1]);
        Assert.Equal(1, bands.Lt100k);
        Assert.Equal(2, bands.From100To200k);
        Assert.Equal(3, bands.Ge100k);
    }

    [Fact]
    public void LastFridayBefore_SkipsTodayIfFriday()
    {
        var friday = new DateOnly(2026, 11, 6);
        Assert.Equal(new DateOnly(2026, 10, 30), DelegateFollowupRules.LastFridayBefore(friday));
        var saturday = new DateOnly(2026, 11, 7);
        Assert.Equal(friday, DelegateFollowupRules.LastFridayBefore(saturday));
    }

    [Fact]
    public void RequiredDue_ExcludesLegalWhenFlagKnown()
    {
        var sum = DelegateFollowupRules.SumRequiredDueInstallment(
        [
            (100, 500, false),
            (50, 200, true),
            (20, 0, false),
        ]);
        Assert.Equal(100, sum);
    }

    [Fact]
    public void ConversionCounts_ImprovedDeclined()
    {
        var c = DelegateFollowupRules.CountConversions(
        [
            ("سيء", "ممتاز"),
            ("ممتاز", "ضعيف"),
            ("جيد", "جيد"),
            ("قانونية", "ممتاز"),
        ]);
        Assert.Equal(1, c.Improved);
        Assert.Equal(1, c.Declined);
        Assert.Equal(2, c.Unchanged);
    }

    [Fact]
    public void NewSaleUnpaid_ExcludesLegal()
    {
        var asOf = new DateOnly(2026, 11, 8);
        Assert.False(DelegateFollowupRules.IsNewSaleUnpaid3Days(
            100, 500, 0, 0, asOf.AddDays(-5), asOf, isLegal: true));
        Assert.True(DelegateFollowupRules.IsNewSaleUnpaid3Days(
            100, 500, 0, 0, asOf.AddDays(-5), asOf, isLegal: false));
    }

    [Fact]
    public void PickLatestDayRating_PrefersToday()
    {
        var asOf = new DateOnly(2026, 11, 8);
        var rows = new[]
        {
            new { Date = asOf.AddDays(-2), Score = 3 },
            new { Date = asOf, Score = 8 },
            new { Date = asOf.AddDays(-1), Score = 5 },
        };
        var pick = DelegateFollowupRules.PickLatestDayRating(rows, r => r.Date, asOf);
        Assert.NotNull(pick);
        Assert.Equal(8, pick!.Score);
    }
}
