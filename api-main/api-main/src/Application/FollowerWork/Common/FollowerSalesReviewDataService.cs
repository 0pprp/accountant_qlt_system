using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerSalesReviewDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public FollowerSalesReviewDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public async Task<Result<(int FollowerId, string Name, IReadOnlyList<int> ListIds, IReadOnlyList<int> BranchIds)>>
        ResolveFollowerScopeAsync(
            int followerId,
            IReadOnlyList<int> authorizedBranches,
            int? branchId,
            CancellationToken ct)
    {
        if (followerId <= 0)
            return FollowerWorkErrors.FollowerNotFound;

        var branchScope = branchId is > 0
            ? authorizedBranches.Contains(branchId.Value) ? new[] { branchId.Value } : Array.Empty<int>()
            : authorizedBranches.ToArray();
        if (branchScope.Length == 0)
            return FollowerWorkErrors.BranchAccessDenied;

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(l => l.MotabaId == followerId && branchScope.Contains(l.BranchId))
            .Select(l => new { l.Id, l.BranchId, Name = l.Motaba != null ? l.Motaba.FullName : null })
            .ToListAsync(ct);

        if (lists.Count == 0)
        {
            // متابع بلا تكليف في النطاق — لا نسرّب هوية من فرع آخر
            var existsElsewhere = await _db.OrderLists.AsNoTracking()
                .AnyAsync(l => l.MotabaId == followerId, ct);
            return existsElsewhere
                ? FollowerWorkErrors.FollowerBranchMismatch
                : FollowerWorkErrors.FollowerNotFound;
        }

        var name = lists.Select(x => x.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                   ?? $"متابع {followerId}";
        return (followerId, name, lists.Select(x => x.Id).Distinct().ToList(),
            lists.Select(x => x.BranchId).Distinct().ToList());
    }

    public async Task<Result<FollowerSalesReviewWorkspaceDto>> GetSalesReviewAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        CancellationToken ct)
    {
        var scope = await ResolveFollowerScopeAsync(followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, followerName, listIds, _) = scope.Data!;
        var asOf = TodayBaghdad();
        var weekAgo = asOf.AddDays(-7);
        var yesterday = asOf.AddDays(-1);

        var listMeta = await _db.OrderLists.AsNoTracking()
            .Where(l => listIds.Contains(l.Id))
            .Select(l => new { l.Id, l.Name })
            .ToListAsync(ct);

        var facts = await MaterializeFactsForListsAsync(listIds, ct);
        var factsByList = facts
            .Where(f => f.OrderListId is > 0)
            .GroupBy(f => f.OrderListId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var listCards = new List<FollowerSalesReviewListCardDto>(listMeta.Count);
        foreach (var list in listMeta.OrderBy(l => l.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            factsByList.TryGetValue(list.Id, out var listFacts);
            listFacts ??= [];
            var today = ActiveRatingCohort.Aggregate(listFacts, asOf, usePhantomSaleDate: true);
            var week = ActiveRatingCohort.Aggregate(listFacts, weekAgo, usePhantomSaleDate: true);
            listCards.Add(new FollowerSalesReviewListCardDto(
                list.Id,
                list.Name,
                Math.Round(today.AverageScore, 2, MidpointRounding.AwayFromZero),
                today.FinalRatingLabel,
                Math.Round(week.AverageScore, 2, MidpointRounding.AwayFromZero),
                FollowerWorkRules.ComputeScoreTrend(today.AverageScore, week.AverageScore),
                today.CustomerCount));
        }

        var activeToday = ActiveRatingCohort.Build(facts, asOf, usePhantomSaleDate: true);
        var activeYesterday = ActiveRatingCohort.Build(facts, yesterday, usePhantomSaleDate: true);
        var buckets = BuildRatingBuckets(activeToday, activeYesterday);

        var remToday = facts.Where(f => ActiveCustomerRules.IsActive(f.LastPaymentDate, asOf) && f.HasOpenBalance).ToList();
        var remY = facts.Where(f => ActiveCustomerRules.IsActive(f.LastPaymentDate, yesterday) && f.HasOpenBalance).ToList();
        var le100 = remToday.Count(f => f.AmountRemaining > 0 && f.AmountRemaining < FollowerSaleAuthRules.RemainingBand100k);
        var lt200 = remToday.Count(f => f.AmountRemaining >= FollowerSaleAuthRules.RemainingBand100k
                                        && f.AmountRemaining < FollowerSaleAuthRules.RemainingBand200k);
        var le100Y = remY.Count(f => f.AmountRemaining > 0 && f.AmountRemaining < FollowerSaleAuthRules.RemainingBand100k);
        var lt200Y = remY.Count(f => f.AmountRemaining >= FollowerSaleAuthRules.RemainingBand100k
                                     && f.AmountRemaining < FollowerSaleAuthRules.RemainingBand200k);

        var salesRows = await LoadSalesRowsAsync(followerId, listIds, asOf, ct);
        var pending = salesRows.Count(r => r.Authenticated == false);
        var done = salesRows.Count(r => r.Authenticated);

        return new FollowerSalesReviewWorkspaceDto(
            new FollowerSalesReviewIdentityDto(followerId, followerName),
            listCards,
            buckets,
            activeToday.Count,
            le100,
            lt200,
            FollowerWorkRules.ComputeCountTrend(le100, le100Y),
            FollowerWorkRules.ComputeCountTrend(lt200, lt200Y),
            pending,
            done,
            salesRows,
            FollowerWorkRules.TimeZoneId,
            asOf.ToString("yyyy-MM-dd"),
            _clock.UtcNowOffset);
    }

    public async Task<Result<FollowerRemainingCustomersResponse>> GetRemainingCustomersAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        string bucket,
        CancellationToken ct)
    {
        if (FollowerSaleAuthRules.TryNormalizeBucket(bucket, out var normalized) == false
            && string.IsNullOrWhiteSpace(bucket) == false)
            return new Error("bucket غير مسموح", "FollowerSalesReview.InvalidBucket");

        var scope = await ResolveFollowerScopeAsync(followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, _, listIds, _) = scope.Data!;
        var asOf = TodayBaghdad();
        var facts = await MaterializeFactsForListsAsync(listIds, ct);
        var listNames = await _db.OrderLists.AsNoTracking()
            .Where(l => listIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var customerNames = await _db.Customers.AsNoTracking()
            .Where(c => facts.Select(f => f.CustomerId).Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToListAsync(ct);
        var nameMap = customerNames.ToDictionary(c => c.Id);

        IEnumerable<(RatingCustomerFact Fact, int Score, string Level)> pairs =
            ActiveRatingCohort.Build(facts, asOf, usePhantomSaleDate: true);

        pairs = normalized switch
        {
            "excellent" => pairs.Where(x => x.Level == "ممتاز"),
            "good" => pairs.Where(x => x.Level == "جيد"),
            "weak" => pairs.Where(x => x.Level == "ضعيف"),
            "lagging" => pairs.Where(x => x.Level == "متلكئ"),
            "bad" => pairs.Where(x => x.Level == "سيء"),
            "le100" => pairs.Where(x => x.Fact.AmountRemaining > 0
                                        && x.Fact.AmountRemaining < FollowerSaleAuthRules.RemainingBand100k),
            "lt200" => pairs.Where(x => x.Fact.AmountRemaining >= FollowerSaleAuthRules.RemainingBand100k
                                        && x.Fact.AmountRemaining < FollowerSaleAuthRules.RemainingBand200k),
            _ => pairs
        };

        var items = pairs
            .Select(x =>
            {
                nameMap.TryGetValue(x.Fact.CustomerId, out var info);
                var listId = x.Fact.OrderListId ?? 0;
                listNames.TryGetValue(listId, out var listName);
                return new FollowerRemainingCustomerDto(
                    x.Fact.CustomerId,
                    info?.FullName ?? $"زبون {x.Fact.CustomerId}",
                    info?.PhoneNumber,
                    listId,
                    listName,
                    x.Fact.AmountRemaining,
                    x.Level);
            })
            .OrderBy(x => x.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new FollowerRemainingCustomersResponse(
            normalized,
            items,
            items.Count,
            FollowerWorkRules.TimeZoneId,
            asOf.ToString("yyyy-MM-dd"),
            _clock.UtcNowOffset);
    }

    private async Task<List<FollowerSalesReviewSaleRowDto>> LoadSalesRowsAsync(
        int followerId,
        IReadOnlyList<int> listIds,
        DateOnly asOf,
        CancellationToken ct)
    {
        if (listIds.Count == 0)
            return [];

        var listNames = await _db.OrderLists.AsNoTracking()
            .Where(l => listIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var auths = await _db.FollowerSaleAuthentications.AsNoTracking()
            .Where(a => listIds.Contains(a.OrderListId))
            .OrderByDescending(a => a.AuthenticatedAtUtc)
            .ToListAsync(ct);

        // وحدة السجل = OrderId — أحدث مصادقة لكل طلب
        var authByOrder = auths
            .GroupBy(a => a.OrderId)
            .ToDictionary(g => g.Key, g => g.First());

        var authOrderIds = authByOrder.Keys.ToList();
        var authOrders = await _db.Orders.AsNoTracking()
            .Where(o => authOrderIds.Contains(o.Id))
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FullName : "",
                Phone = o.Customer != null ? o.Customer.PhoneNumber : null,
                o.OrderListId,
                o.SellAmount,
                o.SaleDate,
                o.DailyInstallmentAmount,
                SellerName = o.Seller != null ? o.Seller.FullName : null,
                Paid = o.InstallmentPayments.Sum(p => p.Amount),
            })
            .ToListAsync(ct);

        var pendingOrders = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value)
                        && o.ExecutionStatus == OrderExecutionStatus.NotStarted
                        && authOrderIds.Contains(o.Id) == false)
            .OrderByDescending(o => o.CreatedAt)
            .Take(300)
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FullName : "",
                Phone = o.Customer != null ? o.Customer.PhoneNumber : null,
                o.OrderListId,
                o.SellAmount,
                o.SaleDate,
                o.DailyInstallmentAmount,
                SellerName = o.Seller != null ? o.Seller.FullName : null,
                Paid = o.InstallmentPayments.Sum(p => p.Amount),
            })
            .ToListAsync(ct);

        var allCustomerIds = authOrders.Select(o => o.CustomerId)
            .Concat(pendingOrders.Select(o => o.CustomerId))
            .Distinct()
            .ToList();
        var facts = await MaterializeFactsForCustomersAsync(allCustomerIds, listIds, ct);
        var factByCustomer = facts.GroupBy(f => f.CustomerId)
            .ToDictionary(g => g.Key, g => g.First());

        var rows = new List<FollowerSalesReviewSaleRowDto>();

        foreach (var o in authOrders)
        {
            authByOrder.TryGetValue(o.Id, out var auth);
            factByCustomer.TryGetValue(o.CustomerId, out var fact);
            var listId = o.OrderListId ?? auth?.OrderListId ?? 0;
            listNames.TryGetValue(listId, out var listName);
            var remaining = fact?.AmountRemaining ?? Math.Max(0, o.SellAmount - o.Paid);
            string? ratingLabel = null;
            if (fact is not null)
            {
                var score = CustomerRatingCalculator.ScoreFor(
                    fact.AmountTotalSales, fact.ReceiptsTotal, fact.AmountRemaining,
                    fact.SaleDate, fact.LastPaymentDate, asOf, usePhantomSaleDate: true);
                if (score is not null)
                    ratingLabel = CustomerRatingCalculator.LevelName(score.Value);
            }

            rows.Add(new FollowerSalesReviewSaleRowDto(
                o.Id,
                o.CustomerId,
                string.IsNullOrWhiteSpace(o.CustomerName) ? $"زبون {o.CustomerId}" : o.CustomerName,
                o.Phone,
                listId,
                listName,
                true,
                auth?.AuthenticatedAtUtc,
                auth?.Rating,
                auth?.Note,
                auth?.ShopStockEstimatedValue,
                auth?.EstimatedDailyRevenue,
                null, null, null, null, null, null,
                ratingLabel,
                o.SellAmount,
                o.SaleDate,
                o.DailyInstallmentAmount,
                o.SellerName,
                o.Paid,
                remaining,
                FollowerSaleAuthRules.IsLowAuthRisk(auth?.Rating)));
        }

        foreach (var o in pendingOrders)
        {
            var listId = o.OrderListId ?? 0;
            listNames.TryGetValue(listId, out var listName);
            factByCustomer.TryGetValue(o.CustomerId, out var fact);
            var remaining = fact?.AmountRemaining ?? Math.Max(0, o.SellAmount - o.Paid);
            string? ratingLabel = null;
            if (fact is not null)
            {
                var score = CustomerRatingCalculator.ScoreFor(
                    fact.AmountTotalSales, fact.ReceiptsTotal, fact.AmountRemaining,
                    fact.SaleDate, fact.LastPaymentDate, asOf, usePhantomSaleDate: true);
                if (score is not null)
                    ratingLabel = CustomerRatingCalculator.LevelName(score.Value);
            }

            rows.Add(new FollowerSalesReviewSaleRowDto(
                o.Id, o.CustomerId,
                string.IsNullOrWhiteSpace(o.CustomerName) ? $"زبون {o.CustomerId}" : o.CustomerName,
                o.Phone, listId, listName,
                false, null, null, null, null, null,
                null, null, null, null, null, null,
                ratingLabel, o.SellAmount, o.SaleDate, o.DailyInstallmentAmount, o.SellerName,
                o.Paid, remaining, false));
        }

        return rows
            .OrderByDescending(r => r.Authenticated)
            .ThenByDescending(r => r.AuthenticatedAtUtc)
            .ThenByDescending(r => r.SaleDate)
            .ToList();
    }

    private static List<FollowerSalesReviewRatingBucketDto> BuildRatingBuckets(
        IReadOnlyList<(RatingCustomerFact Fact, int Score, string Level)> today,
        IReadOnlyList<(RatingCustomerFact Fact, int Score, string Level)> yesterday)
    {
        static int Count(IReadOnlyList<(RatingCustomerFact Fact, int Score, string Level)> rows, string level)
            => rows.Count(x => x.Level == level);

        (string Key, string Label, string Level)[] defs =
        [
            ("excellent", "ممتاز", "ممتاز"),
            ("good", "جيد", "جيد"),
            ("weak", "ضعيف", "ضعيف"),
            ("lagging", "متلكئ", "متلكئ"),
            ("bad", "سيء", "سيء"),
        ];

        return defs.Select(d =>
        {
            var c = Count(today, d.Level);
            var y = Count(yesterday, d.Level);
            return new FollowerSalesReviewRatingBucketDto(
                d.Key, d.Label, c, y, FollowerWorkRules.ComputeCountTrend(c, y));
        }).ToList();
    }

    private async Task<List<RatingCustomerFact>> MaterializeFactsForListsAsync(
        IReadOnlyList<int> listIds,
        CancellationToken ct)
    {
        if (listIds.Count == 0)
            return [];

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

        return GroupFacts(rows.Select(r => (
            r.CustomerId, r.OrderListId, r.SellerId, r.SellAmount, r.DailyInstallmentAmount,
            r.SaleDate, r.CreatedAt, r.Receipts, r.LastPaymentDate, r.IsLegal)));
    }

    private async Task<List<RatingCustomerFact>> MaterializeFactsForCustomersAsync(
        IReadOnlyList<int> customerIds,
        IReadOnlyList<int> listIds,
        CancellationToken ct)
    {
        if (customerIds.Count == 0)
            return [];

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && customerIds.Contains(o.CustomerId)
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

        return GroupFacts(rows.Select(r => (
            r.CustomerId, r.OrderListId, r.SellerId, r.SellAmount, r.DailyInstallmentAmount,
            r.SaleDate, r.CreatedAt, r.Receipts, r.LastPaymentDate, r.IsLegal)));
    }

    private static List<RatingCustomerFact> GroupFacts(
        IEnumerable<(int CustomerId, int? OrderListId, int? SellerId, double SellAmount, double Daily,
            DateOnly? SaleDate, DateTimeOffset? CreatedAt, double Receipts, DateOnly? LastPayment, bool IsLegal)> rows)
    {
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
                    AmountDaySales = g.Sum(x => x.Daily),
                    SaleDate = saleDate,
                    LastPaymentDate = g.Max(x => x.LastPayment),
                    IsLegal = g.Any(x => x.IsLegal)
                };
            })
            .ToList();
    }
}
