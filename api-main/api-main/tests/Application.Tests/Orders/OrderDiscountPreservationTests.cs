using Domain.Entities.OrderAggregate;
using Xunit;

namespace Application.Tests.Orders;

public class OrderDiscountPreservationTests
{
    [Fact]
    public void EditingAnOrder_KeepsTheAmountNet_AndTheAppliedDiscountIntact()
    {
        var order = Order.CreateByAdmin(900_000, 1_500_000, 0, 10_000, customerId: 1, branchId: 1, orderItems: []);
        order.ApplyCustomerDiscounts(100_000, 1_000);

        order.Update(
            buyAmount: 900_000,
            grossSellAmount: 1_500_000,
            prepaymentAmount: 0,
            grossDailyInstallmentAmount: 10_000,
            location: null,
            creationAddress: "النجف",
            saleDate: new DateOnly(2026, 11, 1),
            saleTime: new TimeOnly(10, 30),
            sellerId: 1,
            orderListId: 1);

        Assert.Equal(1_400_000, order.SellAmount);
        Assert.Equal(9_000, order.DailyInstallmentAmount);
        Assert.Equal(100_000, order.SellAmountDiscountApplied);
        Assert.Equal(1_000, order.DailyInstallmentDiscountApplied);
    }

    [Fact]
    public void EditingAnOrderToNewItemTotals_RebasesTheDiscountOnTheNewGross()
    {
        var order = Order.CreateByAdmin(900_000, 1_500_000, 0, 10_000, customerId: 1, branchId: 1, orderItems: []);
        order.ApplyCustomerDiscounts(100_000, 1_000);

        order.Update(
            buyAmount: 1_200_000,
            grossSellAmount: 2_000_000,
            prepaymentAmount: 0,
            grossDailyInstallmentAmount: 12_000,
            location: null,
            creationAddress: "النجف",
            saleDate: new DateOnly(2026, 11, 1),
            saleTime: new TimeOnly(10, 30),
            sellerId: 1,
            orderListId: 1);

        Assert.Equal(1_900_000, order.SellAmount);
        Assert.Equal(11_000, order.DailyInstallmentAmount);
        Assert.Equal(2_000_000, order.GrossSell());
    }

    [Fact]
    public void GrossSurvivesTheDiscount_SoCheckoutRetryStillMatchesTheBasket()
    {
        var order = Order.CreateByAdmin(900_000, 0, 0, 0, customerId: 1, branchId: 1, orderItems: []);

        order.SetSaleAmounts(
            grossSellAmount: 1_500_000,
            prepaymentAmount: 150_000,
            grossDailyInstallmentAmount: 10_000,
            sellDiscount: 100_000,
            dailyDiscount: 1_000);

        Assert.Equal(1_400_000, order.SellAmount);
        Assert.Equal(9_000, order.DailyInstallmentAmount);
        Assert.Equal(1_500_000, order.GrossSell());
        Assert.Equal(10_000, order.GrossDaily());
        Assert.Equal(150_000, order.PrepaymentAmount);
    }

    [Fact]
    public void SettingSaleAmountsTwice_IsNotCumulative()
    {
        var order = Order.CreateByAdmin(900_000, 0, 0, 0, customerId: 1, branchId: 1, orderItems: []);

        order.SetSaleAmounts(1_500_000, 0, 10_000, 100_000, 1_000);
        order.SetSaleAmounts(1_500_000, 0, 10_000, 100_000, 1_000);

        Assert.Equal(1_400_000, order.SellAmount);
        Assert.Equal(1_500_000, order.GrossSell());
    }

    [Fact]
    public void WaiverReducesTheNet_WithoutResurrectingTheCustomerDiscount()
    {
        var order = Order.CreateByAdmin(900_000, 1_500_000, 0, 10_000, customerId: 1, branchId: 1, orderItems: []);
        order.ApplyCustomerDiscounts(100_000, 0);

        order.ApplyWaiverDiscount(200_000);

        Assert.Equal(1_200_000, order.SellAmount);
        Assert.Equal(100_000, order.SellAmountDiscountApplied);

        order.ApplyCustomerDiscounts(100_000, 0);
        Assert.Equal(1_200_000, order.SellAmount);
    }
}
