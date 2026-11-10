using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementRatings.Common;

public class ManagementRatingsDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public ManagementRatingsDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public async Task<ManagementRatingsBranchesResponse> GetBranchesAsync(
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        var asOf = TodayBaghdad();
        var currentYm = asOf.Year * 100 + asOf.Month;
        var prevYm = PreviousYearMonth(currentYm);

        var branches = await _db.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync(ct);

        var cards = new List<ManagementRatingsBranchCardDto>(branches.Count);
        foreach (var b in branches)
        {
            var current = await LoadMonthBucketAsync(b.Id, currentYm, ManagementRatingsScope.Branch, 0, ct);
            var previous = await LoadMonthBucketAsync(b.Id, prevYm, ManagementRatingsScope.Branch, 0, ct);
            cards.Add(new ManagementRatingsBranchCardDto(
                b.Id, b.Name, current, previous, "Available"));
        }

        return new ManagementRatingsBranchesResponse(
            ManagementSummaryRules.TimeZoneId,
            cards,
            _clock.UtcNowOffset);
    }

    public async Task<Result<ManagementRatingsCatalogResponse>> GetCatalogAsync(
        int branchId,
        CancellationToken ct)
    {
        var branch = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => new { b.Id, b.Name })
            .FirstOrDefaultAsync(ct);
        if (branch is null)
            return ManagementRatingsErrors.BranchAccessDenied;

        var asOf = TodayBaghdad();
        var activeLists = await LoadActiveListOptionsAsync(branchId, asOf, ct);
        var activeListIds = activeLists.Select(l => l.Id).ToHashSet();

        var followerRows = await _db.OrderLists.AsNoTracking()
            .Where(l => l.BranchId == branchId && activeListIds.Contains(l.Id))
            .Select(l => new { l.Id, l.MotabaId, MotabaName = l.Motaba != null ? l.Motaba.FullName : null })
            .ToListAsync(ct);

        var followers = followerRows
            .GroupBy(x => x.MotabaId)
            .Where(g => g.Key > 0)
            .Select(g => new ManagementRatingsEntityOptionDto(
                g.Key,
                g.Select(x => x.MotabaName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? $"متابع {g.Key}",
                g.Select(x => x.Id).Distinct().ToList()))
            .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var employeeRows = await _db.Orders.AsNoTracking()
            .Where(o => o.BranchId == branchId
                        && o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.SellerId != null
                        && o.SellerId > 0)
            .Select(o => new { SellerId = o.SellerId!.Value, Name = o.Seller != null ? o.Seller.FullName : null })
            .Distinct()
            .ToListAsync(ct);

        var employees = employeeRows
            .GroupBy(x => x.SellerId)
            .Select(g => new ManagementRatingsEntityOptionDto(
                g.Key,
                g.Select(x => x.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? $"موظف {g.Key}",
                null))
            .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ManagementRatingsCatalogResponse(
            branch.Id,
            branch.Name,
            activeLists,
            followers,
            employees,
            _clock.UtcNowOffset);
    }

    public async Task<Result<ManagementRatingsPeriodSummaryResponse>> GetPeriodSummaryAsync(
        int branchId,
        ManagementRatingsScope scope,
        int resourceId,
        CancellationToken ct)
    {
        var scopeError = await ValidateResourceAsync(branchId, scope, resourceId, ct);
        if (scopeError is not null)
            return scopeError.Value;

        var asOf = TodayBaghdad();
        var currentYm = asOf.Year * 100 + asOf.Month;
        var prevYm = PreviousYearMonth(currentYm);
        var current = await LoadMonthBucketAsync(branchId, currentYm, scope, resourceId, ct);
        var previous = await LoadMonthBucketAsync(branchId, prevYm, scope, resourceId, ct);

        return new ManagementRatingsPeriodSummaryResponse(
            branchId,
            ManagementRatingsScopeParser.ToApiKey(scope),
            resourceId,
            current,
            previous,
            _clock.UtcNowOffset);
    }

    public async Task<Result<ManagementRatingsTrendResponse>> GetTrendAsync(
        int branchId,
        ManagementRatingsScope scope,
        int resourceId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct)
    {
        var scopeError = await ValidateResourceAsync(branchId, scope, resourceId, ct);
        if (scopeError is not null)
            return scopeError.Value;

        var asOf = TodayBaghdad();
        (fromDate, toDate) = RatingTrendCalculator.ClampSaleWindow(fromDate, toDate);
        var chartTo = asOf < fromDate ? fromDate : asOf;
        var chartFrom = fromDate;
        if ((chartTo.DayNumber - chartFrom.DayNumber) > RatingTrendCalculator.MaxTrendDays - 1)
            chartFrom = chartTo.AddDays(-(RatingTrendCalculator.MaxTrendDays - 1));

        var facts = await LoadFactsAsync(branchId, scope, resourceId, ct);
        facts = facts.Where(f => f.HasOpenBalance).ToList();

        var name = await ResolveResourceNameAsync(branchId, scope, resourceId, ct);
        var series = RatingTrendCalculator.DailyScores(facts, fromDate, toDate, chartFrom, chartTo);
        var periodEnd = RatingTrendCalculator.PeriodAggregate(facts, fromDate, toDate, asOf);

        var start = series.FirstOrDefault().Value;
        var endSeries = series.LastOrDefault().Value;
        var change = endSeries - start;
        double pct;
        if (Math.Abs(start) < 0.0001)
            pct = endSeries == 0 ? 0 : 100;
        else
            pct = change * 100.0 / Math.Abs(start);

        var direction = RatingAggregator.DirectionFromSeries(start, endSeries);

        // Headline: active cohort (30d) for branch/list/follower — employee keeps period cohort
        RatingAggregateResult headline;
        RatingAggregateResult phantom;
        double? installmentSum = null;
        var periodIgnored = false;

        if (scope is ManagementRatingsScope.Branch or ManagementRatingsScope.List or ManagementRatingsScope.Follower)
        {
            headline = ActiveRatingCohort.Aggregate(facts, asOf, usePhantomSaleDate: false);
            phantom = ActiveRatingCohort.Aggregate(facts, asOf, usePhantomSaleDate: true);
            periodIgnored = true;
            if (scope == ManagementRatingsScope.List)
                installmentSum = Math.Round(facts.Where(f => f.HasOpenBalance).Sum(f => f.AmountDaySales), 0, MidpointRounding.AwayFromZero);
        }
        else
        {
            headline = periodEnd;
            phantom = RatingTrendCalculator.PeriodAggregate(facts, fromDate, toDate, asOf, usePhantomSaleDate: true);
        }

        var points = series
            .Select(p => new ManagementRatingsTrendPointDto(p.Date.ToString("yyyy-MM-dd"), p.Value))
            .ToList();

        return new ManagementRatingsTrendResponse(
            ManagementRatingsScopeParser.ToApiKey(scope),
            resourceId,
            name,
            periodIgnored
                ? "تقييم الزبائن المسددين (آخر تسديد خلال 30 يوماً) — مستقل عن الفترة"
                : "متوسط نقاط زبائن تاريخ البيع — التقييم حتى اليوم",
            fromDate,
            toDate,
            chartFrom,
            chartTo,
            ManagementSummaryRules.TimeZoneId,
            points,
            Math.Round(start, 2),
            Math.Round(headline.AverageScore, 2, MidpointRounding.AwayFromZero),
            Math.Round(change, 2),
            Math.Round(pct, 2),
            direction,
            periodIgnored,
            Math.Round(periodEnd.AverageScore, 2, MidpointRounding.AwayFromZero),
            periodEnd.FinalRatingLabel,
            periodEnd.CustomerCount,
            headline.FinalRatingLabel,
            headline.CustomerCount,
            headline.ExcellentCount,
            headline.GoodCount,
            headline.WeakCount,
            headline.LaggingCount,
            headline.BadCount,
            Math.Round(phantom.AverageScore, 2, MidpointRounding.AwayFromZero),
            phantom.FinalRatingLabel,
            installmentSum,
            "Available",
            null,
            _clock.UtcNowOffset);
    }

    public async Task<Result<ManagementRatingsCustomersResponse>> GetCustomersAsync(
        int branchId,
        ManagementRatingsScope scope,
        int resourceId,
        string levelArabic,
        DateOnly fromDate,
        DateOnly toDate,
        int page,
        int pageSize,
        string? search,
        CancellationToken ct)
    {
        var scopeError = await ValidateResourceAsync(branchId, scope, resourceId, ct);
        if (scopeError is not null)
            return scopeError.Value;

        var asOf = TodayBaghdad();
        (fromDate, toDate) = RatingTrendCalculator.ClampSaleWindow(fromDate, toDate);
        var facts = await LoadFactsAsync(branchId, scope, resourceId, ct);
        facts = facts.Where(f => f.HasOpenBalance).ToList();

        List<(RatingCustomerFact Fact, int Score, string Level)> matched;
        if (scope is ManagementRatingsScope.Branch or ManagementRatingsScope.List or ManagementRatingsScope.Follower)
        {
            matched = ActiveRatingCohort.Build(facts, asOf)
                .Where(m => m.Level == levelArabic)
                .ToList();
        }
        else
        {
            matched = [];
            foreach (var fact in RatingTrendCalculator.FactsSoldInPeriod(facts, fromDate, toDate, asOf))
            {
                if (fact.HasOpenBalance == false)
                    continue;
                var score = CustomerRatingCalculator.ScoreFor(
                    fact.AmountTotalSales,
                    fact.ReceiptsTotal,
                    fact.AmountRemaining,
                    fact.SaleDate,
                    fact.LastPaymentDate,
                    asOf,
                    false);
                if (score is null)
                    continue;
                var level = CustomerRatingCalculator.LevelName(score.Value);
                if (level != levelArabic)
                    continue;
                matched.Add((fact, score.Value, level));
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            var ids = matched.Select(m => m.Fact.CustomerId).Distinct().Take(2000).ToList();
            var nameMap = await LoadCustomerMetaAsync(ids, ct);
            matched = matched
                .Where(m =>
                {
                    nameMap.TryGetValue(m.Fact.CustomerId, out var meta);
                    var name = meta.Name ?? "";
                    var phone = meta.Phone ?? "";
                    return name.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || phone.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || m.Fact.CustomerId.ToString().Contains(q, StringComparison.Ordinal);
                })
                .ToList();
        }

        var total = matched.Count;
        var pageItems = matched
            .OrderBy(m => m.Fact.CustomerId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pageIds = pageItems.Select(m => m.Fact.CustomerId).ToList();
        var names = await LoadCustomerMetaAsync(pageIds, ct);

        var cards = pageItems.Select(m =>
        {
            names.TryGetValue(m.Fact.CustomerId, out var meta);
            var isLegal = meta.IsLegal || m.Fact.IsLegal;
            double? paid = Math.Round(m.Fact.ReceiptsTotal, 0, MidpointRounding.AwayFromZero);
            double? rem = m.Fact.AmountRemaining;
            double? totalAmt = Math.Round(m.Fact.AmountTotalSales, 0, MidpointRounding.AwayFromZero);
            if (isLegal)
            {
                paid = null;
                rem = null;
            }

            return new ManagementRatingsCustomerCardDto(
                m.Fact.CustomerId,
                string.IsNullOrWhiteSpace(meta.Name) ? $"زبون {m.Fact.CustomerId}" : meta.Name!,
                meta.Phone,
                m.Level,
                m.Level,
                m.Score,
                m.Fact.SaleDate,
                m.Fact.LastPaymentDate,
                paid,
                rem,
                totalAmt,
                isLegal,
                isLegal);
        })
            .OrderBy(c => c.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ManagementRatingsCustomersResponse(
            ManagementRatingsScopeParser.ToApiKey(scope),
            resourceId,
            levelArabic,
            page,
            pageSize,
            total,
            cards,
            _clock.UtcNowOffset);
    }

    public async Task<Result<ManagementRatingsListPaymentsSumResponse>> GetListPaymentsSumAsync(
        int branchId,
        int listId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct)
    {
        var scopeError = await ValidateResourceAsync(branchId, ManagementRatingsScope.List, listId, ct);
        if (scopeError is not null)
            return scopeError.Value;

        if (toDate < fromDate)
            (fromDate, toDate) = (toDate, fromDate);

        var rows = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Order!.OrderListId == listId
                        && p.Order.BranchId == branchId
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved
                        && p.Date >= fromDate
                        && p.Date <= toDate)
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Sum(x => x.Amount), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return new ManagementRatingsListPaymentsSumResponse(
            listId,
            fromDate,
            toDate,
            Convert.ToDecimal(Math.Round(rows?.Total ?? 0, 0, MidpointRounding.AwayFromZero)),
            rows?.Count ?? 0,
            _clock.UtcNowOffset);
    }

    private async Task<Error?> ValidateResourceAsync(
        int branchId,
        ManagementRatingsScope scope,
        int resourceId,
        CancellationToken ct)
    {
        switch (scope)
        {
            case ManagementRatingsScope.Branch:
                return null;
            case ManagementRatingsScope.List:
                if (resourceId <= 0)
                    return ManagementRatingsErrors.InvalidResource;
                var listOk = await _db.OrderLists.AsNoTracking()
                    .AnyAsync(l => l.Id == resourceId && l.BranchId == branchId, ct);
                return listOk ? null : ManagementRatingsErrors.InvalidResource;
            case ManagementRatingsScope.Follower:
                if (resourceId <= 0)
                    return ManagementRatingsErrors.InvalidResource;
                var followerOk = await _db.OrderLists.AsNoTracking()
                    .AnyAsync(l => l.BranchId == branchId && l.MotabaId == resourceId, ct);
                return followerOk ? null : ManagementRatingsErrors.InvalidResource;
            case ManagementRatingsScope.Employee:
                if (resourceId <= 0)
                    return ManagementRatingsErrors.InvalidResource;
                var empOk = await _db.Orders.AsNoTracking()
                    .AnyAsync(o => o.BranchId == branchId
                                   && o.SellerId == resourceId
                                   && o.ApprovalStatus == OrderApprovalStatus.Approved, ct);
                return empOk ? null : ManagementRatingsErrors.InvalidResource;
            default:
                return ManagementRatingsErrors.InvalidScope;
        }
    }

    private async Task<string> ResolveResourceNameAsync(
        int branchId,
        ManagementRatingsScope scope,
        int resourceId,
        CancellationToken ct)
    {
        return scope switch
        {
            ManagementRatingsScope.List => await _db.OrderLists.AsNoTracking()
                .Where(l => l.Id == resourceId && l.BranchId == branchId)
                .Select(l => l.Name)
                .FirstOrDefaultAsync(ct) ?? $"قائمة {resourceId}",
            ManagementRatingsScope.Follower => await _db.OrderLists.AsNoTracking()
                .Where(l => l.BranchId == branchId && l.MotabaId == resourceId)
                .Select(l => l.Motaba != null ? l.Motaba.FullName : null)
                .FirstOrDefaultAsync(ct) ?? $"متابع {resourceId}",
            ManagementRatingsScope.Employee => await _db.Orders.AsNoTracking()
                .Where(o => o.BranchId == branchId && o.SellerId == resourceId)
                .Select(o => o.Seller != null ? o.Seller.FullName : null)
                .FirstOrDefaultAsync(ct) ?? $"موظف {resourceId}",
            _ => "أداء الفرع",
        };
    }

    private async Task<IReadOnlyList<ManagementRatingsEntityOptionDto>> LoadActiveListOptionsAsync(
        int branchId,
        DateOnly asOf,
        CancellationToken ct)
    {
        var cutoff = asOf.AddDays(-DelegateFollowupRules.MaxDaysListActivity);
        var lastPayments = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Order!.BranchId == branchId
                        && p.Order.OrderListId != null
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved
                        && p.Date >= cutoff)
            .GroupBy(p => p.Order!.OrderListId!.Value)
            .Select(g => new { ListId = g.Key, Last = g.Max(x => x.Date) })
            .ToListAsync(ct);

        var activeIds = lastPayments
            .Where(x => DelegateFollowupRules.IsListActive(x.Last, asOf))
            .Select(x => x.ListId)
            .ToList();
        if (activeIds.Count == 0)
            return [];

        return await _db.OrderLists.AsNoTracking()
            .Where(l => l.BranchId == branchId && activeIds.Contains(l.Id))
            .OrderBy(l => l.Name)
            .Select(l => new ManagementRatingsEntityOptionDto(l.Id, l.Name, null))
            .ToListAsync(ct);
    }

    private Task<List<RatingCustomerFact>> LoadAllBranchFactsAsync(int branchId, CancellationToken ct)
        => MaterializeFactsAsync(
            _db.Orders.AsNoTracking()
                .Where(o => o.BranchId == branchId && o.ApprovalStatus == OrderApprovalStatus.Approved),
            ct);

    private async Task<List<RatingCustomerFact>> LoadFactsAsync(
        int branchId,
        ManagementRatingsScope scope,
        int resourceId,
        CancellationToken ct)
    {
        IQueryable<Domain.Entities.OrderAggregate.Order> query = _db.Orders.AsNoTracking()
            .Where(o => o.BranchId == branchId && o.ApprovalStatus == OrderApprovalStatus.Approved);

        switch (scope)
        {
            case ManagementRatingsScope.List:
                query = query.Where(o => o.OrderListId == resourceId);
                break;
            case ManagementRatingsScope.Follower:
                var listIds = await _db.OrderLists.AsNoTracking()
                    .Where(l => l.BranchId == branchId && l.MotabaId == resourceId)
                    .Select(l => l.Id)
                    .ToListAsync(ct);
                if (listIds.Count == 0)
                    return [];
                query = query.Where(o => o.OrderListId != null && listIds.Contains(o.OrderListId.Value));
                break;
            case ManagementRatingsScope.Employee:
                query = query.Where(o => o.SellerId == resourceId);
                break;
            case ManagementRatingsScope.Branch:
                var active = await LoadActiveListOptionsAsync(branchId, TodayBaghdad(), ct);
                var activeIds = active.Select(a => a.Id).ToList();
                if (activeIds.Count == 0)
                    return [];
                query = query.Where(o => o.OrderListId != null && activeIds.Contains(o.OrderListId.Value));
                break;
        }

        return await MaterializeFactsAsync(query, ct);
    }

    private static async Task<List<RatingCustomerFact>> MaterializeFactsAsync(
        IQueryable<Domain.Entities.OrderAggregate.Order> query,
        CancellationToken ct)
    {
        var rows = await query
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
            .GroupBy(r => r.CustomerId)
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
                    CustomerId = g.Key,
                    OrderListId = g.Select(x => x.OrderListId).FirstOrDefault(x => x != null),
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

    private async Task<ManagementRatingsMonthBucketDto> LoadMonthBucketAsync(
        int branchId,
        int yearMonth,
        ManagementRatingsScope scope,
        int resourceId,
        CancellationToken ct)
    {
        var y = yearMonth / 100;
        var m = yearMonth % 100;
        var start = new DateOnly(y, m, 1);
        var endExclusive = start.AddMonths(1);

        // شهر المحافظة: كل زبائن الفرع (ليس فقط قوائم 40ي) — مطابق BranchMonthStatsService.
        var facts = scope == ManagementRatingsScope.Branch && resourceId <= 0
            ? await LoadAllBranchFactsAsync(branchId, ct)
            : await LoadFactsAsync(branchId, scope, resourceId, ct);

        var neu = facts
            .Where(f => f.HasOpenBalance
                        && f.SaleDate is DateOnly sale
                        && sale >= start
                        && sale < endExclusive)
            .ToList();

        var zeroed = facts
            .Where(f => f.HasOpenBalance == false
                        && f.LastPaymentDate is DateOnly paid
                        && paid >= start
                        && paid < endExclusive)
            .ToList();

        static decimal SumSale(IEnumerable<RatingCustomerFact> rows) =>
            Convert.ToDecimal(Math.Round(rows.Sum(x => x.AmountTotalSales), 2, MidpointRounding.AwayFromZero));
        static decimal SumDay(IEnumerable<RatingCustomerFact> rows) =>
            Convert.ToDecimal(Math.Round(rows.Sum(x => x.AmountDaySales), 2, MidpointRounding.AwayFromZero));

        return new ManagementRatingsMonthBucketDto(
            yearMonth,
            neu.Count,
            SumSale(neu),
            SumDay(neu),
            zeroed.Count,
            SumSale(zeroed),
            SumDay(zeroed),
            neu.Count - zeroed.Count,
            SumSale(neu) - SumSale(zeroed),
            SumDay(neu) - SumDay(zeroed));
    }

    private async Task<Dictionary<int, (string? Name, string? Phone, bool IsLegal)>> LoadCustomerMetaAsync(
        IReadOnlyList<int> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<int, (string?, string?, bool)>();

        var rows = await _db.Customers.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber, c.IsLegal })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Id, r => ((string?)r.FullName, (string?)r.PhoneNumber, r.IsLegal));
    }

    private static int PreviousYearMonth(int yearMonth)
    {
        var y = yearMonth / 100;
        var m = yearMonth % 100;
        return m <= 1 ? (y - 1) * 100 + 12 : y * 100 + (m - 1);
    }
}
