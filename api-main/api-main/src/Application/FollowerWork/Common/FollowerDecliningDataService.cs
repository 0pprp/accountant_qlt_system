using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.FollowerWorkAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

/// <summary>
/// قراءة/تحديث لقطات التقييم لمجموعة المتدنية ضمن تكليف Motaba الحالي.
/// لا يخترع تاريخ تقييمات؛ اليوم يُلتقط حيًا، والمقارنة مع لقطات مخزّنة سابقة فقط.
/// </summary>
public class FollowerDecliningDataService
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;
    public const string ComparisonModeStoredSnapshots = "stored_snapshots";
    public const string NotesAvailabilityCustomerNotes = "customer_notes";

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly FollowerSalesReviewDataService _salesReviewScope;

    public FollowerDecliningDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        FollowerSalesReviewDataService salesReviewScope)
    {
        _db = db;
        _clock = clock;
        _salesReviewScope = salesReviewScope;
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public async Task<Result<FollowerDecliningResponse>> GetDecliningForAdminAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var scope = await _salesReviewScope.ResolveFollowerScopeAsync(
            followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, followerName, listIds, _) = scope.Data!;
        return await BuildResponseAsync(followerId, followerName, listIds, page, pageSize, captureToday: true, ct);
    }

    /// <summary>مسار تطبيق المتابع: قوائم Motaba فقط (بدون Mandob).</summary>
    public async Task<Result<FollowerDecliningResponse>> GetDecliningForMotabaAsync(
        int motabaUserId,
        CancellationToken ct)
    {
        if (motabaUserId <= 0)
            return FollowerWorkErrors.FollowerNotFound;

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(l => l.MotabaId == motabaUserId)
            .Select(l => new { l.Id, Name = l.Motaba != null ? l.Motaba.FullName : null })
            .ToListAsync(ct);

        if (lists.Count == 0)
            return new FollowerDecliningResponse(
                [], 0, 1, DefaultPageSize, 0,
                "هبوط درجتين (−10) خلال 7 أيام على انتقالات محددة",
                FollowerDeclineRules.LookbackDays,
                FollowerDeclineRules.MinScoreDrop,
                TargetTransitions(),
                ComparisonModeStoredSnapshots,
                NotesAvailabilityCustomerNotes,
                FollowerWorkRules.TimeZoneId,
                TodayBaghdad().ToString("yyyy-MM-dd"),
                _clock.UtcNowOffset,
                null,
                motabaUserId);

        var name = lists.Select(x => x.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                   ?? $"متابع {motabaUserId}";
        var listIds = lists.Select(x => x.Id).Distinct().ToList();
        return await BuildResponseAsync(
            motabaUserId, name, listIds, 1, MaxPageSize, captureToday: true, ct);
    }

    private async Task<Result<FollowerDecliningResponse>> BuildResponseAsync(
        int followerId,
        string followerName,
        IReadOnlyList<int> listIds,
        int page,
        int pageSize,
        bool captureToday,
        CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1
            ? DefaultPageSize
            : Math.Min(pageSize, MaxPageSize);

        var asOf = TodayBaghdad();
        if (captureToday)
            await CaptureTodaySnapshotsAsync(listIds, asOf, ct);

        var rows = await LoadDecliningRowsAsync(listIds, asOf, ct);
        var totalCount = rows.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var pageItems = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new FollowerDecliningResponse(
            pageItems,
            totalCount,
            page,
            pageSize,
            totalPages,
            "هبوط درجتين (−10) خلال 7 أيام — ممتاز→ضعيف، جيد→متلكئ، ضعيف→سيء",
            FollowerDeclineRules.LookbackDays,
            FollowerDeclineRules.MinScoreDrop,
            TargetTransitions(),
            ComparisonModeStoredSnapshots,
            NotesAvailabilityCustomerNotes,
            FollowerWorkRules.TimeZoneId,
            asOf.ToString("yyyy-MM-dd"),
            _clock.UtcNowOffset,
            followerName,
            followerId);
    }

    private static IReadOnlyList<string> TargetTransitions() =>
    [
        "ممتاز→ضعيف",
        "جيد→متلكئ",
        "ضعيف→سيء"
    ];

    /// <summary>
    /// يلتقط تقييم اليوم للمجموعة النشطة (رصيد مفتوح + تسديد ≤30ي) ويحدّث لقطة اليوم فقط.
    /// لا يولّد أيامًا سابقة.
    /// </summary>
    public async Task CaptureTodaySnapshotsAsync(
        IReadOnlyList<int> listIds,
        DateOnly asOf,
        CancellationToken ct)
    {
        if (listIds.Count == 0)
            return;

        var facts = await MaterializeFactsForListsAsync(listIds, ct);
        var customerIds = facts.Select(f => f.CustomerId).Distinct().ToList();
        var names = await _db.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToDictionaryAsync(c => c.Id, ct);

        var existingToday = await _db.FollowerCustomerRatingSnapshots
            .Where(s => s.CaptureDate == asOf && customerIds.Contains(s.CustomerId))
            .ToListAsync(ct);
        var byCustomer = existingToday.ToDictionary(s => s.CustomerId);

        var now = _clock.UtcNowOffset;

        foreach (var fact in facts)
        {
            if (fact.HasOpenBalance == false)
                continue;
            if (ActiveCustomerRules.IsActive(fact.LastPaymentDate, asOf) == false)
                continue;
            if (fact.OrderListId is not > 0)
                continue;

            var score = CustomerRatingCalculator.ScoreFor(
                fact.AmountTotalSales,
                fact.ReceiptsTotal,
                fact.AmountRemaining,
                fact.SaleDate,
                fact.LastPaymentDate,
                asOf,
                usePhantomSaleDate: true);
            if (score is null)
                continue;

            var label = FollowerDeclineRules.LabelForScore(score.Value);
            names.TryGetValue(fact.CustomerId, out var info);
            var listId = fact.OrderListId.Value;

            if (byCustomer.TryGetValue(fact.CustomerId, out var row))
            {
                row.ReplaceCapture(
                    listId,
                    info?.FullName,
                    info?.PhoneNumber,
                    label,
                    score.Value,
                    now);
            }
            else
            {
                var snap = new FollowerCustomerRatingSnapshot(
                    fact.CustomerId,
                    listId,
                    info?.FullName,
                    info?.PhoneNumber,
                    label,
                    score.Value,
                    asOf,
                    now);
                _db.FollowerCustomerRatingSnapshots.Add(snap);
                byCustomer[fact.CustomerId] = snap;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<List<FollowerDecliningRowDto>> LoadDecliningRowsAsync(
        IReadOnlyList<int> listIds,
        DateOnly asOf,
        CancellationToken ct)
    {
        if (listIds.Count == 0)
            return [];

        var fromDate = asOf.AddDays(-FollowerDeclineRules.LookbackDays);

        // أحدث لقطة لكل زبون على قوائم النطاق (قد تكون اليوم أو أقدم إن لم يُلتقط اليوم)
        var snapshots = await _db.FollowerCustomerRatingSnapshots.AsNoTracking()
            .Where(s => listIds.Contains(s.OrderListId)
                        && s.CaptureDate >= fromDate
                        && s.CaptureDate <= asOf)
            .Select(s => new
            {
                s.CustomerId,
                s.OrderListId,
                s.CustomerName,
                s.PhoneNumber,
                s.RatingLabel,
                s.Score,
                s.CaptureDate,
                s.CapturedAtUtc
            })
            .ToListAsync(ct);

        if (snapshots.Count == 0)
            return [];

        var current = snapshots
            .GroupBy(s => s.CustomerId)
            .Select(g => g
                .OrderByDescending(x => x.CaptureDate)
                .ThenByDescending(x => x.CapturedAtUtc)
                .First())
            .ToList();

        var priorLookup = snapshots
            .GroupBy(s => s.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.CaptureDate)
                    .ThenByDescending(x => x.CapturedAtUtc)
                    .ToList());

        var listNames = await _db.OrderLists.AsNoTracking()
            .Where(l => listIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var customerIds = current.Select(c => c.CustomerId).ToList();
        var authSet = await _db.FollowerSaleAuthentications.AsNoTracking()
            .Where(a => listIds.Contains(a.OrderListId) && customerIds.Contains(a.CustomerId))
            .Select(a => a.CustomerId)
            .Distinct()
            .ToListAsync(ct);
        var authHash = authSet.ToHashSet();

        // HasNote: أي CustomerNotes على الزبون (مطابقة LoadLatestNotesAsync) — مجمع لا N+1
        var noteTimes = await _db.CustomerNotes.AsNoTracking()
            .Where(n => customerIds.Contains(n.CustomerId))
            .GroupBy(n => n.CustomerId)
            .Select(g => new { CustomerId = g.Key, At = g.Max(x => (DateTimeOffset?)x.CreatedAtUtc) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.At, ct);

        var custMeta = await _db.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToDictionaryAsync(c => c.Id, ct);

        var result = new List<FollowerDecliningRowDto>();
        foreach (var cur in current)
        {
            if (priorLookup.TryGetValue(cur.CustomerId, out var hist) == false)
                continue;

            var prior = hist.FirstOrDefault(h =>
                h.CaptureDate < cur.CaptureDate && h.CaptureDate >= fromDate);
            if (prior is null)
                continue;

            var days = cur.CaptureDate.DayNumber - prior.CaptureDate.DayNumber;
            if (FollowerDeclineRules.IsDeclining(
                    prior.RatingLabel, prior.Score, cur.RatingLabel, cur.Score, days) == false)
                continue;

            custMeta.TryGetValue(cur.CustomerId, out var meta);
            listNames.TryGetValue(cur.OrderListId, out var listName);
            var name = !string.IsNullOrWhiteSpace(cur.CustomerName)
                ? cur.CustomerName!
                : meta?.FullName ?? $"زبون {cur.CustomerId}";
            var phone = !string.IsNullOrWhiteSpace(cur.PhoneNumber)
                ? cur.PhoneNumber
                : meta?.PhoneNumber;

            noteTimes.TryGetValue(cur.CustomerId, out var noteAt);
            result.Add(new FollowerDecliningRowDto(
                cur.CustomerId,
                name,
                phone,
                cur.OrderListId,
                listName,
                FollowerDeclineRules.NormalizeBand(prior.RatingLabel),
                FollowerDeclineRules.NormalizeBand(cur.RatingLabel),
                FollowerDeclineRules.NormalizeBand(cur.RatingLabel),
                prior.Score,
                cur.Score,
                days,
                prior.CaptureDate,
                cur.CaptureDate,
                HasNote: noteAt.HasValue,
                LastNoteAtUtc: noteAt,
                HasSaleAuth: authHash.Contains(cur.CustomerId)));
        }

        return result
            .OrderBy(r => r.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(r => r.CustomerId)
            .ToList();
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
