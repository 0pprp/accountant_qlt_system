using Application.FollowerWork.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.FollowerWork;

public class FollowerSaleAuthRulesTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(10, true)]
    [InlineData(0, false)]
    [InlineData(11, false)]
    public void Rating_bounds(int rating, bool ok)
        => Assert.Equal(ok, FollowerSaleAuthRules.IsValidRating(rating));

    [Theory]
    [InlineData(7, null, true)]
    [InlineData(7, "", true)]
    [InlineData(6, null, false)]
    [InlineData(6, "سبب", true)]
    public void Note_required_below_7(int rating, string? note, bool ok)
        => Assert.Equal(ok, FollowerSaleAuthRules.IsValidNoteForRating(rating, note));

    [Fact]
    public void Low_auth_risk_is_rating_le_6()
    {
        Assert.True(FollowerSaleAuthRules.IsLowAuthRisk(6));
        Assert.False(FollowerSaleAuthRules.IsLowAuthRisk(7));
        Assert.False(FollowerSaleAuthRules.IsLowAuthRisk(null));
    }

    [Theory]
    [InlineData("excellent", "excellent", true)]
    [InlineData("le100", "le100", true)]
    [InlineData("100k", "le100", true)]
    [InlineData("hack", "rated", false)]
    public void Bucket_allowlist(string input, string expected, bool ok)
    {
        var parsed = FollowerSaleAuthRules.TryNormalizeBucket(input, out var n);
        Assert.Equal(ok, parsed);
        Assert.Equal(expected, n);
    }

    [Fact]
    public void Shop_money_must_be_positive()
    {
        Assert.False(FollowerSaleAuthRules.IsValidShopMoney(0));
        Assert.True(FollowerSaleAuthRules.IsValidShopMoney(1));
    }

    [Fact]
    public void Authenticate_permission_is_not_read()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.SalesReviewRead,
            SeedPermissions.FollowerWork.SalesReviewAuthenticate);
    }
}
