using Application.DelegateFollowup.Common;
using Application.SalesRatings.Common;

namespace Application.Tests.ManagementRatings;

public class ManagementRatingsRulesTests
{
    [Theory]
    [InlineData(10, "ممتاز")]
    [InlineData(5, "جيد")]
    [InlineData(0, "ضعيف")]
    [InlineData(-5, "متلكئ")]
    [InlineData(-10, "سيء")]
    public void LevelName_matches_source_bands(int score, string label)
    {
        Assert.Equal(label, CustomerRatingCalculator.LevelName(score));
    }

    [Theory]
    [InlineData(7.0, "ممتاز")]
    [InlineData(3.0, "جيد")]
    [InlineData(0.0, "ضعيف")]
    [InlineData(-7.49, "متلكئ")]
    [InlineData(-7.5, "سيء")]
    public void ClassifyAverage_boundaries_inclusive_as_source(double avg, string label)
    {
        Assert.Equal(label, RatingAggregator.ClassifyAverageLabel(avg));
        Assert.Equal(label, DelegateFollowupRules.ClassifyAverageLabel(avg));
    }

    [Fact]
    public void ActiveCustomer_30_days_not_40()
    {
        var asOf = new DateOnly(2026, 11, 8);
        Assert.True(ActiveCustomerRules.IsActive(asOf.AddDays(-30), asOf));
        Assert.False(ActiveCustomerRules.IsActive(asOf.AddDays(-31), asOf));
        Assert.True(DelegateFollowupRules.IsListActive(asOf.AddDays(-40), asOf));
        Assert.False(DelegateFollowupRules.IsListActive(asOf.AddDays(-41), asOf));
        Assert.NotEqual(ActiveCustomerRules.MaxDaysWithoutPayment, DelegateFollowupRules.MaxDaysListActivity);
    }

    [Fact]
    public void ActiveRatingCohort_excludes_settled_and_inactive()
    {
        var asOf = new DateOnly(2026, 11, 8);
        var facts = new List<RatingCustomerFact>
        {
            new()
            {
                CustomerId = 1,
                AmountTotalSales = 1_000_000,
                ReceiptsTotal = 100_000,
                SaleDate = asOf.AddDays(-10),
                LastPaymentDate = asOf.AddDays(-2)
            },
            new()
            {
                CustomerId = 2,
                AmountTotalSales = 1_000_000,
                ReceiptsTotal = 1_000_000,
                SaleDate = asOf.AddDays(-10),
                LastPaymentDate = asOf.AddDays(-1)
            },
            new()
            {
                CustomerId = 3,
                AmountTotalSales = 1_000_000,
                ReceiptsTotal = 100_000,
                SaleDate = asOf.AddDays(-10),
                LastPaymentDate = asOf.AddDays(-40)
            },
        };

        var built = ActiveRatingCohort.Build(facts, asOf);
        Assert.Single(built);
        Assert.Equal(1, built[0].Fact.CustomerId);
    }

    [Fact]
    public void PeriodAggregate_zero_denominator_returns_empty_not_error()
    {
        var agg = RatingTrendCalculator.PeriodAggregate(
            [],
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 30),
            new DateOnly(2026, 11, 8));
        Assert.Equal(0, agg.CustomerCount);
        Assert.Equal(0, agg.AverageScore);
    }

    [Fact]
    public void Score_active_rate_thresholds()
    {
        var asOf = new DateOnly(2026, 11, 8);
        var sale = asOf.AddDays(-9); // 10 inclusive days
        // rate = receipts*1000/(total*days) — excellent >= 1000/150
        var excellent = CustomerRatingCalculator.ScoreFor(150_000, 150_000, 1, sale, asOf, asOf, false);
        Assert.Equal(10, excellent);

        var zeroDenom = CustomerRatingCalculator.ScoreFor(0, 0, 0, sale, asOf, asOf, false);
        Assert.Null(zeroDenom);
    }

    [Theory]
    [InlineData(1.0, 1.06, "up")]
    [InlineData(1.0, 0.94, "down")]
    [InlineData(1.0, 1.02, "flat")]
    public void Direction_threshold_0_05(double start, double end, string expected)
    {
        Assert.Equal(expected, RatingAggregator.DirectionFromSeries(start, end));
    }
}
