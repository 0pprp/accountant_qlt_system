using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRatings.Common;

public static class SalesRatingCustomerProjector
{
    public static async Task<List<SalesRatedCustomer>> GetRatedCustomersAsync(
        IApplicationDbContext dbContext,
        int sellerUserId,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.Orders.AsNoTracking()
            .Where(x => x.SellerId == sellerUserId)
            .Where(x => x.ApprovalStatus == OrderApprovalStatus.Approved)
            .GroupBy(x => new { x.CustomerId, x.Customer!.FullName, x.Customer.PhoneNumber })
            .Select(g => new
            {
                g.Key.CustomerId,
                CustomerName = g.Key.FullName,
                Phone = g.Key.PhoneNumber,
                AmountTotalSales = g.Sum(x => x.SellAmount),
                ReceiptsTotal = g.Sum(x => x.InstallmentPayments.Sum(ip => ip.Amount)),
                SaleDate = g.Min(x => x.SaleDate),
                LastPaymentDate = g.SelectMany(x => x.InstallmentPayments).Max(ip => (DateOnly?)ip.Date)
            })
            .ToListAsync(cancellationToken);
        var unallocated = await Application.Common.Balances.CustomerBalanceQuery.UnallocatedDetailsAsync(
            dbContext, rows.Select(r => r.CustomerId).Distinct().ToList(), cancellationToken);

        var result = new List<SalesRatedCustomer>();
        foreach (var row in rows)
        {
            unallocated.TryGetValue(row.CustomerId, out var extra);
            var receipts = row.ReceiptsTotal + (extra?.Amount ?? 0);
            var lastPayment = Application.Common.Balances.CustomerBalanceQuery.LatestDate(row.LastPaymentDate, extra?.LastDate);
            var remaining = row.AmountTotalSales - receipts;
            var score = CustomerRatingCalculator.ScoreFor(
                row.AmountTotalSales,
                receipts,
                remaining,
                row.SaleDate,
                lastPayment,
                asOf);
            if (score is null)
                continue;

            var level = CustomerRatingCalculator.LevelName(score.Value);
            result.Add(new SalesRatedCustomer
            {
                CustomerId = row.CustomerId,
                CustomerName = row.CustomerName,
                Phone = row.Phone,
                Level = level,
                RatingLabel = level,
                Score = score.Value,
                AmountRemaining = Application.Common.Balances.CustomerBalanceRules.RoundToUnit(remaining),
                SaleDate = row.SaleDate,
                LastPaymentDate = lastPayment,
                AmountTotalSales = row.AmountTotalSales,
                ReceiptsTotal = receipts
            });
        }

        return result;
    }
}

public record SalesRatedCustomer
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public required string Phone { get; set; }
    public required string Level { get; set; }
    public required string RatingLabel { get; set; }
    public int Score { get; set; }
    public double AmountRemaining { get; set; }
    public DateOnly? SaleDate { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public double AmountTotalSales { get; set; }
    public double ReceiptsTotal { get; set; }
}
