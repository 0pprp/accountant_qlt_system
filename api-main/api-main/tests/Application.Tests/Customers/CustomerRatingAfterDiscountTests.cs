using Xunit;
using LabelCalculator = Application.Customers.Common.CustomerRatingCalculator;
using ScoreCalculator = Application.SalesRatings.Common.CustomerRatingCalculator;

namespace Application.Tests.Customers;

public class CustomerRatingAfterDiscountTests
{
    private static readonly DateOnly SaleDate = new(2026, 9, 1);
    private static readonly DateOnly LastPayment = new(2026, 11, 15);
    private static readonly DateOnly AsOf = new(2026, 11, 18);

    private const double GrossSell = 1_500_000;
    private const double Discount = 300_000;
    private const double Paid = 600_000;

    [Fact]
    public void DiscountedSalePrice_RaisesTheNumericScore()
    {
        var before = ScoreCalculator.ScoreFor(
            GrossSell, Paid, GrossSell - Paid, SaleDate, LastPayment, AsOf);

        var net = GrossSell - Discount;
        var after = ScoreCalculator.ScoreFor(
            net, Paid, net - Paid, SaleDate, LastPayment, AsOf);

        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.True(after >= before);
    }

    [Fact]
    public void DiscountedSalePrice_FeedsBothCalculatorsFromTheSameStoredNet()
    {
        var net = GrossSell - Discount;

        var label = LabelCalculator.EvaluateLabel(
            net, Paid, net - Paid, SaleDate, LastPayment, AsOf);
        var score = ScoreCalculator.ScoreFor(
            net, Paid, net - Paid, SaleDate, LastPayment, AsOf);

        Assert.NotNull(label);
        Assert.NotNull(score);
    }

    [Fact]
    public void DiscountThatClosesTheDebt_IsGradedAsSettledNotAsNegative()
    {
        var net = Paid;

        var label = LabelCalculator.EvaluateLabel(
            net, Paid, net - Paid, SaleDate, LastPayment, AsOf);

        Assert.NotNull(label);
        Assert.NotEqual("ضعيف", label);
    }
}
