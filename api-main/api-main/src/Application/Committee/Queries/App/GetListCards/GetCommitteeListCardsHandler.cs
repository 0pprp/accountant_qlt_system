using Application.Common.Interfaces;
using Application.FollowerWork.Common;
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
            .Where(x => request.ListId == null || x.Id == request.ListId)
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
        var captureByList = (await ShopCaptureProgressQuery.LoadAsync(_db, listIds, cancellationToken))
            .GroupBy(r => r.ListId)
            .ToDictionary(g => g.Key, g => g.ToList());
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
            captureByList.TryGetValue(list.Id, out var capture);
            capture ??= [];

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
                LocatedCustomers = capture.Count(r => r.HasLocation),
                PhotographedCustomers = capture.Count(r => r.HasPhoto),
                CompletedCustomers = capture.Count(r => r.IsComplete),
                LocationCustomers = request.ListId.HasValue
                    ? capture
                        .OrderBy(r => r.IsComplete ? 1 : 0)
                        .ThenBy(r => r.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                        .ThenBy(r => r.CustomerId)
                        .Select(r => new CommitteeLocationCustomer(
                            r.CustomerId, r.CustomerName, r.PhoneNumber ?? "", r.HasLocation, r.HasPhoto, r.ShopName))
                        .ToList()
                    : null,
                LocationCustomersTotal = capture.Count
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

    private async Task<List<ListCardFact>> MaterializeFactsForListsAsync(
        IReadOnlyList<int> listIds,
        CancellationToken ct)
        => (await FollowerFactsQuery.LoadAsync(_db, listIds, null, null, ct))
            .Select(f => new ListCardFact
            {
                CustomerId = f.CustomerId,
                OrderListId = f.OrderListId,
                AmountTotalSales = f.AmountTotalSales,
                ReceiptsTotal = f.ReceiptsTotal,
                AmountRemaining = f.AmountRemaining,
                SaleDate = f.SaleDate,
                LastPaymentDate = f.LastPaymentDate,
                IsLegal = f.IsLegal
            })
            .ToList();

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
    }
}
