using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.ManagementSummary.Common;
using Application.SalesEmployees.Common;
using Domain.Entities.SalesLocationAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesEmployeeTracking.Common;

public class SalesEmployeeTrackingDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public SalesEmployeeTrackingDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<SalesEmployeeLiveLocationsResponse>> GetLatestAsync(
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        int? employeeId,
        CancellationToken ct)
    {
        var branchScope = branchId is > 0
            ? authorizedBranches.Contains(branchId.Value) ? new[] { branchId.Value } : Array.Empty<int>()
            : authorizedBranches.ToArray();
        if (branchScope.Length == 0)
            return new Error("لا صلاحية على الفرع.", "SalesEmployeeTracking.BranchForbidden");

        var now = _clock.UtcNowOffset;
        var assignments = await SalesWorkforce.ActiveAssignments(_db, now, branchScope)
            .Select(a => new { a.UserId, a.BranchId })
            .ToListAsync(ct);

        if (employeeId is > 0)
            assignments = assignments.Where(a => a.UserId == employeeId.Value).ToList();

        var employeeIds = assignments.Select(a => a.UserId).Distinct().ToList();
        if (employeeIds.Count == 0)
        {
            return new SalesEmployeeLiveLocationsResponse
            {
                Items = [],
                TimeZoneId = IraqCalendar.TimeZone.Id,
                GeneratedAtUtc = now,
                LiveThresholdSeconds = (int)FollowerLocationRules.LiveThreshold.TotalSeconds,
                StaleThresholdMinutes = (int)FollowerLocationRules.StaleThreshold.TotalMinutes,
            };
        }

        var names = await _db.Users.AsNoTracking()
            .Where(u => employeeIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, ct);

        var branchNames = await _db.Branches.AsNoTracking()
            .Where(b => branchScope.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        var activeShifts = await _db.SalesShifts.AsNoTracking()
            .Where(s => employeeIds.Contains(s.UserId) && s.EndedAtUtc == null)
            .Select(s => new { s.UserId, s.Id, s.BranchId })
            .ToListAsync(ct);
        var shiftByUser = activeShifts
            .GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

        var salesActor = SalesLocationActorKind.SalesEmployee;
        var pointHeads = await _db.SalesLocationPoints.AsNoTracking()
            .Where(p => employeeIds.Contains(p.UserId) && p.ActorKind == salesActor)
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
                .ToListAsync(ct);
        var pointByUser = latestPoints.ToDictionary(p => p.UserId);

        var items = new List<SalesEmployeeLiveLocationRowDto>();
        foreach (var empId in employeeIds.OrderBy(id => names.TryGetValue(id, out var n) ? n.FullName : id.ToString()))
        {
            names.TryGetValue(empId, out var user);
            shiftByUser.TryGetValue(empId, out var shift);
            pointByUser.TryGetValue(empId, out var point);
            var primaryBranch = assignments.First(a => a.UserId == empId).BranchId;
            if (shift is not null && branchScope.Contains(shift.BranchId))
                primaryBranch = shift.BranchId;

            double? lat = null, lng = null, acc = null;
            DateTimeOffset? captured = null, received = null;
            if (point is not null
                && FollowerLocationRules.IsValidCoordinate(point.Latitude, point.Longitude))
            {
                lat = point.Latitude;
                lng = point.Longitude;
                acc = point.AccuracyMeters;
                captured = point.RecordedAtUtc;
                received = point.ReceivedAtUtc;
            }

            var hasShift = shift is not null;
            var status = FollowerLocationRules.ResolveStatus(hasShift, captured, now);

            items.Add(new SalesEmployeeLiveLocationRowDto
            {
                EmployeeId = empId,
                EmployeeName = user?.FullName ?? $"#{empId}",
                BranchId = primaryBranch,
                BranchName = branchNames.GetValueOrDefault(primaryBranch) ?? $"#{primaryBranch}",
                ShiftStatus = hasShift ? SalesWorkforce.ShiftActive : SalesWorkforce.ShiftNoShift,
                LocationStatus = status,
                Latitude = lat,
                Longitude = lng,
                AccuracyMeters = acc,
                CapturedAtUtc = captured,
                ReceivedAtUtc = received,
                HasCoordinates = lat is not null && lng is not null,
            });
        }

        return new SalesEmployeeLiveLocationsResponse
        {
            Items = items,
            TimeZoneId = IraqCalendar.TimeZone.Id,
            GeneratedAtUtc = now,
            LiveThresholdSeconds = (int)FollowerLocationRules.LiveThreshold.TotalSeconds,
            StaleThresholdMinutes = (int)FollowerLocationRules.StaleThreshold.TotalMinutes,
        };
    }

    /// <summary>
    /// مسار يوم تقويمي ببغداد (00:00→+1) — ليس قطع بيع 03:00.
    /// التفويض: BranchId وقت الالتقاط ضمن فروع القارئ؛ لا يشترط تكليفًا ساريًا اليوم.
    /// نقاط بلا BranchId موثوق تُستبعد (لا تخمين).
    /// </summary>
    public async Task<Result<SalesEmployeeRouteResponse>> GetRouteAsync(
        int employeeId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        DateOnly? dateIraq,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var branchScope = branchId is > 0
            ? authorizedBranches.Contains(branchId.Value) ? new[] { branchId.Value } : Array.Empty<int>()
            : authorizedBranches.ToArray();
        if (branchScope.Length == 0)
            return new Error("لا صلاحية على الفرع.", "SalesEmployeeTracking.BranchForbidden");

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == employeeId)
            .Select(u => new { u.Id, u.FullName })
            .FirstOrDefaultAsync(ct);
        if (user is null)
            return new Error("الموظف غير موجود.", "SalesEmployeeTracking.EmployeeNotFound");

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1
            ? FollowerLocationRules.DefaultRoutePageSize
            : Math.Min(pageSize, FollowerLocationRules.MaxRoutePageSize);

        var day = dateIraq ?? TodayBaghdad();
        var (fromUtc, toUtc) = IraqCalendar.DayBoundsUtc(day);
        var salesActor = SalesLocationActorKind.SalesEmployee;

        // نقاط بلا BranchId لا تُنسب بالتخمين
        var query = _db.SalesLocationPoints.AsNoTracking()
            .Where(p => p.UserId == employeeId
                        && p.ActorKind == salesActor
                        && p.BranchId != null
                        && branchScope.Contains(p.BranchId.Value)
                        && p.RecordedAtUtc >= fromUtc
                        && p.RecordedAtUtc < toUtc);

        var totalCount = await query.CountAsync(ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var truncated = totalPages > 0 && page < totalPages;

        var rows = await query
            .OrderBy(p => p.RecordedAtUtc)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Latitude,
                p.Longitude,
                p.AccuracyMeters,
                p.RecordedAtUtc,
                p.ReceivedAtUtc,
                p.IsLive,
                p.ShiftId,
                p.BranchId,
            })
            .ToListAsync(ct);

        var points = rows
            .Where(p => FollowerLocationRules.IsValidCoordinate(p.Latitude, p.Longitude))
            .Select(p => new SalesEmployeeRoutePointDto
            {
                PointId = p.Id,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                AccuracyMeters = p.AccuracyMeters,
                CapturedAtUtc = p.RecordedAtUtc,
                ReceivedAtUtc = p.ReceivedAtUtc,
                IsLive = p.IsLive,
                ShiftId = p.ShiftId,
                BranchId = p.BranchId,
            })
            .ToList();

        var gaps = new List<SalesEmployeeRouteGapDto>();
        for (var i = 1; i < points.Count; i++)
        {
            var gap = (points[i].CapturedAtUtc - points[i - 1].CapturedAtUtc).TotalMinutes;
            if (gap > 30)
            {
                gaps.Add(new SalesEmployeeRouteGapDto
                {
                    AfterPointId = points[i - 1].PointId,
                    BeforePointId = points[i].PointId,
                    GapMinutes = (int)Math.Round(gap),
                });
            }
        }

        return new SalesEmployeeRouteResponse
        {
            EmployeeId = employeeId,
            EmployeeName = user.FullName ?? $"#{employeeId}",
            DateIraq = day,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Points = points,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            Truncated = truncated,
            TimeZoneId = IraqCalendar.TimeZone.Id,
            GeneratedAtUtc = _clock.UtcNowOffset,
            Gaps = gaps,
            DayBoundNote = "يوم تقويمي بغداد 00:00→+1 — ليس قطع بيع المبيعات 03:00",
        };
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }
}
