using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesOverview.Common;
using Application.SalesRatings.Common;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesEmployees.Common;

public class SalesEmployeesDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;

    public SalesEmployeesDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        ICurrentUserService currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<ListSalesEmployeesResponse> ListAsync(
        IReadOnlyList<int> branchScope,
        string? search,
        string? shiftState,
        int page,
        int pageSize,
        string sort,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var now = _clock.UtcNowOffset;
        var business = SalesWorkDayIraq.BusinessDate(now);
        var asOf = _clock.Today;

        var assignmentRows = await SalesWorkforce.ActiveAssignments(_db, now, branchScope)
            .Select(a => new { a.UserId, a.BranchId })
            .ToListAsync(ct);

        var ids = assignmentRows.Select(a => a.UserId).Distinct().ToList();
        var users = ids.Count == 0
            ? []
            : await _db.Users.AsNoTracking()
                .Where(u => ids.Contains(u.Id) && u.CreationStep == Domain.Entities.UserAggregate.Enums.UserCreationStep.Completed)
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync(ct);

        var baseUsers = users.Select(u => new
        {
            u.Id,
            u.FullName,
            BranchIds = assignmentRows.Where(a => a.UserId == u.Id).Select(a => a.BranchId).Distinct().ToList(),
        }).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseUsers = baseUsers
                .Where(u => (u.FullName ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ids = baseUsers.Select(u => u.Id).ToList();
        var activeShifts = ids.Count == 0
            ? []
            : await _db.SalesShifts.AsNoTracking()
                .Where(s => ids.Contains(s.UserId) && s.EndedAtUtc == null)
                .Select(s => new { s.UserId, s.Id, s.StartedAtUtc, s.BranchId })
                .ToListAsync(ct);
        var shiftByUser = activeShifts
            .GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

        var salesActor = Domain.Entities.SalesLocationAggregate.Enums.SalesLocationActorKind.SalesEmployee;
        var pointHeads = ids.Count == 0
            ? []
            : await _db.SalesLocationPoints.AsNoTracking()
                .Where(p => ids.Contains(p.UserId) && p.ActorKind == salesActor)
                .Select(p => new { p.Id, p.UserId, p.RecordedAtUtc })
                .ToListAsync(ct);
        var latestIds = pointHeads
            .GroupBy(p => p.UserId)
            .Select(g => g.OrderByDescending(x => x.RecordedAtUtc).ThenByDescending(x => x.Id).First().Id)
            .ToList();
        var latestPoints = latestIds.Count == 0
            ? []
            : await _db.SalesLocationPoints.AsNoTracking()
                .Where(p => latestIds.Contains(p.Id))
                .Select(p => new { p.UserId, p.RecordedAtUtc, p.Latitude, p.Longitude })
                .ToListAsync(ct);
        var pointByUser = latestPoints.ToDictionary(p => p.UserId);

        var branchNames = await _db.Branches.AsNoTracking()
            .Where(b => branchScope.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        // تقييمات دفعة واحدة من المبيعات المعتمدة (SellerId)
        var orderFacts = ids.Count == 0
            ? []
            : await _db.Orders.AsNoTracking()
                .Where(o => o.SellerId != null
                            && ids.Contains(o.SellerId.Value)
                            && branchScope.Contains(o.BranchId)
                            && o.ApprovalStatus == OrderApprovalStatus.Approved)
                .Select(o => new
                {
                    SellerId = o.SellerId!.Value,
                    o.CustomerId,
                    o.SellAmount,
                    o.SaleDate,
                    Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                    LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
                })
                .ToListAsync(ct);

        var salesTodayBySeller = ids.Count == 0
            ? new Dictionary<int, int>()
            : await _db.Orders.AsNoTracking()
                .Where(o => o.SellerId != null
                            && ids.Contains(o.SellerId.Value)
                            && branchScope.Contains(o.BranchId)
                            && o.ApprovalStatus == OrderApprovalStatus.Approved
                            && o.SaleDate == business)
                .GroupBy(o => o.SellerId!.Value)
                .Select(g => new { SellerId = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.SellerId, x => x.Cnt, ct);

        var pendingByTarget = ids.Count == 0
            ? new Dictionary<int, int>()
            : await _db.SalesRequests.AsNoTracking()
                .Where(r => !r.IsDeleted
                            && r.TargetEmployeeId != null
                            && ids.Contains(r.TargetEmployeeId.Value)
                            && branchScope.Contains(r.BranchId)
                            && r.Status == SalesRequestStatus.Pending)
                .GroupBy(r => r.TargetEmployeeId!.Value)
                .Select(g => new { Id = g.Key, Cnt = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Cnt, ct);

        var canFollowup = _currentUser.HasPermission(SeedPermissions.SalesFollowup.Read);
        var canLiveMap = _currentUser.HasPermission(SeedPermissions.SalesLocation.Read);
        var canRoutes = _currentUser.HasPermission(SeedPermissions.SalesRoute.Read);

        var cards = new List<SalesEmployeeCardDto>(baseUsers.Count);
        foreach (var u in baseUsers)
        {
            var hasShift = shiftByUser.TryGetValue(u.Id, out var shift);
            var shiftStatus = hasShift
                ? SalesEmployeeIdentity.ShiftActive
                : SalesEmployeeIdentity.ShiftNoShift;

            if (!string.IsNullOrWhiteSpace(shiftState)
                && !string.Equals(shiftStatus, shiftState, StringComparison.OrdinalIgnoreCase))
                continue;

            pointByUser.TryGetValue(u.Id, out var pt);
            DateTimeOffset? captured = pt?.RecordedAtUtc;
            if (pt is not null &&
                FollowerLocationRules.IsValidCoordinate(pt.Latitude, pt.Longitude) == false)
                captured = null;
            var locStatus = FollowerLocationRules.ResolveStatus(hasShift, captured, now);

            var visibleBranches = SalesEmployeeIdentity.VisibleBranches(u.BranchIds, branchScope);
            var primary = shift?.BranchId is int sb && visibleBranches.Contains(sb)
                ? sb
                : visibleBranches.FirstOrDefault();
            var branchLabel = string.Join(" · ",
                visibleBranches.Select(id => branchNames.GetValueOrDefault(id) ?? $"#{id}"));

            var empOrders = orderFacts.Where(o => o.SellerId == u.Id).ToList();
            var realScores = new List<int>();
            var phantomScores = new List<int>();
            foreach (var g in empOrders.GroupBy(o => o.CustomerId))
            {
                var sellTotal = g.Sum(x => x.SellAmount);
                var receipts = g.Sum(x => x.Receipts);
                var remaining = sellTotal - receipts;
                var saleDate = g.Min(x => x.SaleDate);
                var lastPay = g.Max(x => x.LastPaymentDate);
                var real = CustomerRatingCalculator.ScoreFor(
                    sellTotal, receipts, remaining, saleDate, lastPay, asOf, usePhantomSaleDate: false);
                var phantom = CustomerRatingCalculator.ScoreFor(
                    sellTotal, receipts, remaining, saleDate, lastPay, asOf, usePhantomSaleDate: true);
                if (real is int rs && remaining > 0) realScores.Add(rs);
                if (phantom is int ps && remaining > 0) phantomScores.Add(ps);
            }

            var realAvg = realScores.Count == 0 ? (double?)null : Math.Round(realScores.Average(), 2);
            var phantomAvg = phantomScores.Count == 0 ? (double?)null : Math.Round(phantomScores.Average(), 2);

            cards.Add(new SalesEmployeeCardDto
            {
                EmployeeId = u.Id,
                EmployeeName = u.FullName ?? $"#{u.Id}",
                PrimaryBranchId = primary,
                BranchName = string.IsNullOrWhiteSpace(branchLabel) ? "—" : branchLabel,
                VisibleBranchIds = visibleBranches,
                ShiftStatus = shiftStatus,
                ShiftStartedAtUtc = shift?.StartedAtUtc,
                CurrentShiftId = shift?.Id,
                LocationStatus = locStatus,
                LastLocationAtUtc = captured,
                LatestRating = realAvg,
                PhantomLatestRating = phantomAvg,
                RatingLabel = realAvg is null ? null : RatingAggregator.ClassifyAverageLabel(realAvg.Value),
                SalesTodayCount = salesTodayBySeller.GetValueOrDefault(u.Id),
                PendingSalesCount = pendingByTarget.GetValueOrDefault(u.Id),
                IsAccountActive = true,
                Links = BuildLinks(canFollowup, canLiveMap, canRoutes),
            });
        }

        cards = SortCards(cards, sort).ToList();
        var total = cards.Count;
        var pageItems = cards.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new ListSalesEmployeesResponse
        {
            Items = pageItems,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            BranchScope = branchScope,
        };
    }

    public async Task<Result<SalesEmployeeRatingSummaryDto>> GetRatingSummaryAsync(
        int employeeId,
        IReadOnlyList<int> branchScope,
        string? bucket,
        CancellationToken ct)
    {
        var now = _clock.UtcNowOffset;
        var assignedBranches = await SalesWorkforce.ActiveAssignments(_db, now, branchScope)
            .Where(a => a.UserId == employeeId)
            .Select(a => a.BranchId)
            .Distinct()
            .ToListAsync(ct);
        if (assignedBranches.Count == 0)
            return SalesEmployeeErrors.NotFound;

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == employeeId)
            .Select(u => new { u.Id, u.FullName })
            .FirstOrDefaultAsync(ct);
        if (user is null)
            return SalesEmployeeErrors.NotFound;

        var visible = SalesWorkforce.VisibleBranches(assignedBranches, branchScope);
        if (visible.Count == 0)
            return SalesEmployeeErrors.NotFound;

        var branchId = visible[0];
        var asOf = _clock.Today;
        var saleFrom = asOf.AddDays(-60);
        var saleTo = asOf;

        var orderRows = await _db.Orders.AsNoTracking()
            .Where(o => o.SellerId == employeeId
                        && branchScope.Contains(o.BranchId)
                        && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(o => new
            {
                o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FullName : null,
                Phone = o.Customer != null ? o.Customer.PhoneNumber : null,
                o.SellAmount,
                o.SaleDate,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
            })
            .ToListAsync(ct);

        var factRows = orderRows
            .GroupBy(o => o.CustomerId)
            .Select(g =>
            {
                var total = g.Sum(x => x.SellAmount);
                var receipts = g.Sum(x => x.Receipts);
                var saleDate = g.Min(x => x.SaleDate);
                var lastPay = g.Max(x => x.LastPaymentDate);
                var name = g.Select(x => x.CustomerName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                           ?? $"#{g.Key}";
                var phone = g.Select(x => x.Phone).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));
                var fact = new RatingCustomerFact
                {
                    CustomerId = g.Key,
                    SellerId = employeeId,
                    AmountTotalSales = total,
                    ReceiptsTotal = receipts,
                    SaleDate = saleDate,
                    LastPaymentDate = lastPay,
                };
                return (Fact: fact, Name: name, Phone: phone);
            })
            .ToList();

        var facts = factRows.Select(x => x.Fact).ToList();

        var realAgg = RatingTrendCalculator.PeriodAggregate(facts, saleFrom, saleTo, asOf, usePhantomSaleDate: false);
        var phantomAgg = RatingTrendCalculator.PeriodAggregate(facts, saleFrom, saleTo, asOf, usePhantomSaleDate: true);
        var points = RatingTrendCalculator.DailyScores(facts, saleFrom, saleTo, saleFrom, saleTo)
            .Select(p => new SalesEmployeeTrendPointDto
            {
                Date = p.Date.ToString("yyyy-MM-dd"),
                Value = p.Value,
            })
            .ToList();

        var financialVisible = _currentUser.HasPermission(SeedPermissions.SalesRequest.ReadFinancial)
                               || _currentUser.HasPermission(SeedPermissions.SalesFollowup.Read);

        List<SalesEmployeeRatingCustomerDto>? customers = null;
        if (!string.IsNullOrWhiteSpace(bucket))
        {
            var level = NormalizeBucket(bucket);
            if (level is null)
                return SalesEmployeeErrors.InvalidBucket;

            customers = [];
            foreach (var row in factRows.Where(x => x.Fact.HasOpenBalance))
            {
                var f = row.Fact;
                var score = CustomerRatingCalculator.ScoreFor(
                    f.AmountTotalSales, f.ReceiptsTotal, f.AmountRemaining,
                    f.SaleDate, f.LastPaymentDate, asOf, usePhantomSaleDate: false);
                if (score is null) continue;
                var label = CustomerRatingCalculator.LevelName(score.Value);
                if (!string.Equals(label, level, StringComparison.Ordinal))
                    continue;

                customers.Add(new SalesEmployeeRatingCustomerDto
                {
                    CustomerId = f.CustomerId,
                    CustomerName = row.Name,
                    Phone = row.Phone,
                    DateSale = f.SaleDate?.ToString("yyyy-MM-dd"),
                    LastPaymentDate = f.LastPaymentDate?.ToString("yyyy-MM-dd"),
                    AmountPaid = financialVisible ? f.ReceiptsTotal : null,
                    AmountRemaining = financialVisible ? f.AmountRemaining : null,
                    AmountTotal = financialVisible ? f.AmountTotalSales : null,
                    RatingLabel = label,
                    AmountsRedacted = !financialVisible,
                });
            }
        }

        return new SalesEmployeeRatingSummaryDto
        {
            EmployeeId = employeeId,
            EmployeeName = user.FullName ?? $"#{employeeId}",
            BranchId = branchId,
            LatestRating = realAgg.CustomerCount == 0 ? null : Math.Round(realAgg.AverageScore, 2),
            PhantomLatestRating = phantomAgg.CustomerCount == 0 ? null : Math.Round(phantomAgg.AverageScore, 2),
            RatingLabel = realAgg.CustomerCount == 0 ? null : realAgg.FinalRatingLabel,
            ExcellentCount = realAgg.ExcellentCount,
            GoodCount = realAgg.GoodCount,
            WeakCount = realAgg.WeakCount,
            RejectedCount = realAgg.LaggingCount,
            BadCount = realAgg.BadCount,
            Points = points,
            FinancialVisible = financialVisible,
            Customers = customers,
        };
    }

    private static IReadOnlyList<SalesEmployeeLinkDto> BuildLinks(
        bool canFollowup,
        bool canLiveMap,
        bool canRoutes)
    {
        var links = new List<SalesEmployeeLinkDto>();
        if (canFollowup)
        {
            links.Add(new SalesEmployeeLinkDto
            {
                Key = "followup",
                LabelAr = "متابعة الأداء",
                RouteName = "SalesFollowupRoute",
                Path = "/sale/followup",
                RequiredPermission = SeedPermissions.SalesFollowup.Read,
                Allowed = true,
            });
        }
        if (canLiveMap)
        {
            links.Add(new SalesEmployeeLinkDto
            {
                Key = "liveMap",
                LabelAr = "الموقع المباشر",
                RouteName = "SaleLiveMapRoute",
                Path = "/sale/live-map",
                RequiredPermission = SeedPermissions.SalesLocation.Read,
                Allowed = true,
            });
        }
        if (canRoutes)
        {
            links.Add(new SalesEmployeeLinkDto
            {
                Key = "routes",
                LabelAr = "المسار",
                RouteName = "SaleRoutesRoute",
                Path = "/sale/routes",
                RequiredPermission = SeedPermissions.SalesRoute.Read,
                Allowed = true,
            });
        }
        return links;
    }

    private static string? NormalizeBucket(string bucket) => bucket.Trim().ToLowerInvariant() switch
    {
        "excellent" or "ممتاز" => "ممتاز",
        "good" or "جيد" => "جيد",
        "weak" or "ضعيف" => "ضعيف",
        "rejected" or "lagging" or "متلكئ" => "متلكئ",
        "bad" or "سيء" => "سيء",
        _ => null,
    };

    private static IEnumerable<SalesEmployeeCardDto> SortCards(
        IReadOnlyList<SalesEmployeeCardDto> cards,
        string sort)
    {
        var key = string.IsNullOrWhiteSpace(sort) ? "name" : sort.Trim();
        IOrderedEnumerable<SalesEmployeeCardDto> ordered = key.ToLowerInvariant() switch
        {
            "branch" => cards.OrderBy(c => c.BranchName).ThenBy(c => c.EmployeeName).ThenBy(c => c.EmployeeId),
            "shift" => cards.OrderBy(c => c.ShiftStatus == SalesEmployeeIdentity.ShiftActive ? 0 : 1)
                .ThenBy(c => c.EmployeeName).ThenBy(c => c.EmployeeId),
            "rating" => cards.OrderByDescending(c => c.LatestRating ?? double.MinValue)
                .ThenBy(c => c.EmployeeName).ThenBy(c => c.EmployeeId),
            "lastlocation" => cards.OrderByDescending(c => c.LastLocationAtUtc ?? DateTimeOffset.MinValue)
                .ThenBy(c => c.EmployeeName).ThenBy(c => c.EmployeeId),
            _ => cards.OrderBy(c => c.EmployeeName).ThenBy(c => c.EmployeeId),
        };
        return ordered;
    }
}
