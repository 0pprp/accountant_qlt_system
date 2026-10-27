using Application.Customers.Common;
using Xunit;

namespace Application.Tests.Customers;

public class CustomerRatingCalculatorTests
{
    [Theory]
    [InlineData(150, "ممتاز")]
    [InlineData(160, "جيد")]
    [InlineData(179, "ضعيف")]
    [InlineData(199, "متلكئ")]
    [InlineData(200, "سيء")]
    public void ClassifySettledDays_boundaries(int days, string expected)
    {
        Assert.Equal(expected, CustomerRatingCalculator.ClassifySettledDays(days));
    }

    [Fact]
    public void EvaluateLabel_returns_name_only_for_slow_payer()
    {
        var sale = new DateOnly(2026, 9, 1);
        var asOf = sale.AddDays(89);
        var label = CustomerRatingCalculator.EvaluateLabel(
            amountTotalSales: 3_000_000,
            receiptsTotal: 1_200_000,
            amountRemaining: 1_800_000,
            saleDate: sale,
            lastPaymentDate: asOf,
            asOf: asOf);

        Assert.Equal("سيء", label);
    }

    [Fact]
    public void EvaluateLabel_null_when_no_sale()
    {
        Assert.Null(CustomerRatingCalculator.EvaluateLabel(
            100, 10, 90, null, null, new DateOnly(2026, 10, 27)));
    }
}
