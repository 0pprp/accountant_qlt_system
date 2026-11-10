using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerTrackingDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly FollowerSalesReviewDataService _scope;

    public FollowerTrackingDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        FollowerSalesReviewDataService scope)
    {
        _db = db;
        _clock = clock;
        _scope = scope;
    }

    public async Task<Result<FollowerLiveLocationsResponse>> GetLatestAsync(
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        int? followerId,
        CancellationToken ct)
    {
        var branchScope = branchId is > 0
            ? authorizedBranches.Contains(branchId.Value) ? new[] { branchId.Value } : Array.Empty<int>()
            : authorizedBranches.ToArray();
        if (branchScope.Length == 0)
            return FollowerWorkErrors.BranchAccessDenied;

        // متابعون = Motaba على قوائم ضمن الفروع — لا مندوب جباية ولا موظف مبيعات
        var followersQuery = _db.OrderLists.AsNoTracking()
            .Where(l => branchScope.Contains(l.BranchId))
            .Select(l => new { l.MotabaId, BranchId = l.BranchId })
            .Distinct();

        if (followerId is > 0)
            followersQuery = followersQuery.Where(x => x.MotabaId == followerId.Value);

        var followerPairs = await followersQuery.ToListAsync(ct);
        var followerIds = followerPairs.Select(x => x.MotabaId).Distinct().ToList();
        if (followerIds.Count == 0)
        {
            return new FollowerLiveLocationsResponse(
                [],
                FollowerWorkRules.TimeZoneId,
                _clock.UtcNowOffset,
                (int)FollowerLocationRules.LiveThreshold.TotalSeconds,
                (int)FollowerLocationRules.StaleThreshold.TotalMinutes);
        }

        var names = await _db.Users.AsNoTracking()
            .Where(u => followerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, ct);

        var branchNames = await _db.Branches.AsNoTracking()
            .Where(b => branchScope.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        var activeShifts = await _db.SalesShifts.AsNoTracking()
            .Where(s => followerIds.Contains(s.UserId) && s.EndedAtUtc == null)
            .Select(s => new { s.UserId, s.Id, s.BranchId })
            .ToListAsync(ct);
        var shiftByUser = activeShifts
            .GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

        // آخر موقع حسب وقت الالتقاط (RecordedAtUtc) — ليس ReceivedAt
        // عزل: نقاط مسار المبيعات (SalesEmployee) لا تدخل خريطة المتابعين
        var salesActor = Domain.Entities.SalesLocationAggregate.Enums.SalesLocationActorKind.SalesEmployee;
        var pointHeads = await _db.SalesLocationPoints.AsNoTracking()
            .Where(p => followerIds.Contains(p.UserId) && p.ActorKind != salesActor)
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

        var now = _clock.UtcNowOffset;
        var items = new List<FollowerLiveLocationRowDto>();
        foreach (var fid in followerIds.OrderBy(id => names.TryGetValue(id, out var n) ? n.FullName : id.ToString()))
        {
            names.TryGetValue(fid, out var user);
            shiftByUser.TryGetValue(fid, out var shift);
            pointByUser.TryGetValue(fid, out var point);

            // نقطة بلا إحداثيات صالحة = لا marker
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

            // فرع العرض: فرع الدوام النشط ثم فرع النقطة ثم أول تكليف
            var displayBranch = shift?.BranchId
                ?? point?.BranchId
                ?? followerPairs.FirstOrDefault(p => p.MotabaId == fid)?.BranchId;

            string? bName = null;
            if (displayBranch is int bid)
                branchNames.TryGetValue(bid, out bName);

            items.Add(new FollowerLiveLocationRowDto(
                fid,
                user?.FullName ?? $"متابع {fid}",
                displayBranch,
                bName,
                shift?.Id,
                hasShift,
                lat,
                lng,
                acc,
                captured,
                received,
                status));
        }

        return new FollowerLiveLocationsResponse(
            items,
            FollowerWorkRules.TimeZoneId,
            now,
            (int)FollowerLocationRules.LiveThreshold.TotalSeconds,
            (int)FollowerLocationRules.StaleThreshold.TotalMinutes);
    }

    public async Task<Result<FollowerRouteResponse>> GetRouteAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        DateOnly? dateIraq,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var scope = await _scope.ResolveFollowerScopeAsync(followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, followerName, _, branchIds) = scope.Data!;
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1
            ? FollowerLocationRules.DefaultRoutePageSize
            : Math.Min(pageSize, FollowerLocationRules.MaxRoutePageSize);

        var day = dateIraq ?? TodayBaghdad();
        var fromUtc = IraqDayStartUtc(day);
        var toUtc = IraqDayStartUtc(day.AddDays(1));

        // النقاط ضمن يوم بغداد؛ نطاق الفرع = BranchId وقت الحدث إن وُجد
        // عزل نقاط مسار موظف المبيعات عن مسار المتابع
        var salesActor = Domain.Entities.SalesLocationAggregate.Enums.SalesLocationActorKind.SalesEmployee;
        var query = _db.SalesLocationPoints.AsNoTracking()
            .Where(p => p.UserId == followerId
                        && p.ActorKind != salesActor
                        && p.RecordedAtUtc >= fromUtc
                        && p.RecordedAtUtc < toUtc);

        if (branchId is > 0)
            query = query.Where(p => p.BranchId == null || p.BranchId == branchId.Value);
        else
            query = query.Where(p => p.BranchId == null || branchIds.Contains(p.BranchId.Value));

        var totalCount = await query.CountAsync(ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var truncated = totalPages > 0 && page < totalPages;

        var rows = await query
            .OrderBy(p => p.RecordedAtUtc)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new FollowerRoutePointDto(
                p.Id,
                p.Latitude,
                p.Longitude,
                p.AccuracyMeters,
                p.RecordedAtUtc,
                p.ReceivedAtUtc,
                p.IsLive,
                p.ShiftId))
            .ToListAsync(ct);

        // إسقاط نقاط بإحداثيات غير صالحة من العرض دون حذف التخزين
        rows = rows.Where(p => FollowerLocationRules.IsValidCoordinate(p.Latitude, p.Longitude)).ToList();

        return new FollowerRouteResponse(
            followerId,
            followerName,
            day,
            fromUtc,
            toUtc,
            rows,
            totalCount,
            page,
            pageSize,
            totalPages,
            truncated,
            FollowerWorkRules.TimeZoneId,
            _clock.UtcNowOffset);
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    private static DateTimeOffset IraqDayStartUtc(DateOnly dayIraq)
    {
        var local = new DateTimeOffset(
            dayIraq.Year, dayIraq.Month, dayIraq.Day, 0, 0, 0,
            TimeSpan.FromHours(3));
        return local.ToUniversalTime();
    }
}
