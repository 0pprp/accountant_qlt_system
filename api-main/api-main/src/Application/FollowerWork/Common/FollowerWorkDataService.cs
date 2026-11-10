using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerWorkDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public FollowerWorkDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public async Task<Result<FollowerWorkFollowersResponse>> GetFollowersAsync(
        IReadOnlyList<int> branchScope,
        string? search,
        int page,
        int pageSize,
        string sort,
        bool includeMetrics,
        CancellationToken ct)
    {
        if (branchScope.Count == 0)
            return FollowerWorkErrors.BranchAccessDenied;

        var asOf = TodayBaghdad();
        var weekAgo = FollowerWorkRules.WeekAgo(asOf);

        var assignmentRows = await _db.OrderLists.AsNoTracking()
            .Where(l => branchScope.Contains(l.BranchId) && l.MotabaId > 0)
            .Select(l => new
            {
                l.Id,
                l.MotabaId,
                l.BranchId,
                BranchName = l.Branch != null ? l.Branch.Name : "",
                MotabaName = l.Motaba != null ? l.Motaba.FullName : null
            })
            .ToListAsync(ct);

        var groups = assignmentRows
            .GroupBy(x => x.MotabaId)
            .Select(g =>
            {
                var name = g.Select(x => x.MotabaName)
                                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                            ?? $"متابع {g.Key}";
                var branches = g
                    .GroupBy(x => x.BranchId)
                    .Select(b => new FollowerWorkBranchRefDto(
                        b.Key,
                        b.Select(x => x.BranchName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                        ?? $"فرع {b.Key}"))
                    .OrderBy(b => b.BranchName, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
                var listIds = g.Select(x => x.Id).Distinct().ToList();
                return new FollowerAgg(
                    g.Key,
                    name,
                    branches,
                    listIds);
            })
            .ToList();

        if (string.IsNullOrWhiteSpace(search) == false)
        {
            var q = search.Trim();
            groups = groups
                .Where(g => g.Name.Contains(q, StringComparison.CurrentCultureIgnoreCase)
                            || g.FollowerId.ToString() == q)
                .ToList();
        }

        Dictionary<int, CardMetricScratch>? metricScratch = null;
        var sortByRating = sort.Equals("rating", StringComparison.OrdinalIgnoreCase);
        if (includeMetrics && sortByRating)
            metricScratch = await ComputeMetricsBatchAsync(groups, asOf, weekAgo, ct);

        IEnumerable<FollowerAgg> ordered = sort.ToLowerInvariant() switch
        {
            "lists" => groups
                .OrderByDescending(g => g.ListIds.Count)
                .ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase),
            "rating" when metricScratch is not null => groups
                .OrderByDescending(g => metricScratch.TryGetValue(g.FollowerId, out var m)
                    ? m.PhantomRating ?? double.MinValue
                    : double.MinValue)
                .ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase),
            _ => groups.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase),
        };

        var all = ordered.ToList();
        var totalCount = all.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var pageItems = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        if (includeMetrics && pageItems.Count > 0)
        {
            if (metricScratch is null)
                metricScratch = await ComputeMetricsBatchAsync(pageItems, asOf, weekAgo, ct);
        }

        var cards = new List<FollowerWorkFollowerCardDto>(pageItems.Count);
        foreach (var item in pageItems)
        {
            FollowerWorkCardMetricsDto? metrics = null;
            if (includeMetrics)
            {
                if (metricScratch is not null
                    && metricScratch.TryGetValue(item.FollowerId, out var scratch)
                    && scratch.Availability == "available")
                {
                    metrics = new FollowerWorkCardMetricsDto(
                        scratch.PhantomRating,
                        scratch.PhantomRatingLabel,
                        scratch.PhantomRatingWeekAgo,
                        scratch.ScoreTrend,
                        scratch.RatedCustomerCount,
                        scratch.RatedCustomerCountWeekAgo,
                        scratch.CountTrend,
                        "available");
                }
                else
                {
                    metrics = new FollowerWorkCardMetricsDto(
                        null, null, null, null, null, null, null, "unavailable");
                }
            }

            cards.Add(new FollowerWorkFollowerCardDto(
                item.FollowerId,
                item.Name,
                item.Branches,
                item.ListIds.Count,
                metrics));
        }

        return new FollowerWorkFollowersResponse(
            cards,
            page,
            pageSize,
            totalCount,
            totalPages,
            FollowerWorkRules.TimeZoneId,
            asOf.ToString("yyyy-MM-dd"),
            _clock.UtcNowOffset);
    }

    /// <summary>
    /// تحقق علاقة متابع↔فرع عبر تكليف قائمة فعلي — ليس مجرد وجود الحساب أو فرع مسموح منفردًا.
    /// </summary>
    public async Task<Error?> ValidateFollowerInBranchAsync(
        int followerId,
        int branchId,
        IReadOnlyList<int> authorizedBranches,
        CancellationToken ct)
    {
        if (followerId <= 0)
            return FollowerWorkErrors.FollowerNotFound;
        if (authorizedBranches.Contains(branchId) == false)
            return FollowerWorkErrors.BranchAccessDenied;

        var linked = await _db.OrderLists.AsNoTracking()
            .AnyAsync(l => l.MotabaId == followerId && l.BranchId == branchId, ct);
        if (linked == false)
            return FollowerWorkErrors.FollowerBranchMismatch;

        return null;
    }

    private async Task<Dictionary<int, CardMetricScratch>> ComputeMetricsBatchAsync(
        IReadOnlyList<FollowerAgg> followers,
        DateOnly asOf,
        DateOnly weekAgo,
        CancellationToken ct)
    {
        var result = new Dictionary<int, CardMetricScratch>();
        if (followers.Count == 0)
            return result;

        var allListIds = followers.SelectMany(f => f.ListIds).Distinct().ToList();
        if (allListIds.Count == 0)
        {
            foreach (var f in followers)
                result[f.FollowerId] = CardMetricScratch.Unavailable();
            return result;
        }

        List<RatingCustomerFact> facts;
        try
        {
            facts = await MaterializeFactsForListsAsync(allListIds, ct);
        }
        catch
        {
            foreach (var f in followers)
                result[f.FollowerId] = CardMetricScratch.Unavailable();
            return result;
        }

        var factsByList = facts
            .Where(f => f.OrderListId is > 0)
            .GroupBy(f => f.OrderListId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var f in followers)
        {
            try
            {
                var listScores = new List<double>(f.ListIds.Count);
                var weekScores = new List<double>(f.ListIds.Count);
                var ratedToday = 0;
                var ratedWeek = 0;

                foreach (var listId in f.ListIds)
                {
                    factsByList.TryGetValue(listId, out var listFacts);
                    listFacts ??= [];

                    var todayAgg = ActiveRatingCohort.Aggregate(listFacts, asOf, usePhantomSaleDate: true);
                    listScores.Add(todayAgg.AverageScore);
                    ratedToday += todayAgg.CustomerCount;

                    var weekAgg = ActiveRatingCohort.Aggregate(listFacts, weekAgo, usePhantomSaleDate: true);
                    weekScores.Add(weekAgg.AverageScore);
                    ratedWeek += weekAgg.CustomerCount;
                }

                if (listScores.Count == 0)
                {
                    result[f.FollowerId] = CardMetricScratch.Unavailable();
                    continue;
                }

                var avg = listScores.Average();
                var weekAvg = weekScores.Average();
                result[f.FollowerId] = new CardMetricScratch(
                    Math.Round(avg, 2, MidpointRounding.AwayFromZero),
                    RatingAggregator.ClassifyAverageLabel(avg),
                    Math.Round(weekAvg, 2, MidpointRounding.AwayFromZero),
                    FollowerWorkRules.ComputeScoreTrend(avg, weekAvg),
                    ratedToday,
                    ratedWeek,
                    FollowerWorkRules.ComputeCountTrend(ratedToday, ratedWeek),
                    "available");
            }
            catch
            {
                // لا تسقط البطاقة — المؤشر unavailable
                result[f.FollowerId] = CardMetricScratch.Unavailable();
            }
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

    private sealed record FollowerAgg(
        int FollowerId,
        string Name,
        IReadOnlyList<FollowerWorkBranchRefDto> Branches,
        IReadOnlyList<int> ListIds);

    private sealed record CardMetricScratch(
        double? PhantomRating,
        string? PhantomRatingLabel,
        double? PhantomRatingWeekAgo,
        string? ScoreTrend,
        int? RatedCustomerCount,
        int? RatedCustomerCountWeekAgo,
        string? CountTrend,
        string Availability)
    {
        public static CardMetricScratch Unavailable() =>
            new(null, null, null, null, null, null, null, "unavailable");
    }
}
