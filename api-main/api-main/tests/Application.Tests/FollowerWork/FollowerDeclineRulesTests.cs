using Application.FollowerWork.Common;

namespace Application.Tests.FollowerWork;

public class FollowerDeclineRulesTests
{
    [Theory]
    [InlineData("ممتاز", 10, "ضعيف", 0, 5, true)]
    [InlineData("جيد", 5, "متلكئ", -5, 7, true)]
    [InlineData("ضعيف", 0, "سيء", -10, 1, true)]
    [InlineData("ممتاز", 10, "ضعيف", 0, 8, false)]
    [InlineData("ممتاز", 10, "جيد", 5, 3, false)]
    [InlineData("جيد", 5, "سيء", -10, 2, false)]
    [InlineData("ممتاز", 10, "متلكئ", -5, 2, false)]
    [InlineData("ضعيف", 0, "متلكئ", -5, 3, false)]
    public void Declining_requires_ten_point_drop_within_seven_days(
        string from, int fromScore, string to, int toScore, int days, bool expected)
        => Assert.Equal(
            expected,
            FollowerDeclineRules.IsDeclining(from, fromScore, to, toScore, days));

    [Theory]
    [InlineData("ممتاز - معدل التسديد 1%", "ممتاز")]
    [InlineData("متلكئ", "متلكئ")]
    [InlineData("مرفوض", "متلكئ")]
    [InlineData("سيئ", "سيء")]
    public void Normalize_band(string input, string expected)
        => Assert.Equal(expected, FollowerDeclineRules.NormalizeBand(input));

    [Fact]
    public void Boundary_day_7_included_day_8_excluded()
    {
        Assert.True(FollowerDeclineRules.IsDeclining("ممتاز", 10, "ضعيف", 0, 7));
        Assert.False(FollowerDeclineRules.IsDeclining("ممتاز", 10, "ضعيف", 0, 8));
    }

    [Fact]
    public void Score_drop_below_10_rejected_even_on_target_labels()
    {
        // انتقال صحيح لكن فرق أقل من 10 (غير واقعي للدرجات القياسية؛ حماية القاعدة)
        Assert.False(FollowerDeclineRules.IsDeclining("ممتاز", 10, "ضعيف", 5, 2));
    }

    [Fact]
    public void Unchanged_or_improved_not_declining()
    {
        Assert.False(FollowerDeclineRules.IsDeclining("ضعيف", 0, "ضعيف", 0, 1));
        Assert.False(FollowerDeclineRules.IsDeclining("ضعيف", 0, "ممتاز", 10, 1));
    }

    [Fact]
    public void Lookback_and_drop_constants_match_source()
    {
        Assert.Equal(7, FollowerDeclineRules.LookbackDays);
        Assert.Equal(10, FollowerDeclineRules.MinScoreDrop);
    }

    [Fact]
    public void Declining_read_permission_is_section_scoped()
    {
        Assert.Equal("FollowerDeclining.Read", Application.Common.SeedData.Permissions.FollowerWork.DecliningRead);
        Assert.NotEqual(
            Application.Common.SeedData.Permissions.FollowerWork.SalesReviewRead,
            Application.Common.SeedData.Permissions.FollowerWork.DecliningRead);
    }
}
