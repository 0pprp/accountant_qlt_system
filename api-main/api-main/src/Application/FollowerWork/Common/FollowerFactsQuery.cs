using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public static class FollowerFactsQuery
{
    public static async Task<List<RatingCustomerFact>> LoadAsync(
        IApplicationDbContext db,
        IReadOnlyList<int> listIds,
        DateOnly? asOfCutoff,
        IReadOnlyList<int>? customerIds,
        CancellationToken ct)
    {
        if (listIds.Count == 0)
            return [];

        var query = db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value));

        if (customerIds is not null)
            query = query.Where(o => customerIds.Contains(o.CustomerId));

        if (asOfCutoff is DateOnly cutoff)
            query = query.Where(o => o.SaleDate == null || o.SaleDate <= cutoff);

        var rows = await query
            .Select(o => new
            {
                o.CustomerId,
                o.OrderListId,
                o.SellerId,
                o.SellAmount,
                o.DailyInstallmentAmount,
                o.SaleDate,
                o.CreatedAt,
                Receipts = o.InstallmentPayments
                    .Where(ip => asOfCutoff == null || ip.Date <= asOfCutoff)
                    .Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments
                    .Where(ip => asOfCutoff == null || ip.Date <= asOfCutoff)
                    .Max(ip => (DateOnly?)ip.Date),
                IsLegal = o.Customer != null && o.Customer.IsLegal
            })
            .ToListAsync(ct);

        var rowCustomerIds = rows.Select(r => r.CustomerId).Distinct().ToList();
        var unallocated = await db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId == null
                        && p.CustomerId != null
                        && rowCustomerIds.Contains(p.CustomerId.Value)
                        && (asOfCutoff == null || p.Date <= asOfCutoff))
            .GroupBy(p => p.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Amount = g.Sum(p => p.Amount), LastDate = g.Max(p => (DateOnly?)p.Date) })
            .ToDictionaryAsync(x => x.CustomerId, ct);

        var unallocatedListByCustomer = rows
            .Where(r => unallocated.ContainsKey(r.CustomerId))
            .GroupBy(r => r.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.SaleDate ?? DateOnly.MinValue)
                    .ThenByDescending(r => r.CreatedAt ?? DateTimeOffset.MinValue)
                    .First().OrderListId);

        return rows
            .GroupBy(r => new { r.CustomerId, r.OrderListId })
            .Select(g =>
            {
                var saleDates = g.Where(x => x.SaleDate != null).Select(x => x.SaleDate!.Value).ToList();
                DateOnly? saleDate = saleDates.Count > 0
                    ? saleDates.Min()
                    : g.Where(x => x.CreatedAt != null)
                        .Select(x => IraqCalendar.ToLocalDate(x.CreatedAt!.Value))
                        .OrderBy(d => d)
                        .Cast<DateOnly?>()
                        .FirstOrDefault();

                var receipts = g.Sum(x => x.Receipts);
                var lastPayment = g.Max(x => x.LastPaymentDate);
                if (unallocated.TryGetValue(g.Key.CustomerId, out var extra)
                    && unallocatedListByCustomer.TryGetValue(g.Key.CustomerId, out var targetList)
                    && targetList == g.Key.OrderListId)
                {
                    receipts += extra.Amount;
                    if (extra.LastDate is DateOnly d && (lastPayment is null || d > lastPayment))
                        lastPayment = d;
                }

                return new RatingCustomerFact
                {
                    CustomerId = g.Key.CustomerId,
                    OrderListId = g.Key.OrderListId,
                    SellerId = g.Select(x => x.SellerId).FirstOrDefault(x => x != null),
                    AmountTotalSales = g.Sum(x => x.SellAmount),
                    ReceiptsTotal = receipts,
                    AmountDaySales = g.Sum(x => x.DailyInstallmentAmount),
                    SaleDate = saleDate,
                    LastPaymentDate = lastPayment,
                    IsLegal = g.Any(x => x.IsLegal)
                };
            })
            .ToList();
    }

    public static IReadOnlyList<double> NonEmptyListAverages(IEnumerable<RatingAggregateResult> perList)
        => perList.Where(a => a.CustomerCount > 0).Select(a => a.AverageScore).ToList();
}
