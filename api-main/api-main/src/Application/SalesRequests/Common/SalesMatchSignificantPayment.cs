using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Common;

public enum SalesMatchPaymentShareBand
{
    None = 0,
    Orange = 1,
    Green = 2
}

public sealed record SalesMatchSignificantPaymentInfo(
    DateOnly Date,
    double Amount,
    SalesMatchPaymentShareBand Band,
    double Ratio);

/// <summary>
/// فلتر نتائج المطابقة: أكبر مجموع تسديد ليوم واحد خلال آخر 30 يوماً
/// نسبة إلى إجمالي البيع المعتمد — أخضر ≥25%، برتقالي 20–24%، دون ذلك يُستبعد.
/// </summary>
public static class SalesMatchSignificantPayment
{
    public const double GreenThreshold = 0.25;
    public const double OrangeThreshold = 0.20;
    public const int WindowDays = 30;

    public static string BandName(SalesMatchPaymentShareBand band) => band switch
    {
        SalesMatchPaymentShareBand.Green => "green",
        SalesMatchPaymentShareBand.Orange => "orange",
        _ => "none"
    };

    public static SalesMatchSignificantPaymentInfo? Evaluate(
        double sellTotal,
        IEnumerable<(DateOnly Date, double Amount)> paymentsInWindow)
    {
        if (sellTotal <= 0)
            return null;

        var best = paymentsInWindow
            .GroupBy(p => p.Date)
            .Select(g => (Date: g.Key, Amount: g.Sum(x => x.Amount)))
            .Where(x => x.Amount > 0)
            .OrderByDescending(x => x.Amount)
            .ThenByDescending(x => x.Date)
            .FirstOrDefault();

        if (best.Amount <= 0)
            return null;

        var ratio = best.Amount / sellTotal;
        if (ratio >= GreenThreshold)
            return new SalesMatchSignificantPaymentInfo(
                best.Date, best.Amount, SalesMatchPaymentShareBand.Green, ratio);
        if (ratio >= OrangeThreshold)
            return new SalesMatchSignificantPaymentInfo(
                best.Date, best.Amount, SalesMatchPaymentShareBand.Orange, ratio);
        return null;
    }

    public static async Task<Dictionary<int, SalesMatchSignificantPaymentInfo>> LoadQualifiedAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> customerIds,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, SalesMatchSignificantPaymentInfo>();
        if (customerIds.Count == 0)
            return result;

        var windowStart = asOf.AddDays(-(WindowDays - 1));
        var ids = customerIds.Distinct().ToList();

        var sellTotals = await db.Orders.AsNoTracking()
            .Where(o =>
                ids.Contains(o.CustomerId)
                && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, SellTotal = g.Sum(o => o.SellAmount) })
            .ToListAsync(cancellationToken);

        var sellMap = sellTotals.ToDictionary(x => x.CustomerId, x => x.SellTotal);

        var payments = await db.InstallmentPayments.AsNoTracking()
            .Where(ip =>
                ip.Order != null
                && ids.Contains(ip.Order.CustomerId)
                && ip.Order.ApprovalStatus == OrderApprovalStatus.Approved
                && ip.Date >= windowStart
                && ip.Date <= asOf)
            .Select(ip => new
            {
                ip.Order!.CustomerId,
                ip.Date,
                ip.Amount
            })
            .ToListAsync(cancellationToken);

        var byCustomer = payments.GroupBy(p => p.CustomerId);
        foreach (var group in byCustomer)
        {
            if (!sellMap.TryGetValue(group.Key, out var sell) || sell <= 0)
                continue;

            var info = Evaluate(
                sell,
                group.Select(p => (p.Date, p.Amount)));
            if (info is not null)
                result[group.Key] = info;
        }

        return result;
    }
}
