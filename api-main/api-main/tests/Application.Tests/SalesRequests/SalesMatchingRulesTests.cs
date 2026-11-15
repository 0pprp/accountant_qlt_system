using Application.SalesRequests.Common;
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

    [Fact]
    public void SignificantPayment_green_at_25_percent_same_day_sum()
    {
        var day = new DateOnly(2026, 11, 10);
        var info = SalesMatchSignificantPayment.Evaluate(
            2_000_000,
            [(day, 300_000), (day, 200_000)]);
        Assert.NotNull(info);
        Assert.Equal(SalesMatchPaymentShareBand.Green, info!.Band);
        Assert.Equal(500_000, info.Amount);
        Assert.Equal(day, info.Date);
    }

    [Fact]
    public void SignificantPayment_orange_between_20_and_25()
    {
        var day = new DateOnly(2026, 11, 5);
        var info = SalesMatchSignificantPayment.Evaluate(
            2_700_000,
            [(day, 570_000)]);
        Assert.NotNull(info);
        Assert.Equal(SalesMatchPaymentShareBand.Orange, info!.Band);
    }

    [Fact]
    public void SignificantPayment_rejects_below_20_percent()
    {
        var day = new DateOnly(2026, 11, 5);
        var info = SalesMatchSignificantPayment.Evaluate(
            2_700_000,
            [(day, 400_000)]);
        Assert.Null(info);
    }
}
