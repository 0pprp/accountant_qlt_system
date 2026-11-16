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

        var locationCounts = await MaterializeContinuousLocationCountsAsync(
            listIds, facts, asOf, cancellationToken);

        var cards = new List<CommitteeListCardDto>(lists.Count);
        foreach (var list in lists)
        {
            factsByList.TryGetValue(list.Id, out var listFacts);
            listFacts ??= [];

            // نفس تقييم تطبيق المندوب (collection-book): RealRating بدون phantom
            var today = ActiveRatingCohort.Aggregate(
                listFacts, asOf, usePhantomSaleDate: false, requireRecentPayment: false);
            var previous = ActiveRatingCohort.Aggregate(
                listFacts, weekAgo, usePhantomSaleDate: false, requireRecentPayment: false);

            locationCounts.TryGetValue(list.Id, out var loc);
            cards.Add(new CommitteeListCardDto
            {
                ListId = list.Id,
                ListName = list.Name,
                MandobName = list.MandobName ?? "",
                PhantomLabel = today.FinalRatingLabel,
                CurrentMonthScore = Math.Round(today.AverageScore, 2, MidpointRounding.AwayFromZero),
                PreviousMonthScore = Math.Round(previous.AverageScore, 2, MidpointRounding.AwayFromZero),
                ActiveCustomers = today.CustomerCount,
                CollectionEnabled = list.CollectionEnabled,
                LocatedCustomers = loc.Located,
                LocationCustomersTotal = loc.Total
            });
        }

        return cards;
    }

    /// <summary>
    /// عداد المواقع فقط على مستمرين المندوب (باقي &gt; 0، غير قانوني، آخر تسديد &lt; 365 يوم).
    /// </summary>
    private async Task<Dictionary<int, (int Located, int Total)>> MaterializeContinuousLocationCountsAsync(
        IReadOnlyList<int> listIds,
        IReadOnlyList<RatingCustomerFact> facts,
        DateOnly asOf,
        CancellationToken ct)
    {
        var continuousByList = facts
            .Where(f => f.OrderListId is > 0 && listIds.Contains(f.OrderListId.Value))
            .Where(f => DelegateContinuousCustomerRules.IsContinuous(f, asOf))
            .GroupBy(f => f.OrderListId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.CustomerId).Distinct().ToHashSet());

        var allCustomerIds = continuousByList.Values.SelectMany(x => x).Distinct().ToList();
        if (allCustomerIds.Count == 0)
        {
            return listIds.ToDictionary(id => id, _ => (0, 0));
        }

        var customerGps = await _db.Customers.AsNoTracking()
            .Where(c => allCustomerIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                HasGps = c.Business.GpsLatitude != null
                         && c.Business.GpsLongitude != null
                         && !(c.Business.GpsLatitude == 0 && c.Business.GpsLongitude == 0)
            })
            .ToDictionaryAsync(x => x.Id, x => x.HasGps, ct);

        var orderGpsRows = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value)
                        && allCustomerIds.Contains(o.CustomerId)
                        && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(o => new
            {
                ListId = o.OrderListId!.Value,
                o.CustomerId,
                HasGps = o.Location != null
                         && !(o.Location.Latitude == 0 && o.Location.Longitude == 0)
            })
            .ToListAsync(ct);

        var orderGpsByListCustomer = orderGpsRows
            .GroupBy(x => (x.ListId, x.CustomerId))
            .ToDictionary(g => g.Key, g => g.Any(x => x.HasGps));

        var result = new Dictionary<int, (int Located, int Total)>();
        foreach (var listId in listIds)
        {
            if (!continuousByList.TryGetValue(listId, out var customers) || customers.Count == 0)
            {
                result[listId] = (0, 0);
                continue;
            }

            var located = customers.Count(customerId =>
            {
                var fromCustomer = customerGps.TryGetValue(customerId, out var cg) && cg;
                var fromOrder = orderGpsByListCustomer.TryGetValue((listId, customerId), out var og) && og;
                return fromCustomer || fromOrder;
            });

            result[listId] = (located, customers.Count);
        }

        return result;
    }

    private async Task<List<RatingCustomerFact>> MaterializeFactsForListsAsync(
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
                o.SellerId,
                o.SellAmount,
                o.DailyInstallmentAmount,
                o.SaleDate,
                o.CreatedAt,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
                IsLegal = o.Customer != null && o.Customer.IsLegal
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

                return new RatingCustomerFact
                {
                    CustomerId = g.Key.CustomerId,
                    OrderListId = g.Key.OrderListId,
                    SellerId = g.Select(x => x.SellerId).FirstOrDefault(x => x != null),
                    AmountTotalSales = g.Sum(x => x.SellAmount),
                    ReceiptsTotal = g.Sum(x => x.Receipts),
                    AmountDaySales = g.Sum(x => x.DailyInstallmentAmount),
                    SaleDate = saleDate,
                    LastPaymentDate = g.Max(x => x.LastPaymentDate),
                    IsLegal = g.Any(x => x.IsLegal)
                };
            })
            .ToList();
    }
}
