using Application.SalesFollowup.Common;
using Xunit;

namespace Application.Tests.SalesFollowup;

public class SalesFollowupRulesTests
{
    [Theory]
    [InlineData(10, false)]
    [InlineData(5, false)]
    [InlineData(0, true)]
    [InlineData(-5, true)]
    [InlineData(-10, true)]
    public void IsLow_matches_current_scale(int score, bool expected) =>
        Assert.Equal(expected, SalesFollowupRules.IsLow(score));

    [Theory]
    [InlineData(10, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    public void IsHigh_matches_current_scale(int score, bool expected) =>
        Assert.Equal(expected, SalesFollowupRules.IsHigh(score));

    [Fact]
    public void IsZeroedCustomer_requires_paid_sale_and_non_legal()
    {
        Assert.True(SalesFollowupRules.IsZeroedCustomer(new SalesFollowupCustomerFact
        {
            CustomerId = 1,
            CustomerName = "A",
            AmountTotalSales = 1000,
            ReceiptsTotal = 1000,
            AmountRemaining = 0,
            IsLegal = false,
        }));

        Assert.False(SalesFollowupRules.IsZeroedCustomer(new SalesFollowupCustomerFact
        {
            CustomerId = 2,
            CustomerName = "B",
            AmountTotalSales = 1000,
            ReceiptsTotal = 1000,
            AmountRemaining = 0,
            IsLegal = true,
        }));

        Assert.False(SalesFollowupRules.IsZeroedCustomer(new SalesFollowupCustomerFact
        {
            CustomerId = 3,
            CustomerName = "C",
            AmountTotalSales = 0,
            ReceiptsTotal = 0,
            AmountRemaining = 0,
            IsLegal = false,
        }));
    }

    [Fact]
    public void AnonymizeLowOnly_strips_identity_and_keeps_low_only()
    {
        var rows = new[]
        {
            new SalesFollowupRatingRowDto
            {
                Score = 10,
                CustomerName = "High",
                RatingDate = DateTime.Today,
                Kind = "system",
            },
            new SalesFollowupRatingRowDto
            {
                Score = 0,
                CustomerName = "Low",
                Note = "n",
                RatingDate = DateTime.Today,
                Kind = "system",
                ListName = "L1",
            },
        };

        var result = SalesFollowupRules.AnonymizeLowOnly(rows);
        Assert.Single(result);
        Assert.Equal(0, result[0].Score);
        Assert.Null(result[0].CustomerName);
        Assert.Null(result[0].ListName);
        Assert.Equal("n", result[0].Note);
        Assert.Equal("system", result[0].Kind);
    }
}
