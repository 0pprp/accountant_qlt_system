using System.Security.Claims;
using Application.Common.Models.TokenFactory;
using Application.FollowerWork.Common;
using Application.Tests;

namespace Application.Tests.FollowerWork;

public class FollowerWorkRulesTests
{
    [Theory]
    [InlineData(null, "name", true)]
    [InlineData("name", "name", true)]
    [InlineData("rating", "rating", true)]
    [InlineData("lists", "lists", true)]
    [InlineData("hack", "name", false)]
    public void Sort_allowlist(string? input, string expected, bool ok)
    {
        var parsed = FollowerWorkRules.TryParseSort(input, out var sort);
        Assert.Equal(ok, parsed);
        Assert.Equal(expected, sort);
    }

    [Theory]
    [InlineData(5.0, 4.0, "up")]
    [InlineData(4.0, 5.0, "down")]
    [InlineData(5.0, 5.0, "flat")]
    [InlineData(5.0, null, "flat")]
    public void ScoreTrend_matches_source_bands(double today, double? prior, string expected)
    {
        Assert.Equal(expected, FollowerWorkRules.ComputeScoreTrend(today, prior));
    }

    [Fact]
    public void Directory_access_or_section_without_parent()
    {
        var parentOnly = UserWith("FollowerWork.Read");
        Assert.True(FollowerWorkAccess.CanReadDirectory(parentOnly));
        Assert.True(FollowerWorkAccess.CanReadCardMetrics(parentOnly));

        var sectionOnly = UserWith("FollowerSalesReview.Read");
        Assert.True(FollowerWorkAccess.CanReadDirectory(sectionOnly));
        Assert.False(FollowerWorkAccess.CanReadCardMetrics(sectionOnly));

        var none = UserWith("Customer.Read");
        Assert.False(FollowerWorkAccess.CanReadDirectory(none));
    }

    [Fact]
    public void Branch_scope_does_not_expand()
    {
        var user = new CurrentUserServiceStub { BranchIds = [10, 20] };
        Assert.Equal([10, 20], FollowerWorkAccess.ResolveBranchScope(user, null));
        Assert.Equal([10], FollowerWorkAccess.ResolveBranchScope(user, 10));
        Assert.Empty(FollowerWorkAccess.ResolveBranchScope(user, 99));
        Assert.NotNull(FollowerWorkAccess.ValidateBranchOrAll(user, 99));
        Assert.Null(FollowerWorkAccess.ValidateBranchOrAll(user, 10));
    }

    [Fact]
    public void Response_dto_has_no_sensitive_account_fields()
    {
        var props = typeof(FollowerWorkFollowerCardDto).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.DoesNotContain("Password", props);
        Assert.DoesNotContain("Token", props);
        Assert.DoesNotContain("RefreshToken", props);
        Assert.DoesNotContain("SecurityStamp", props);
        Assert.DoesNotContain("Email", props);
        Assert.DoesNotContain("PhoneNumber", props);
        Assert.Contains("FollowerId", props);
        Assert.Contains("FollowerName", props);
        Assert.Contains("AssignedListCount", props);
    }

    [Fact]
    public void Card_metrics_omit_section_counters()
    {
        var props = typeof(FollowerWorkCardMetricsDto).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.DoesNotContain("PendingSalesCount", props);
        Assert.DoesNotContain("NotesCount", props);
        Assert.DoesNotContain("CollectionCount", props);
        Assert.Contains("Availability", props);
        Assert.Contains("PhantomRating", props);
    }

    private static CurrentUserServiceStub UserWith(string permission)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(CustomClaims.Permissions, permission)
        ], "test");
        return new CurrentUserServiceStub
        {
            User = new ClaimsPrincipal(identity),
            BranchIds = [1]
        };
    }
}
