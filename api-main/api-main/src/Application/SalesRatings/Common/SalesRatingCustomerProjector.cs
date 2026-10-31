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

        var result = new List<SalesRatedCustomer>();
        foreach (var row in rows)
        {
            var remaining = row.AmountTotalSales - row.ReceiptsTotal;
            var score = CustomerRatingCalculator.ScoreFor(
                row.AmountTotalSales,
                row.ReceiptsTotal,
                remaining,
                row.SaleDate,
                row.LastPaymentDate,
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
                AmountRemaining = remaining,
                SaleDate = row.SaleDate,
                LastPaymentDate = row.LastPaymentDate,
                AmountTotalSales = row.AmountTotalSales,
                ReceiptsTotal = row.ReceiptsTotal
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
