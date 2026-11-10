using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesEmployees.Common;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesOverview.Common;

public class SalesOverviewDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUserService _currentUser;

    public SalesOverviewDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        ICurrentUserService currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<SalesOverviewResponse> BuildAsync(
        IReadOnlyList<int> branchScope,
        int? requestedBranchId,
        CancellationToken ct)
    {
        var now = _clock.UtcNowOffset;
        var business = SalesWorkDayIraq.BusinessDate(now);
        var (windowStart, windowEnd) = SalesWorkDayIraq.TodayWindowUtc(now);

        var employeeIds = await SalesWorkforce.ActiveUserIdsAsync(_db, now, branchScope, ct);

        var onShift = 0;
        var offShift = 0;
        var live = 0;
        string? shiftUnavailable = null;

        if (employeeIds.Count == 0)
        {
            onShift = 0;
            offShift = 0;
            live = 0;
        }
        else
        {
            var activeShifts = await _db.SalesShifts.AsNoTracking()
                .Where(s => employeeIds.Contains(s.UserId) && s.EndedAtUtc == null)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync(ct);
            var onSet = activeShifts.ToHashSet();
            onShift = onSet.Count;
            offShift = employeeIds.Count - onShift;

            if (onSet.Count > 0)
            {
                var salesActor = Domain.Entities.SalesLocationAggregate.Enums.SalesLocationActorKind.SalesEmployee;
                var pointHeads = await _db.SalesLocationPoints.AsNoTracking()
                    .Where(p => onSet.Contains(p.UserId) && p.ActorKind == salesActor)
                    .Select(p => new { p.Id, p.UserId, p.RecordedAtUtc })
                    .ToListAsync(ct);
                var latestIds = pointHeads
                    .GroupBy(p => p.UserId)
                    .Select(g => g.OrderByDescending(x => x.RecordedAtUtc).ThenByDescending(x => x.Id).First().Id)
                    .ToList();
                var latest = latestIds.Count == 0
                    ? []
                    : await _db.SalesLocationPoints.AsNoTracking()
                        .Where(p => latestIds.Contains(p.Id))
                        .Select(p => new { p.UserId, p.RecordedAtUtc, p.Latitude, p.Longitude })
                        .ToListAsync(ct);

                foreach (var empId in onSet)
                {
                    var pt = latest.FirstOrDefault(p => p.UserId == empId);
                    DateTimeOffset? captured = pt?.RecordedAtUtc;
                    if (pt is not null &&
                        FollowerLocationRules.IsValidCoordinate(pt.Latitude, pt.Longitude) == false)
                        captured = null;
                    var status = FollowerLocationRules.ResolveStatus(true, captured, now);
                    if (status == FollowerLocationStatuses.Live)
                        live++;
                }
            }
        }

        // مبيعات منجزة = Order معتمدة — ليس SalesRequest.Completed وحده
        var salesToday = await _db.Orders.AsNoTracking()
            .Where(o => branchScope.Contains(o.BranchId)
                        && o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.SellerId != null
                        && o.SaleDate == business)
            .CountAsync(ct);

        var pending = await _db.SalesRequests.AsNoTracking()
            .Where(r => !r.IsDeleted
                        && branchScope.Contains(r.BranchId)
                        && r.Status == SalesRequestStatus.Pending)
            .CountAsync(ct);

        var @new = await _db.SalesRequests.AsNoTracking()
            .Where(r => !r.IsDeleted
                        && branchScope.Contains(r.BranchId)
                        && r.Status == SalesRequestStatus.New)
            .CountAsync(ct);

        var canEmployees = _currentUser.HasPermission(SeedPermissions.SalesEmployee.Read);
        var canRequestsPending = _currentUser.HasPermission(SeedPermissions.SalesRequest.TabPending);
        var canRequestsIncoming = _currentUser.HasPermission(SeedPermissions.SalesRequest.TabIncoming);
        var canOrders = _currentUser.HasPermission(SeedPermissions.Order.Read);

        var branchQ = requestedBranchId is > 0
            ? new Dictionary<string, string> { ["branchId"] = requestedBranchId.Value.ToString() }
            : null;

        var metrics = new List<SalesOverviewMetricDto>
        {
            Metric(
                SalesOverviewMetricKeys.EmployeesOnShift,
                "موظفين المبيعات في الدوام",
                "count",
                "SalesShift",
                onShift,
                "الآن",
                Link("SaleEmployeesRoute", "/sale/employees", SeedPermissions.SalesEmployee.Read, canEmployees,
                    MergeQuery(branchQ, ("shiftState", SalesEmployeeIdentity.ShiftActive)))),
            Metric(
                SalesOverviewMetricKeys.EmployeesOffShift,
                "بدون دوام",
                "count",
                "SalesShift",
                offShift,
                "الآن",
                Link("SaleEmployeesRoute", "/sale/employees", SeedPermissions.SalesEmployee.Read, canEmployees,
                    MergeQuery(branchQ, ("shiftState", SalesEmployeeIdentity.ShiftNoShift)))),
            Metric(
                SalesOverviewMetricKeys.LiveLocations,
                "الموقع المباشر",
                "count",
                "SalesLocationPoint",
                live,
                $"الآن ≤{(int)FollowerLocationRules.LiveThreshold.TotalSeconds}ث — عدد فقط بلا إحداثيات",
                Link("SaleEmployeesRoute", "/sale/employees", SeedPermissions.SalesEmployee.Read, canEmployees,
                    MergeQuery(branchQ, ("shiftState", SalesEmployeeIdentity.ShiftActive)))),
            Metric(
                SalesOverviewMetricKeys.SalesToday,
                "المبيعات اليوم",
                "count",
                "Order",
                salesToday,
                $"يوم العمل {business:yyyy-MM-dd} (قطع 03:00 بغداد) — SaleDate على مبيعات معتمدة",
                Link("SaleListRoute", "/sale", SeedPermissions.Order.Read, canOrders, branchQ)),
            Metric(
                SalesOverviewMetricKeys.PendingSales,
                "الطلبات المعلقة",
                "count",
                "SalesRequest",
                pending,
                "غير محذوف · Status=Pending",
                Link("SalesRequestsWorkspaceRoute", "/sale/requests", SeedPermissions.SalesRequest.TabPending,
                    canRequestsPending, MergeQuery(branchQ, ("tab", "pending")))),
            Metric(
                SalesOverviewMetricKeys.NewSalesRequests,
                "طلبات جديدة",
                "count",
                "SalesRequest",
                @new,
                "غير محذوف · Status=New",
                Link("SalesRequestsWorkspaceRoute", "/sale/requests", SeedPermissions.SalesRequest.TabIncoming,
                    canRequestsIncoming, MergeQuery(branchQ, ("tab", "incoming")))),
        };

        if (shiftUnavailable is not null)
        {
            // reserved for future source failures
        }

        return new SalesOverviewResponse
        {
            Period = new SalesOverviewPeriodDto
            {
                BusinessDateIraq = business.ToString("yyyy-MM-dd"),
                TimeZoneId = SalesWorkDayIraq.TimeZoneId,
                WindowStartUtc = windowStart,
                WindowEndUtc = windowEnd,
                GeneratedAtUtc = now,
            },
            Metrics = metrics,
            BranchId = requestedBranchId is > 0 ? requestedBranchId : null,
            BranchScope = branchScope,
        };
    }

    private static SalesOverviewMetricDto Metric(
        string key,
        string label,
        string unit,
        string owner,
        int value,
        string periodNote,
        SalesOverviewLinkDto? link) => new()
    {
        Key = key,
        LabelAr = label,
        Unit = unit,
        EntityOwner = owner,
        Availability = SalesOverviewAvailability.Available,
        Value = value,
        PeriodNote = periodNote,
        Link = link,
    };

    private static SalesOverviewLinkDto Link(
        string routeName,
        string path,
        string permission,
        bool allowed,
        IReadOnlyDictionary<string, string>? query) => new()
    {
        RouteName = routeName,
        Path = path,
        RequiredPermission = permission,
        Allowed = allowed,
        Query = query,
    };

    private static IReadOnlyDictionary<string, string>? MergeQuery(
        IReadOnlyDictionary<string, string>? baseQ,
        params (string Key, string Value)[] extras)
    {
        var dict = baseQ is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(baseQ);
        foreach (var (k, v) in extras)
            dict[k] = v;
        return dict.Count == 0 ? null : dict;
    }
}
