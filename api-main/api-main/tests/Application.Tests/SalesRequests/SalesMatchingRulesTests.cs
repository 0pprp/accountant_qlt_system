using Domain.Services.SalesMatching;

namespace Application.Tests.SalesRequests;

public class SalesMatchingRulesTests
{
    [Fact]
    public void TripleName_matches_first_three_tokens_after_normalization()
    {
        Assert.True(SalesRequestNameSimilarity.IsTripleNameMatch(
            "حسين أحمد محمد",
            "حسين أحمد محمّد"));
        Assert.False(SalesRequestNameSimilarity.HasEnoughTripleName("حسين أحمد"));
        Assert.False(SalesRequestNameSimilarity.IsTripleNameMatch("حسين أحمد", "حسين أحمد محمد"));
    }

    [Fact]
    public void TripleName_ignores_trailing_numeric_token()
    {
        Assert.True(SalesRequestNameSimilarity.IsTripleNameMatch(
            "علي حسن كاظم 2",
            "علي حسن كاظم"));
    }

    [Fact]
    public void FatherGrandfather_requires_both_tokens()
    {
        Assert.True(SalesRequestNameSimilarity.IsFatherOrGrandfatherMatch(
            "زيد أحمد محمد",
            "سعد أحمد محمد"));
        Assert.False(SalesRequestNameSimilarity.IsFatherOrGrandfatherMatch(
            "زيد أحمد",
            "سعد أحمد محمد"));
    }

    [Fact]
    public void SonFather_works_both_directions()
    {
        Assert.True(SalesRequestNameSimilarity.IsSonFatherMatch(
            "حسين أحمد محمد",
            "أحمد محمد علي"));
        Assert.True(SalesRequestNameSimilarity.IsSonFatherMatch(
            "أحمد محمد علي",
            "حسين أحمد محمد"));
    }

    [Fact]
    public void Phone_normalizer_matches_iraqi_variants()
    {
        Assert.True(SalesPhoneNormalizer.Matches("07804924373", "9647804924373"));
        Assert.True(SalesPhoneNormalizer.HasEnoughPhoneData("07804924373"));
        Assert.False(SalesPhoneNormalizer.HasEnoughPhoneData("123"));
        Assert.False(SalesPhoneNormalizer.Matches("07804924373", "07901112233"));
    }

    [Fact]
    public void Normalize_does_not_mutate_concept_of_original_display()
    {
        const string original = "أَحْمَد محمّد";
        _ = SalesRequestNameSimilarity.Normalize(original);
        Assert.Equal("أَحْمَد محمّد", original);
    }
}
