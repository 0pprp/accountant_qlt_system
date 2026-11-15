using Application.ManagementSalesMap.Common;
using Domain.Entities.OrderAggregate;

namespace Application.Tests.ManagementSalesMap;

public class SalesMapRulesTests
{
    [Fact]
    public void Geo_Rejects_Null_And_Zero()
    {
        Assert.False(SalesMapGeo.TryGetValid(null, out _, out _));
        Assert.False(SalesMapGeo.TryGetValid(new Location(0, 0), out _, out _));
        Assert.True(SalesMapGeo.TryGetValid(new Location(33.3, 44.4), out var lat, out var lng));
        Assert.Equal(33.3, lat);
        Assert.Equal(44.4, lng);
    }

    [Fact]
    public void Letters_Stable_By_Index()
    {
        Assert.Equal("أ", SalesMapListLetters.LetterAt(0));
        Assert.Equal("ب", SalesMapListLetters.LetterAt(1));
        var map = SalesMapListLetters.Assign([10, 5, 7]);
        Assert.Equal("أ", map[10]);
        Assert.Equal("ب", map[5]);
        Assert.Equal("ت", map[7]);
        // إعادة التعيين بنفس الترتيب → نفس الحروف
        var again = SalesMapListLetters.Assign([10, 5, 7]);
        Assert.Equal(map[10], again[10]);
        Assert.Equal(map[5], again[5]);
    }

    [Fact]
    public void Status_Legal_Overrides_Active()
    {
        var asOf = new DateOnly(2026, 11, 15);
        var (code, label) = SalesMapStatusRules.Classify(
            isLegal: true,
            lastPaymentDate: asOf,
            saleDate: asOf.AddDays(-10),
            asOf: asOf);
        Assert.Equal(3, code);
        Assert.Equal(SalesMapStatusRules.LabelLegal, label);
        Assert.False(SalesMapStatusRules.MatchesFilter(code, SalesMapStatusFilter.Continuous));
        Assert.False(SalesMapStatusRules.MatchesFilter(code, SalesMapStatusFilter.Stopped));
        Assert.True(SalesMapStatusRules.MatchesFilter(code, SalesMapStatusFilter.Legal));
        Assert.True(SalesMapStatusRules.MatchesFilter(code, SalesMapStatusFilter.All));
    }

    [Fact]
    public void Status_Continuous_Within_30_Days()
    {
        var asOf = new DateOnly(2026, 11, 15);
        var (code, _) = SalesMapStatusRules.Classify(false, asOf.AddDays(-10), asOf.AddDays(-40), asOf);
        Assert.Equal(1, code);
    }

    [Fact]
    public void Status_Stopped_When_Old_Payment()
    {
        var asOf = new DateOnly(2026, 11, 15);
        var (code, _) = SalesMapStatusRules.Classify(false, asOf.AddDays(-60), asOf.AddDays(-100), asOf);
        Assert.Equal(2, code);
    }

    [Fact]
    public void Status_Unknown_When_No_Dates()
    {
        var asOf = new DateOnly(2026, 11, 15);
        var (code, label) = SalesMapStatusRules.Classify(false, null, null, asOf);
        Assert.Null(code);
        Assert.Equal(SalesMapStatusRules.LabelUnknown, label);
        Assert.True(SalesMapStatusRules.MatchesFilter(code, SalesMapStatusFilter.All));
        Assert.False(SalesMapStatusRules.MatchesFilter(code, SalesMapStatusFilter.Continuous));
    }
}
