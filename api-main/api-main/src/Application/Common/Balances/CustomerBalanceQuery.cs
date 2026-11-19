using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Balances;

public sealed record CustomerBalance(int CustomerId, double SellTotal, double PaidTotal)
{
    public double Raw => SellTotal - PaidTotal;
    public double Rounded => CustomerBalanceRules.RoundToUnit(Raw);
}

public sealed record UnallocatedPayments(int CustomerId, double Amount, int Count, DateOnly? LastDate);

public static class CustomerBalanceRules
{
    public const double RoundingUnit = 1000;

    public static double RoundToUnit(double amount)
        => Math.Round(amount / RoundingUnit, MidpointRounding.AwayFromZero) * RoundingUnit;

    public static bool HasOpenBalance(double rawRemaining) => RoundToUnit(rawRemaining) > 0;

    public static double SumRounded(IEnumerable<CustomerBalance> balances)
        => balances.Sum(b => b.Rounded);

    public static CustomerBalance Combine(int customerId, double sell, double allocatedPaid, double unallocatedPaid)
        => new(customerId, sell, allocatedPaid + unallocatedPaid);
}

public static class CustomerBalanceQuery
{
    public static async Task<List<CustomerBalance>> ForBranchesAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branchIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0)
            return [];

        var sales = await db.Orders.AsNoTracking()
            .Where(o => branchIds.Contains(o.BranchId) && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Sell = g.Sum(o => o.SellAmount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Sell, ct);

        var allocated = await db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId != null && p.Order != null && branchIds.Contains(p.Order.BranchId))
            .GroupBy(p => p.Order!.CustomerId)
            .Select(g => new { CustomerId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Paid, ct);

        var unallocated = await db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId == null && p.CustomerId != null
                        && db.Customers.Any(c => c.Id == p.CustomerId && branchIds.Contains(c.BranchId)))
            .GroupBy(p => p.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Paid, ct);

        return Merge(sales, allocated, unallocated);
    }

    public static async Task<Dictionary<int, CustomerBalance>> ForCustomersAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> customerIds,
        CancellationToken ct)
    {
        if (customerIds.Count == 0)
            return [];

        var sales = await db.Orders.AsNoTracking()
            .Where(o => customerIds.Contains(o.CustomerId) && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Sell = g.Sum(o => o.SellAmount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Sell, ct);

        var allocated = await db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId != null && p.Order != null && customerIds.Contains(p.Order.CustomerId))
            .GroupBy(p => p.Order!.CustomerId)
            .Select(g => new { CustomerId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Paid, ct);

        var unallocated = await UnallocatedByCustomerAsync(db, customerIds, null, ct);

        return Merge(sales, allocated, unallocated).ToDictionary(b => b.CustomerId);
    }

    public static Task<Dictionary<int, double>> UnallocatedByCustomerAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> customerIds,
        DateOnly? asOf,
        CancellationToken ct)
        => db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId == null
                        && p.CustomerId != null
                        && customerIds.Contains(p.CustomerId.Value)
                        && (asOf == null || p.Date <= asOf))
            .GroupBy(p => p.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Paid, ct);

    public static async Task<Dictionary<int, UnallocatedPayments>> UnallocatedDetailsAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> customerIds,
        CancellationToken ct,
        DateOnly? asOf = null)
    {
        if (customerIds.Count == 0)
            return [];

        return await db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId == null
                        && p.CustomerId != null
                        && customerIds.Contains(p.CustomerId.Value)
                        && (asOf == null || p.Date <= asOf))
            .GroupBy(p => p.CustomerId!.Value)
            .Select(g => new UnallocatedPayments(g.Key, g.Sum(p => p.Amount), g.Count(), g.Max(p => (DateOnly?)p.Date)))
            .ToDictionaryAsync(x => x.CustomerId, ct);
    }

    public static DateOnly? LatestDate(DateOnly? a, DateOnly? b)
        => a is null ? b : b is null ? a : (a > b ? a : b);

    private static List<CustomerBalance> Merge(
        IReadOnlyDictionary<int, double> sales,
        IReadOnlyDictionary<int, double> allocated,
        IReadOnlyDictionary<int, double> unallocated)
        => sales.Keys
            .Concat(allocated.Keys)
            .Concat(unallocated.Keys)
            .Distinct()
            .Select(id => CustomerBalanceRules.Combine(
                id,
                sales.GetValueOrDefault(id),
                allocated.GetValueOrDefault(id),
                unallocated.GetValueOrDefault(id)))
            .ToList();
}
