using Application.Common.Balances;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Balances;

public class CustomerBalanceQueryTests
{
    private const int BranchId = 4;

    [Theory]
    [InlineData(0, 0)]
    [InlineData(499, 0)]
    [InlineData(500, 1000)]
    [InlineData(1499, 1000)]
    [InlineData(-600, -1000)]
    [InlineData(2_072_357_242, 2_072_357_000)]
    public void Rounds_to_nearest_thousand_like_source(double raw, double expected)
        => Assert.Equal(expected, CustomerBalanceRules.RoundToUnit(raw));

    [Fact]
    public async Task Branch_total_counts_unallocated_payments_and_credit_only_customers()
    {
        await using var db = CreateDb();
        var withSale = NewCustomer("زبون بمبيع");
        var creditOnly = NewCustomer("زبون دفع بلا مبيع");
        var settled = NewCustomer("زبون باقي كسور");
        db.Customers.AddRange(withSale, creditOnly, settled);
        await db.SaveChangesAsync();

        var order = NewOrder(withSale.Id, 1_000_000);
        order.InstallmentPayments.Add(new InstallmentPayment(300_000, new DateOnly(2026, 10, 1)));
        var small = NewOrder(settled.Id, 100_400);
        small.InstallmentPayments.Add(new InstallmentPayment(100_000, new DateOnly(2026, 10, 1)));
        db.Orders.AddRange(order, small);
        db.InstallmentPayments.AddRange(
            new InstallmentPayment(200_000, new DateOnly(2026, 11, 1)) { CustomerId = withSale.Id },
            new InstallmentPayment(1_345_000, new DateOnly(2026, 11, 1)) { CustomerId = creditOnly.Id });
        await db.SaveChangesAsync();

        var balances = await CustomerBalanceQuery.ForBranchesAsync(db, [BranchId], CancellationToken.None);

        Assert.Equal(3, balances.Count);
        Assert.Equal(500_000, balances.Single(b => b.CustomerId == withSale.Id).Rounded);
        Assert.Equal(-1_345_000, balances.Single(b => b.CustomerId == creditOnly.Id).Rounded);
        Assert.Equal(0, balances.Single(b => b.CustomerId == settled.Id).Rounded);
        Assert.Equal(500_000 - 1_345_000, CustomerBalanceRules.SumRounded(balances));
    }

    [Fact]
    public async Task Each_payment_is_counted_once_for_the_customer()
    {
        await using var db = CreateDb();
        var customer = NewCustomer("زبون");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        var order = NewOrder(customer.Id, 500_000);
        order.InstallmentPayments.Add(new InstallmentPayment(100_000, new DateOnly(2026, 10, 1)) { CustomerId = customer.Id });
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var byCustomer = await CustomerBalanceQuery.ForCustomersAsync(db, [customer.Id], CancellationToken.None);

        Assert.Equal(100_000, byCustomer[customer.Id].PaidTotal);
        Assert.Equal(400_000, byCustomer[customer.Id].Rounded);
    }

    private static Customer NewCustomer(string name)
        => new(name, "ام", Guid.NewGuid().ToString("N")[..8], new DateOnly(1990, 1, 1), "07700000000", "07700000000",
            new Business("محل", "عنوان", "علامة"), branchId: BranchId);

    private static Order NewOrder(int customerId, double sell)
        => Order.CreateByAdmin(sell * 0.6, sell, 0, 10_000, customerId, branchId: BranchId, orderItems: []);

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
