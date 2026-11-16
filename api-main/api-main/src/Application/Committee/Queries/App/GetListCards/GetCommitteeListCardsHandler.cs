using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetListCards;

public class GetCommitteeListCardsHandler
    : IRequestHandler<GetCommitteeListCardsQuery, Result<List<CommitteeListCardDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetCommitteeListCardsHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<List<CommitteeListCardDto>>> Handle(
        GetCommitteeListCardsQuery request,
        CancellationToken cancellationToken)
    {
        var asOf = IraqCalendar.ToLocalDate(_clock.UtcNowOffset);
        var weekAgo = asOf.AddDays(-7);

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MotabaId == request.UserId || x.MandobId == request.UserId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                MandobName = x.Mandob != null ? x.Mandob.FullName : "",
                CollectionEnabled = x.FollowerCollectionEnabled
            })
            .ToListAsync(cancellationToken);

        if (lists.Count == 0)
            return new List<CommitteeListCardDto>();

        var listIds = lists.Select(x => x.Id).ToList();
        var facts = await MaterializeFactsForListsAsync(listIds, cancellationToken);
        var factsByList = facts
            .Where(f => f.OrderListId is > 0)
            .GroupBy(f => f.OrderListId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var cards = new List<CommitteeListCardDto>(lists.Count);
        foreach (var list in lists)
        {
            factsByList.TryGetValue(list.Id, out var listFacts);
            listFacts ??= [];

            // نفس معادلة دفتر التحصيل عند المندوب: RealRating + متوسط بسيط
            var (todayAvg, todayCount, todayLabel) = AverageLikeCollectionBook(listFacts, asOf);
            var (weekAvg, _, _) = AverageLikeCollectionBook(listFacts, weekAgo);
            var (located, total) = ContinuousLocationCounts(listFacts, asOf);

            cards.Add(new CommitteeListCardDto
            {
                ListId = list.Id,
                ListName = list.Name,
                MandobName = list.MandobName ?? "",
                PhantomLabel = todayLabel,
                CurrentMonthScore = Math.Round(todayAvg, 2, MidpointRounding.AwayFromZero),
                PreviousMonthScore = Math.Round(weekAvg, 2, MidpointRounding.AwayFromZero),
                ActiveCustomers = todayCount,
                CollectionEnabled = list.CollectionEnabled,
                LocatedCustomers = located,
                LocationCustomersTotal = total
            });
        }

        return cards;
    }

    /// <summary>مطابق GetCollectionBookHandler: باقي &gt; 0 و ScoreFor(..., usePhantomSaleDate: false).</summary>
    private static (double Average, int Count, string Label) AverageLikeCollectionBook(
        IReadOnlyList<ListCardFact> facts,
        DateOnly asOf)
    {
        var scores = new List<int>();
        foreach (var f in facts)
        {
            if (f.AmountRemaining <= 0)
                continue;
            var score = CustomerRatingCalculator.ScoreFor(
                f.AmountTotalSales,
                f.ReceiptsTotal,
                f.AmountRemaining,
                f.SaleDate,
                f.LastPaymentDate,
                asOf,
                usePhantomSaleDate: false);
            if (score is int s)
                scores.Add(s);
        }

        var avg = scores.Count == 0 ? 0 : scores.Average();
        return (avg, scores.Count, RatingAggregator.ClassifyAverageLabel(avg));
    }

    private static (int Located, int Total) ContinuousLocationCounts(
        IReadOnlyList<ListCardFact> facts,
        DateOnly asOf)
    {
        var continuous = facts
            .Where(f => DelegateContinuousCustomerRules.IsContinuous(
                f.IsLegal, f.AmountRemaining, f.LastPaymentDate, asOf))
            .GroupBy(f => f.CustomerId)
            .ToList();

        var total = continuous.Count;
        var located = continuous.Count(g => g.Any(x => x.CustomerHasGps || x.OrderHasGps));
        return (located, total);
    }

    private async Task<List<ListCardFact>> MaterializeFactsForListsAsync(
        IReadOnlyList<int> listIds,
        CancellationToken ct)
    {
        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value))
            .Select(o => new
            {
                o.CustomerId,
                o.OrderListId,
                o.SellAmount,
                o.SaleDate,
                o.CreatedAt,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
                IsLegal = o.Customer != null && o.Customer.IsLegal,
                CustomerHasGps = o.Customer != null
                                 && o.Customer.Business.GpsLatitude != null
                                 && o.Customer.Business.GpsLongitude != null
                                 && !(o.Customer.Business.GpsLatitude == 0
                                      && o.Customer.Business.GpsLongitude == 0),
                OrderHasGps = o.Location != null
                              && !(o.Location.Latitude == 0 && o.Location.Longitude == 0)
            })
            .ToListAsync(ct);

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

                var sell = g.Sum(x => x.SellAmount);
                var paid = g.Sum(x => x.Receipts);
                return new ListCardFact
                {
                    CustomerId = g.Key.CustomerId,
                    OrderListId = g.Key.OrderListId,
                    AmountTotalSales = sell,
                    ReceiptsTotal = paid,
                    AmountRemaining = Math.Round(sell - paid, MidpointRounding.AwayFromZero),
                    SaleDate = saleDate,
                    LastPaymentDate = g.Max(x => x.LastPaymentDate),
                    IsLegal = g.Any(x => x.IsLegal),
                    CustomerHasGps = g.Any(x => x.CustomerHasGps),
                    OrderHasGps = g.Any(x => x.OrderHasGps)
                };
            })
            .ToList();
    }

    private sealed class ListCardFact
    {
        public int CustomerId { get; init; }
        public int? OrderListId { get; init; }
        public double AmountTotalSales { get; init; }
        public double ReceiptsTotal { get; init; }
        public double AmountRemaining { get; init; }
        public DateOnly? SaleDate { get; init; }
        public DateOnly? LastPaymentDate { get; init; }
        public bool IsLegal { get; init; }
        public bool CustomerHasGps { get; init; }
        public bool OrderHasGps { get; init; }
    }
}
