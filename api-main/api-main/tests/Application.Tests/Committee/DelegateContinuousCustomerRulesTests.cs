using Application.SalesRatings.Common;

namespace Application.Tests.Committee;

public class DelegateContinuousCustomerRulesTests
{
    private static readonly DateOnly AsOf = new(2026, 11, 16);

    [Fact]
    public void Continuous_matches_delegate_app_rule()
    {
        Assert.True(DelegateContinuousCustomerRules.IsContinuous(
            isLegal: false,
            amountRemaining: 1000,
            lastPaymentDate: AsOf.AddDays(-30),
            asOf: AsOf));
    }

    [Fact]
    public void Excludes_legal_zero_remaining_and_stale_or_missing_payment()
    {
        Assert.False(DelegateContinuousCustomerRules.IsContinuous(
            true, 1000, AsOf.AddDays(-10), AsOf));
        Assert.False(DelegateContinuousCustomerRules.IsContinuous(
            false, 0, AsOf.AddDays(-10), AsOf));
        Assert.False(DelegateContinuousCustomerRules.IsContinuous(
            false, 1000, null, AsOf));
        Assert.False(DelegateContinuousCustomerRules.IsContinuous(
            false, 1000, AsOf.AddDays(-365), AsOf));
        Assert.True(DelegateContinuousCustomerRules.IsContinuous(
            false, 1000, AsOf.AddDays(-364), AsOf));
    }
}
