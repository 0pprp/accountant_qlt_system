using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.App.GetCollectionBook;

public class GetCollectionBookHandler
    : IRequestHandler<GetCollectionBookQuery, Result<GetCollectionBookResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetCollectionBookHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<GetCollectionBookResponse>> Handle(
        GetCollectionBookQuery request,
        CancellationToken cancellationToken)
    {
        var asOf = IraqCalendar.ToLocalDate(_clock.UtcNowOffset);
        var weekAgo = asOf.AddDays(-7);

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MandobId == request.UserId || x.MotabaId == request.UserId)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(cancellationToken);

        if (lists.Count == 0)
        {
            return new GetCollectionBookResponse
            {
                AverageLabel = RatingAggregator.ClassifyAverageLabel(0)
            };
        }

        var listIds = lists.Select(x => x.Id).ToList();
        var primary = lists[0];

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value))
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                CustomerName = o.Customer!.FullName,
                BusinessName = o.Customer.Business.Name,
                Phone = o.Customer.PhoneNumber,
                Address = o.Customer.Business.Address,
                IsLegal = o.Customer.IsLegal,
                o.SellAmount,
                o.DailyInstallmentAmount,
                o.SaleDate,
                o.CreatedAt,
                o.ExecutionStatus,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date)
            })
            .ToListAsync(cancellationToken);
        var unallocated = await Application.Common.Balances.CustomerBalanceQuery.UnallocatedDetailsAsync(
            _db, rows.Select(r => r.CustomerId).Distinct().ToList(), cancellationToken);

        var customers = rows
            .GroupBy(r => r.CustomerId)
            .Select(g =>
            {
                unallocated.TryGetValue(g.Key, out var extra);
                var saleDates = g.Where(x => x.SaleDate != null).Select(x => x.SaleDate!.Value).ToList();
                DateOnly? saleDate = saleDates.Count > 0
                    ? saleDates.Min()
                    : g.Where(x => x.CreatedAt != null)
                        .Select(x => IraqCalendar.ToLocalDate(x.CreatedAt!.Value))
                        .OrderBy(d => d)
                        .Cast<DateOnly?>()
                        .FirstOrDefault();

                var sell = g.Sum(x => x.SellAmount);
                var paid = g.Sum(x => x.Receipts) + (extra?.Amount ?? 0);
                var remaining = Application.Common.Balances.CustomerBalanceRules.RoundToUnit(sell - paid);
                var lastPayment = Application.Common.Balances.CustomerBalanceQuery.LatestDate(g.Max(x => x.LastPaymentDate), extra?.LastDate);
                var daily = g
                    .Where(x => x.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .Sum(x => x.DailyInstallmentAmount);
                var activeOrder = g
                    .Where(x => x.ExecutionStatus == OrderExecutionStatus.InProgress && x.SellAmount > x.Receipts)
                    .OrderByDescending(x => x.Id)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefault()
                    ?? g.OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefault();

                // RealRating: بدون phantom sale-date boost
                var score = remaining > 0
                    ? CustomerRatingCalculator.ScoreFor(
                        sell, paid, remaining, saleDate, lastPayment, asOf, false)
                    : null;

                var sample = g.First();
                return new CollectionBookCustomerDto
                {
                    CustomerId = g.Key,
                    FullName = string.IsNullOrWhiteSpace(sample.CustomerName)
                        ? $"زبون #{g.Key}"
                        : sample.CustomerName,
                    BusinessName = sample.BusinessName,
                    PhoneNumber = sample.Phone,
                    Address = sample.Address,
                    AmountTotalSales = sell,
                    ReceiptsTotal = paid,
                    AmountRemaining = remaining,
                    DailyInstallmentAmount = daily,
                    SaleDate = saleDate,
                    LastPaymentDate = lastPayment,
                    ActiveOrderId = activeOrder,
                    IsLegal = g.Any(x => x.IsLegal),
                    Score = score,
                    RatingLabel = score is int s ? CustomerRatingCalculator.LevelName(s) : null
                };
            })
            .OrderBy(c => c.FullName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        // تسديدات آخر 7 أيام لكل زبون (Amount1 = اليوم … Amount7 = قبل 6 أيام)
        var weekFrom = asOf.AddDays(-6);
        var customerIds = customers.Select(c => c.CustomerId).ToList();
        if (customerIds.Count > 0)
        {
            var weekPayments = await _db.InstallmentPayments.AsNoTracking()
                .Where(ip => ip.Order != null
                             && ip.Order.OrderListId != null
                             && listIds.Contains(ip.Order.OrderListId.Value)
                             && customerIds.Contains(ip.Order.CustomerId)
                             && ip.Date >= weekFrom
                             && ip.Date <= asOf)
                .Select(ip => new { ip.Order!.CustomerId, ip.Date, ip.Amount })
                .ToListAsync(cancellationToken);

            var byCustomer = weekPayments
                .GroupBy(x => x.CustomerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.Date).ToDictionary(d => d.Key, d => d.Sum(x => x.Amount)));

            foreach (var c in customers)
            {
                if (!byCustomer.TryGetValue(c.CustomerId, out var byDay))
                    continue;
                double Day(int offset) =>
                    byDay.TryGetValue(asOf.AddDays(-offset), out var a) ? a : 0;
                c.Amount1 = Day(0);
                c.Amount2 = Day(1);
                c.Amount3 = Day(2);
                c.Amount4 = Day(3);
                c.Amount5 = Day(4);
                c.Amount6 = Day(5);
                c.Amount7 = Day(6);
            }
        }

        var openScores = customers
            .Where(c => c.AmountRemaining > 0 && c.Score is not null)
            .Select(c => c.Score!.Value)
            .ToList();

        var weekScores = new List<int>();
        foreach (var c in customers.Where(x => x.AmountRemaining > 0))
        {
            var weekScore = CustomerRatingCalculator.ScoreFor(
                c.AmountTotalSales,
                c.ReceiptsTotal,
                c.AmountRemaining,
                c.SaleDate,
                c.LastPaymentDate,
                weekAgo,
                false);
            if (weekScore is int ws)
                weekScores.Add(ws);
        }

        var avg = openScores.Count == 0 ? 0 : openScores.Average();
        var weekAvg = weekScores.Count == 0 ? 0 : weekScores.Average();

        return new GetCollectionBookResponse
        {
            ListId = primary.Id,
            ListName = primary.Name,
            AverageScore = Math.Round(avg, 2, MidpointRounding.AwayFromZero),
            AverageLabel = RatingAggregator.ClassifyAverageLabel(avg),
            PreviousWeekScore = Math.Round(weekAvg, 2, MidpointRounding.AwayFromZero),
            RatedCustomerCount = openScores.Count,
            InstallmentSum = customers
                .Where(c => c.AmountRemaining > 0)
                .Sum(c => c.DailyInstallmentAmount),
            Customers = customers
        };
    }
}
