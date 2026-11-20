using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.FieldPromoters.App;
using Application.FieldPromoters.Common;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Application.SalesRequests.Common;
using Application.Users.Common;
using Domain.Entities.FieldPromoterAggregate;
using Domain.Entities.SalesLocationAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.FieldPromoters.Admin;

// ───────── live & overview ─────────

public sealed record GetPromotersLiveQuery(int? BranchId) : IRequest<Result<PromotersLiveDto>>;

public sealed record PromoterLiveRow(
    int Id, string FullName, string? Phone, string Status, string StatusLabel, double? Lat, double? Lng,
    DateTimeOffset? LastAt, DateTimeOffset? ShiftStartedAt, int WorkMinutes, int TargetMinutes, int Pins, int Orders,
    int TargetPercent, bool IsSuspended);

public sealed record PromotersLiveDto(
    int Total, int Working, int Stationary, int NotStarted, int PinsToday, int InterestedPinsToday,
    int OrdersToday, int SoldToday, int UnseenAlerts, int MetYesterday, int TargetedYesterday,
    IReadOnlyList<PromoterLiveRow> Promoters);

public class GetPromotersLiveHandler(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock)
    : IRequestHandler<GetPromotersLiveQuery, Result<PromotersLiveDto>>
{
    public async Task<Result<PromotersLiveDto>> Handle(GetPromotersLiveQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        var now = clock.UtcNowOffset;
        var today = FieldPromoterRules.IraqDay(now);
        var (start, end) = FieldPromoterRules.DayBounds(today);

        var promoters = await FieldPromoterAccess.PromotersInScope(db, user, request.BranchId).AsNoTracking()
            .OrderBy(u => u.FullName).Select(u => new { u.Id, u.FullName, u.PhoneNumber }).ToListAsync(ct);
        var ids = promoters.Select(p => p.Id).ToList();

        var suspended = await db.FieldPromoterProfiles.AsNoTracking()
            .Where(p => ids.Contains(p.UserId) && p.IsSuspended).Select(p => p.UserId).ToListAsync(ct);
        var shifts = await db.SalesShifts.AsNoTracking()
            .Where(s => ids.Contains(s.UserId) && s.EndedAtUtc == null)
            .Select(s => new { s.UserId, s.StartedAtUtc }).ToListAsync(ct);
        var lastPoints = await db.SalesLocationPoints.AsNoTracking()
            .Where(p => ids.Contains(p.UserId) && p.ActorKind == SalesLocationActorKind.FieldPromoter && p.RecordedAtUtc >= start)
            .GroupBy(p => p.UserId)
            .Select(g => g.OrderByDescending(p => p.RecordedAtUtc).Select(p => new { p.UserId, p.Latitude, p.Longitude, p.RecordedAtUtc }).First())
            .ToListAsync(ct);
        var openAlerts = await db.FieldPromoterAlerts.AsNoTracking()
            .Where(a => ids.Contains(a.UserId) && a.ResolvedAtUtc == null).Select(a => a.UserId).ToListAsync(ct);
        var unseen = await db.FieldPromoterAlerts.AsNoTracking()
            .CountAsync(a => ids.Contains(a.UserId) && a.SeenAtUtc == null && a.RaisedAtUtc >= now.AddDays(-1), ct);

        var facts = await FieldPromoterDayFacts.ComputeAsync(db, ids, today, ct);
        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        var targets = await FieldPromoterDayFacts.TargetsAsync(db, ids, settings.DefaultDailyWorkMinutes, ct);
        var interested = await db.FieldPromoterPins.AsNoTracking()
            .CountAsync(p => ids.Contains(p.UserId) && p.CreatedAtUtc >= start && p.CreatedAtUtc < end && p.Interest >= 4, ct);
        var soldToday = await db.SalesRequests.AsNoTracking().CountAsync(r =>
            r.SourceType == SalesRequestSourceType.FieldPromoter && r.CreatedByUserId != null
            && ids.Contains(r.CreatedByUserId.Value) && r.Status == SalesRequestStatus.Completed
            && r.StatusChangedAtUtc >= start && r.StatusChangedAtUtc < end, ct);
        var yesterday = today.AddDays(-1);
        var yesterdayDays = await db.FieldPromoterDays.AsNoTracking()
            .Where(d => ids.Contains(d.UserId) && d.Day == yesterday && d.Status != FieldPromoterDayStatus.NoTarget)
            .Select(d => d.Status).ToListAsync(ct);

        var rows = promoters.Select(p =>
        {
            var shift = shifts.Where(s => s.UserId == p.Id).OrderByDescending(s => s.StartedAtUtc).FirstOrDefault();
            var last = lastPoints.FirstOrDefault(x => x.UserId == p.Id);
            var fact = facts[p.Id];
            var target = targets[p.Id];
            var isSuspended = suspended.Contains(p.Id);
            var status = isSuspended ? "suspended"
                : shift is null ? (fact.Worked ? "ended" : "notStarted")
                : openAlerts.Contains(p.Id) ? "stationary" : "working";
            var label = status switch
            {
                "suspended" => "موقوف",
                "ended" => "أنهى الدوام",
                "notStarted" => "لم يبدأ",
                "stationary" => "واقف",
                _ => "في الدوام"
            };
            var percent = target > 0 ? Math.Min(100, (int)Math.Round(fact.WorkMinutes * 100.0 / target)) : 0;
            return new PromoterLiveRow(p.Id, p.FullName, p.PhoneNumber, status, label, last?.Latitude, last?.Longitude,
                last?.RecordedAtUtc, shift?.StartedAtUtc, fact.WorkMinutes, target, fact.Pins, fact.Orders, percent, isSuspended);
        }).ToList();

        return new PromotersLiveDto(rows.Count, rows.Count(r => r.Status == "working" || r.Status == "stationary"),
            rows.Count(r => r.Status == "stationary"), rows.Count(r => r.Status == "notStarted"),
            rows.Sum(r => r.Pins), interested, rows.Sum(r => r.Orders), soldToday, unseen,
            yesterdayDays.Count(s => s == FieldPromoterDayStatus.Met), yesterdayDays.Count, rows);
    }
}

// ───────── one promoter's day ─────────

public sealed record GetPromoterDayQuery(int UserId, DateOnly? Date) : IRequest<Result<PromoterDayDto>>;

public sealed record DayPinDto(int Id, double Lat, double Lng, string Name, byte Interest, string InterestLabel,
    string? Phone, string? Note, DateTimeOffset At, int? MinutesSincePrevious, bool Quick);

public sealed record DayOrderDto(int Id, string CustomerName, string Product, string StatusLabel, DateTimeOffset? At);

public sealed record DayAlertDto(int Id, double Lat, double Lng, DateTimeOffset Since, DateTimeOffset LastSeen,
    DateTimeOffset? ResolvedAt, int Minutes);

public sealed record PromoterDayDto(
    int UserId, string FullName, DateOnly Date, DateTimeOffset? FirstShiftStart, int WorkMinutes, int TargetMinutes,
    string TargetStatus, int Pins, int Orders, IReadOnlyList<PromoterSessionDto> Sessions,
    IReadOnlyList<PromoterPeriodDto> Periods, IReadOnlyList<PromoterRoutePointDto> Route,
    IReadOnlyList<DayPinDto> PinList, IReadOnlyList<DayOrderDto> OrderList, IReadOnlyList<DayAlertDto> Alerts);

public class GetPromoterDayHandler(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock)
    : IRequestHandler<GetPromoterDayQuery, Result<PromoterDayDto>>
{
    public async Task<Result<PromoterDayDto>> Handle(GetPromoterDayQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        var promoter = await FieldPromoterAccess.PromotersInScope(db, user).AsNoTracking()
            .Where(u => u.Id == request.UserId).Select(u => new { u.Id, u.FullName }).FirstOrDefaultAsync(ct);
        if (promoter is null) return FieldPromoterErrors.PromoterNotFound;

        var day = request.Date ?? FieldPromoterRules.IraqDay(clock.UtcNowOffset);
        var (start, end) = FieldPromoterRules.DayBounds(day);

        var sessions = await db.SalesShifts.AsNoTracking()
            .Where(s => s.UserId == promoter.Id && s.StartedAtUtc < end && (s.EndedAtUtc == null || s.EndedAtUtc >= start))
            .OrderBy(s => s.StartedAtUtc).Select(s => new PromoterSessionDto(s.StartedAtUtc, s.EndedAtUtc)).ToListAsync(ct);
        var route = await db.SalesLocationPoints.AsNoTracking()
            .Where(p => p.UserId == promoter.Id && p.ActorKind == SalesLocationActorKind.FieldPromoter
                        && p.RecordedAtUtc >= start && p.RecordedAtUtc < end)
            .OrderBy(p => p.RecordedAtUtc).Take(FieldPromoterRules.MaxDayPoints)
            .Select(p => new PromoterRoutePointDto(p.Latitude, p.Longitude, p.RecordedAtUtc)).ToListAsync(ct);
        var pins = await db.FieldPromoterPins.AsNoTracking()
            .Where(p => p.UserId == promoter.Id && p.CreatedAtUtc >= start && p.CreatedAtUtc < end)
            .OrderBy(p => p.CreatedAtUtc).ToListAsync(ct);
        var orders = await db.SalesRequests.AsNoTracking()
            .Where(r => r.CreatedByUserId == promoter.Id && r.SourceType == SalesRequestSourceType.FieldPromoter
                        && r.CreatedAt >= start && r.CreatedAt < end)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new { r.Id, r.CustomerName, r.RequestedProduct, r.Status, r.IsDeleted, r.CreatedAt }).ToListAsync(ct);
        var alerts = await db.FieldPromoterAlerts.AsNoTracking()
            .Where(a => a.UserId == promoter.Id && a.SinceUtc < end && a.LastSeenAtUtc >= start)
            .OrderBy(a => a.SinceUtc).ToListAsync(ct);

        var fact = (await FieldPromoterDayFacts.ComputeAsync(db, [promoter.Id], day, ct))[promoter.Id];
        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        var target = (await FieldPromoterDayFacts.TargetsAsync(db, [promoter.Id], settings.DefaultDailyWorkMinutes, ct))[promoter.Id];
        var status = FieldPromoterRules.DayStatus(target, fact.WorkMinutes, fact.Worked);

        var pinRows = new List<DayPinDto>();
        DateTimeOffset? previous = null;
        foreach (var p in pins.Where(p => p.IsDeleted == false))
        {
            int? gap = null;
            var quick = false;
            if (previous is DateTimeOffset prev && p.CreatedAtUtc - prev < FieldPromoterRules.PeriodGap)
            {
                gap = (int)Math.Round((p.CreatedAtUtc - prev).TotalMinutes);
                quick = p.CreatedAtUtc - prev < FieldPromoterRules.QuickVisit;
            }
            pinRows.Add(new DayPinDto(p.Id, p.Latitude, p.Longitude, p.Name, p.Interest,
                FieldPromoterRules.InterestLabel(p.Interest), p.Phone, p.Note, p.CreatedAtUtc, gap, quick));
            previous = p.CreatedAtUtc;
        }

        return new PromoterDayDto(promoter.Id, promoter.FullName, day, fact.FirstShiftStart, fact.WorkMinutes, target,
            FieldPromoterRules.DayStatusLabel(status), fact.Pins, fact.Orders, sessions,
            fact.Periods.Select(p => new PromoterPeriodDto(p.Start, p.End, p.Minutes, p.Events)).ToList(), route, pinRows,
            orders.Select(o => new DayOrderDto(o.Id, o.CustomerName, o.RequestedProduct ?? "",
                o.IsDeleted ? "ملغى" : SalesRequestStatusLabels.For(o.Status), o.CreatedAt)).ToList(),
            alerts.Select(a => new DayAlertDto(a.Id, a.Latitude, a.Longitude, a.SinceUtc, a.LastSeenAtUtc, a.ResolvedAtUtc,
                (int)Math.Round((a.LastSeenAtUtc - a.SinceUtc).TotalMinutes))).ToList());
    }
}

// ───────── attendance & targets ─────────

public sealed record GetPromoterAttendanceQuery(DateOnly From, DateOnly To, int? BranchId) : IRequest<Result<AttendanceDto>>;
public sealed record ExportPromoterAttendanceQuery(DateOnly From, DateOnly To, int? BranchId) : IRequest<Result<FileDto>>;

public sealed record AttendanceCell(DateOnly Day, string Status, string Label, int WorkMinutes, int TargetMinutes, int Pins, int Orders);

public sealed record AttendanceRow(int UserId, string FullName, int TargetMinutes, int Met, int Missed, int Off,
    IReadOnlyList<AttendanceCell> Days);

public sealed record AttendanceDto(DateOnly From, DateOnly To, IReadOnlyList<DateOnly> Days, IReadOnlyList<AttendanceRow> Rows);

public sealed record FileDto(byte[] Data, string ContentType, string FileName);

public class PromoterAttendanceHandlers(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock) :
    IRequestHandler<GetPromoterAttendanceQuery, Result<AttendanceDto>>,
    IRequestHandler<ExportPromoterAttendanceQuery, Result<FileDto>>
{
    public async Task<Result<AttendanceDto>> Handle(GetPromoterAttendanceQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        return await BuildAsync(request.From, request.To, request.BranchId, ct);
    }

    public async Task<Result<FileDto>> Handle(ExportPromoterAttendanceQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        var built = await BuildAsync(request.From, request.To, request.BranchId, ct);
        if (built.IsFailed) return built.Error!.Value;
        var data = built.Data!;
        var items = data.Rows.Select(r =>
        {
            var row = new Dictionary<string, object?> { ["name"] = r.FullName, ["target"] = r.TargetMinutes / 60.0,
                ["met"] = r.Met, ["missed"] = r.Missed, ["off"] = r.Off };
            foreach (var c in r.Days) row[c.Day.ToString("yyyy-MM-dd")] = $"{c.Label} ({c.WorkMinutes / 60}:{c.WorkMinutes % 60:00})";
            return row;
        }).ToList();
        var columns = new List<ExcelColumn>
        {
            new("المروّج", "name"), new("الهدف (ساعة)", "target"), new("حقّق", "met"), new("لم يحقّق", "missed"), new("غائب", "off")
        };
        columns.AddRange(data.Days.Select(d => new ExcelColumn(d.ToString("yyyy-MM-dd"), d.ToString("yyyy-MM-dd"))));
        var bytes = ExcelReportGenerator.GenerateExcelReport("الحضور", items, columns);
        return new FileDto(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"attendance-{data.From:yyyyMMdd}-{data.To:yyyyMMdd}.xlsx");
    }

    private async Task<Result<AttendanceDto>> BuildAsync(DateOnly from, DateOnly to, int? branchId, CancellationToken ct)
    {
        if (to < from || to.DayNumber - from.DayNumber >= FieldPromoterRules.MaxRangeDays) return FieldPromoterErrors.InvalidRange;
        var today = FieldPromoterRules.IraqDay(clock.UtcNowOffset);
        if (to > today) to = today;
        var promoters = await FieldPromoterAccess.PromotersInScope(db, user, branchId).AsNoTracking()
            .OrderBy(u => u.FullName).Select(u => new { u.Id, u.FullName }).ToListAsync(ct);
        var ids = promoters.Select(p => p.Id).ToList();
        var frozen = await db.FieldPromoterDays.AsNoTracking()
            .Where(d => ids.Contains(d.UserId) && d.Day >= from && d.Day <= to).ToListAsync(ct);
        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        var targets = await FieldPromoterDayFacts.TargetsAsync(db, ids, settings.DefaultDailyWorkMinutes, ct);

        var days = new List<DateOnly>();
        for (var d = from; d <= to; d = d.AddDays(1)) days.Add(d);

        var live = new Dictionary<DateOnly, Dictionary<int, DayFact>>();
        foreach (var d in days.Where(d => d == today || frozen.Any(f => f.Day == d) == false))
            live[d] = await FieldPromoterDayFacts.ComputeAsync(db, ids, d, ct);

        var rows = promoters.Select(p =>
        {
            var cells = days.Select(d =>
            {
                var f = frozen.FirstOrDefault(x => x.UserId == p.Id && x.Day == d);
                if (f is not null && d != today)
                    return new AttendanceCell(d, f.Status.ToString(), FieldPromoterRules.DayStatusLabel(f.Status),
                        f.WorkMinutes, f.TargetMinutes, f.Pins, f.Orders);
                var fact = live[d][p.Id];
                var target = targets[p.Id];
                if (d == today)
                    return new AttendanceCell(d, "Today", "اليوم", fact.WorkMinutes, target, fact.Pins, fact.Orders);
                var status = FieldPromoterRules.DayStatus(target, fact.WorkMinutes, fact.Worked);
                return new AttendanceCell(d, status.ToString(), FieldPromoterRules.DayStatusLabel(status),
                    fact.WorkMinutes, target, fact.Pins, fact.Orders);
            }).ToList();
            return new AttendanceRow(p.Id, p.FullName, targets[p.Id],
                cells.Count(c => c.Status == nameof(FieldPromoterDayStatus.Met)),
                cells.Count(c => c.Status == nameof(FieldPromoterDayStatus.Missed)),
                cells.Count(c => c.Status == nameof(FieldPromoterDayStatus.Off)), cells);
        }).ToList();
        return new AttendanceDto(from, to, days, rows);
    }
}

// ───────── alerts ─────────

public sealed record GetPromoterAlertsQuery(bool OpenOnly, int Page) : IRequest<Result<PromoterAlertsPage>>;
public sealed record MarkPromoterAlertSeenCommand(int? Id) : IRequest<Result>;

public sealed record PromoterAlertDto(int Id, int UserId, string FullName, double Lat, double Lng, DateTimeOffset Since,
    DateTimeOffset LastSeen, DateTimeOffset? ResolvedAt, int Minutes, bool Seen);

public sealed record PromoterAlertsPage(IReadOnlyList<PromoterAlertDto> Items, int Unseen, int Page, bool HasMore);

public class PromoterAlertHandlers(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock) :
    IRequestHandler<GetPromoterAlertsQuery, Result<PromoterAlertsPage>>,
    IRequestHandler<MarkPromoterAlertSeenCommand, Result>
{
    public async Task<Result<PromoterAlertsPage>> Handle(GetPromoterAlertsQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanAlerts(user) == false) return FieldPromoterErrors.Forbidden;
        if (request.Page is < 1 or > 10000) return FieldPromoterErrors.InvalidRange;
        var ids = FieldPromoterAccess.PromotersInScope(db, user).Select(u => u.Id);
        var query = db.FieldPromoterAlerts.AsNoTracking().Where(a => ids.Contains(a.UserId));
        if (request.OpenOnly) query = query.Where(a => a.ResolvedAtUtc == null);
        var unseen = await db.FieldPromoterAlerts.AsNoTracking()
            .CountAsync(a => ids.Contains(a.UserId) && a.SeenAtUtc == null && a.RaisedAtUtc >= clock.UtcNowOffset.AddDays(-1), ct);
        const int size = 30;
        var rows = await query.OrderByDescending(a => a.RaisedAtUtc).Skip((request.Page - 1) * size).Take(size + 1)
            .Select(a => new PromoterAlertDto(a.Id, a.UserId, a.User!.FullName, a.Latitude, a.Longitude, a.SinceUtc,
                a.LastSeenAtUtc, a.ResolvedAtUtc, 0, a.SeenAtUtc != null))
            .ToListAsync(ct);
        return new PromoterAlertsPage(rows.Take(size)
            .Select(a => a with { Minutes = (int)Math.Round((a.LastSeen - a.Since).TotalMinutes) }).ToList(),
            unseen, request.Page, rows.Count > size);
    }

    public async Task<Result> Handle(MarkPromoterAlertSeenCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanAlerts(user) == false || user.UserId is not int me) return FieldPromoterErrors.Forbidden;
        var ids = FieldPromoterAccess.PromotersInScope(db, user).Select(u => u.Id);
        var query = db.FieldPromoterAlerts.Where(a => ids.Contains(a.UserId) && a.SeenAtUtc == null);
        if (request.Id is int id) query = query.Where(a => a.Id == id);
        var alerts = await query.Take(500).ToListAsync(ct);
        if (request.Id is not null && alerts.Count == 0)
            return await db.FieldPromoterAlerts.AnyAsync(a => a.Id == request.Id && ids.Contains(a.UserId), ct)
                ? Result.Success() : FieldPromoterErrors.AlertNotFound;
        var now = clock.UtcNowOffset;
        foreach (var alert in alerts) alert.MarkSeen(me, now);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ───────── pins & orders ─────────

public sealed record GetAdminPinsQuery(int? BranchId, int? UserId, byte? MinInterest, DateOnly? From, DateOnly? To, string? Q, int Page)
    : IRequest<Result<AdminPinsPage>>;
public sealed record DeleteAdminPinCommand(int Id) : IRequest<Result>;

public sealed record AdminPinDto(int Id, double Lat, double Lng, string Name, byte Interest, string InterestLabel,
    string? Phone, string? Note, int UserId, string PromoterName, DateTimeOffset CreatedAt);

public sealed record AdminPinsPage(IReadOnlyList<AdminPinDto> Items, int Total, int Page, bool HasMore);

public sealed record GetAdminPromoterOrdersQuery(int? BranchId, int? UserId, string? Stage, DateOnly? From, DateOnly? To, int Page)
    : IRequest<Result<AdminPromoterOrdersPage>>;
public sealed record GetPromoterOrderShopPhotoQuery(int SalesRequestId) : IRequest<Result<FileDto>>;

public sealed record AdminPromoterOrderDto(PromoterOrderDto Order, int UserId, string PromoterName, bool HasShopPhoto,
    double? ShopLat, double? ShopLng);

public sealed record AdminPromoterOrdersPage(IReadOnlyList<AdminPromoterOrderDto> Items, int Total, int Page, bool HasMore,
    PromoterOrderCounts Counts);

public class AdminPinAndOrderHandlers(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock) :
    IRequestHandler<GetAdminPinsQuery, Result<AdminPinsPage>>,
    IRequestHandler<DeleteAdminPinCommand, Result>,
    IRequestHandler<GetAdminPromoterOrdersQuery, Result<AdminPromoterOrdersPage>>,
    IRequestHandler<GetPromoterOrderShopPhotoQuery, Result<FileDto>>
{
    public async Task<Result<AdminPinsPage>> Handle(GetAdminPinsQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        if (request.Page is < 1 or > 10000) return FieldPromoterErrors.InvalidRange;
        var ids = FieldPromoterAccess.PromotersInScope(db, user, request.BranchId).Select(u => u.Id);
        var query = db.FieldPromoterPins.AsNoTracking().Where(p => p.IsDeleted == false && ids.Contains(p.UserId));
        if (request.UserId is int uid) query = query.Where(p => p.UserId == uid);
        if (request.MinInterest is byte mi) query = query.Where(p => p.Interest >= mi);
        if (request.From is DateOnly from) query = query.Where(p => p.CreatedAtUtc >= FieldPromoterRules.DayBounds(from).Start);
        if (request.To is DateOnly to) query = query.Where(p => p.CreatedAtUtc < FieldPromoterRules.DayBounds(to).End);
        if (string.IsNullOrWhiteSpace(request.Q) == false)
        {
            var q = request.Q.Trim();
            query = query.Where(p => p.Name.Contains(q) || (p.Phone != null && p.Phone.Contains(q)));
        }
        const int size = 200;
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.CreatedAtUtc).Skip((request.Page - 1) * size).Take(size)
            .Select(p => new AdminPinDto(p.Id, p.Latitude, p.Longitude, p.Name, p.Interest, "", p.Phone, p.Note,
                p.UserId, p.User!.FullName, p.CreatedAtUtc))
            .ToListAsync(ct);
        return new AdminPinsPage(items.Select(i => i with { InterestLabel = FieldPromoterRules.InterestLabel(i.Interest) }).ToList(),
            total, request.Page, total > request.Page * size);
    }

    public async Task<Result> Handle(DeleteAdminPinCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanManage(user) == false) return FieldPromoterErrors.Forbidden;
        var ids = FieldPromoterAccess.PromotersInScope(db, user).Select(u => u.Id);
        var pin = await db.FieldPromoterPins.FirstOrDefaultAsync(p => p.Id == request.Id && p.IsDeleted == false && ids.Contains(p.UserId), ct);
        if (pin is null) return FieldPromoterErrors.PinNotFound;
        pin.Delete(clock.UtcNowOffset);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<AdminPromoterOrdersPage>> Handle(GetAdminPromoterOrdersQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        if (request.Page is < 1 or > 10000) return FieldPromoterErrors.InvalidRange;
        var ids = FieldPromoterAccess.PromotersInScope(db, user, request.BranchId).Select(u => u.Id);
        var source = db.SalesRequests.Where(r => r.SourceType == SalesRequestSourceType.FieldPromoter
            && r.CreatedByUserId != null && ids.Contains(r.CreatedByUserId.Value));
        if (request.UserId is int uid) source = source.Where(r => r.CreatedByUserId == uid);
        if (request.From is DateOnly from) source = source.Where(r => r.CreatedAt >= FieldPromoterRules.DayBounds(from).Start);
        if (request.To is DateOnly to) source = source.Where(r => r.CreatedAt < FieldPromoterRules.DayBounds(to).End);

        var all = await PromoterOrderReader.LoadAsync(db, source, ct);
        var counts = new PromoterOrderCounts(all.Count(r => r.Stage == "inProgress"), all.Count(r => r.Stage == "sold"),
            all.Count(r => r.Stage == "finished"), all.Count(r => r.Stage == "rejected"));
        var filtered = string.IsNullOrWhiteSpace(request.Stage) || request.Stage == "all"
            ? all : all.Where(r => r.Stage == request.Stage).ToList();
        const int size = 30;
        var page = filtered.Skip((request.Page - 1) * size).Take(size).ToList();
        var pageIds = page.Select(p => p.Id).ToList();
        var meta = await db.SalesRequests.AsNoTracking().Where(r => pageIds.Contains(r.Id))
            .Select(r => new { r.Id, UserId = r.CreatedByUserId!.Value, Name = r.CreatedByUser!.FullName }).ToDictionaryAsync(r => r.Id, ct);
        var shops = await db.FieldPromoterOrderShops.AsNoTracking().Where(s => pageIds.Contains(s.SalesRequestId))
            .Select(s => new { s.SalesRequestId, HasPhoto = s.Photo != null, s.Latitude, s.Longitude })
            .ToDictionaryAsync(s => s.SalesRequestId, ct);
        return new AdminPromoterOrdersPage(page.Select(o =>
        {
            shops.TryGetValue(o.Id, out var shop);
            return new AdminPromoterOrderDto(o, meta[o.Id].UserId, meta[o.Id].Name, shop?.HasPhoto ?? false, shop?.Latitude, shop?.Longitude);
        }).ToList(), filtered.Count, request.Page, filtered.Count > request.Page * size, counts);
    }

    public async Task<Result<FileDto>> Handle(GetPromoterOrderShopPhotoQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        var ids = FieldPromoterAccess.PromotersInScope(db, user).Select(u => u.Id);
        var shop = await db.FieldPromoterOrderShops.AsNoTracking()
            .Where(s => s.SalesRequestId == request.SalesRequestId && s.Photo != null
                        && db.SalesRequests.Any(r => r.Id == s.SalesRequestId && r.CreatedByUserId != null && ids.Contains(r.CreatedByUserId.Value)))
            .Select(s => new { s.Photo, s.PhotoContentType }).FirstOrDefaultAsync(ct);
        if (shop?.Photo is null) return FieldPromoterErrors.OrderNotFound;
        return new FileDto(shop.Photo, shop.PhotoContentType ?? "image/jpeg", $"shop-{request.SalesRequestId}");
    }
}

// ───────── accounts ─────────

public sealed record GetPromotersQuery(string? Q, string? Status, int? BranchId) : IRequest<Result<List<PromoterAccountDto>>>;

public sealed record PromoterAccountDto(int Id, string FullName, string Username, string? Phone,
    IReadOnlyList<PromoterBranchDto> Branches, bool IsSuspended, string? SuspendReason, int? TargetMinutes);

public sealed record SavePromoterCommand : IRequest<Result<int>>
{
    public int? Id { get; init; }
    public string FullName { get; init; } = "";
    public string Username { get; init; } = "";
    public string? Password { get; init; }
    public string? Phone { get; init; }
    public List<int> BranchIds { get; init; } = [];
}

public sealed record SetPromoterPasswordCommand(int Id, string Password) : IRequest<Result>;
public sealed record SuspendPromoterCommand(int Id, bool Suspended, string? Reason) : IRequest<Result>;
public sealed record SetPromoterTargetCommand(int Id, int? Minutes) : IRequest<Result>;

public class PromoterAccountHandlers(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock, IActivityLogService activityLog) :
    IRequestHandler<GetPromotersQuery, Result<List<PromoterAccountDto>>>,
    IRequestHandler<SavePromoterCommand, Result<int>>,
    IRequestHandler<SetPromoterPasswordCommand, Result>,
    IRequestHandler<SuspendPromoterCommand, Result>,
    IRequestHandler<SetPromoterTargetCommand, Result>
{
    private const int MinPassword = 8;

    public async Task<Result<List<PromoterAccountDto>>> Handle(GetPromotersQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        var query = FieldPromoterAccess.PromotersInScope(db, user, request.BranchId).AsNoTracking();
        if (string.IsNullOrWhiteSpace(request.Q) == false)
        {
            var q = request.Q.Trim();
            query = query.Where(u => u.FullName.Contains(q) || u.Username.Contains(q) || (u.PhoneNumber != null && u.PhoneNumber.Contains(q)));
        }
        var rows = await query.OrderBy(u => u.FullName)
            .Select(u => new
            {
                u.Id, u.FullName, u.Username, u.PhoneNumber,
                Branches = u.UserBranches.Select(b => new PromoterBranchDto(b.BranchId, b.Branch!.Name)).ToList()
            }).ToListAsync(ct);
        var ids = rows.Select(r => r.Id).ToList();
        var profiles = await db.FieldPromoterProfiles.AsNoTracking().Where(p => ids.Contains(p.UserId)).ToDictionaryAsync(p => p.UserId, ct);
        var result = rows.Select(r =>
        {
            profiles.TryGetValue(r.Id, out var p);
            return new PromoterAccountDto(r.Id, r.FullName, r.Username, r.PhoneNumber, r.Branches,
                p?.IsSuspended ?? false, p?.SuspendReason, p?.DailyWorkMinutesTarget);
        });
        return request.Status switch
        {
            "active" => result.Where(r => r.IsSuspended == false).ToList(),
            "suspended" => result.Where(r => r.IsSuspended).ToList(),
            _ => result.ToList()
        };
    }

    public async Task<Result<int>> Handle(SavePromoterCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanManage(user) == false) return FieldPromoterErrors.Forbidden;
        var fullName = (request.FullName ?? "").Trim();
        var username = (request.Username ?? "").Trim();
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : FieldPromoterRules.NormalizePhone(request.Phone);
        var branchIds = request.BranchIds.Distinct().ToList();
        if (fullName.Length is < 3 or > 50 || username.Length is < 3 or > 50 || branchIds.Count == 0
            || (string.IsNullOrWhiteSpace(request.Phone) == false && phone is null))
            return FieldPromoterErrors.InvalidAccount;
        if (request.Id is null && (request.Password is null || request.Password.Length is < MinPassword or > 64))
            return FieldPromoterErrors.InvalidAccount;
        if (branchIds.Any(b => (user.BranchIds ?? []).Contains(b) == false)) return FieldPromoterErrors.BranchForbidden;
        var roleId = await db.Roles.AsNoTracking().Where(r => r.Name == SeedRoles.FieldPromoter.Name).Select(r => (int?)r.Id).FirstOrDefaultAsync(ct);
        if (roleId is null) return FieldPromoterErrors.InvalidAccount;
        if (await db.Users.AnyAsync(u => u.Username == username && u.Id != (request.Id ?? 0), ct)) return FieldPromoterErrors.UsernameTaken;

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        User entity;
        if (request.Id is int id)
        {
            var found = await FieldPromoterAccess.PromotersInScope(db, user).Include(u => u.UserBranches).FirstOrDefaultAsync(u => u.Id == id, ct);
            if (found is null) return FieldPromoterErrors.PromoterNotFound;
            var guard = await UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync(db, user, id, roleId.Value, branchIds,
                roleId.Value, found.UserBranches.Select(b => b.BranchId).ToList(), ct);
            if (guard.IsFailed) return guard.Error!.Value;
            entity = found;
            var branchesChanged = entity.UserBranches.Select(b => b.BranchId).OrderBy(b => b).SequenceEqual(branchIds.OrderBy(b => b)) == false;
            entity.FullName = fullName;
            entity.Username = username;
            entity.PhoneNumber = phone;
            foreach (var old in entity.UserBranches.Where(b => branchIds.Contains(b.BranchId) == false).ToList())
                entity.UserBranches.Remove(old);
            foreach (var b in branchIds.Where(b => entity.UserBranches.Any(x => x.BranchId == b) == false))
                entity.UserBranches.Add(new UserBranch(b));
            if (branchesChanged) entity.InvalidateSessions();
        }
        else
        {
            var guard = await UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync(db, user, null, roleId.Value, branchIds, null, [], ct);
            if (guard.IsFailed) return guard.Error!.Value;
            entity = new User(fullName, null, null, null, username, PasswordHash.HashPassword(request.Password!), null, phone)
            {
                CreationStep = UserCreationStep.Completed,
                UserBranches = branchIds.Select(b => new UserBranch(b)).ToHashSet(),
                UserRoles = [new UserRole(roleId.Value)]
            };
            db.Users.Add(entity);
        }
        await db.SaveChangesAsync(ct);
        if (await db.FieldPromoterProfiles.AnyAsync(p => p.UserId == entity.Id, ct) == false)
            db.FieldPromoterProfiles.Add(new FieldPromoterProfile(entity.Id, clock.UtcNowOffset));
        foreach (var b in branchIds)
            await activityLog.AddAsync(b, request.Id is null ? Domain.Entities.ActivityLogAggregate.Enums.ActivityType.UserCreated
                : Domain.Entities.ActivityLogAggregate.Enums.ActivityType.UserUpdated,
                Domain.Entities.ActivityLogAggregate.Enums.TargetEntityType.User, entity.Id, details: "field promoter");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return entity.Id;
    }

    public async Task<Result> Handle(SetPromoterPasswordCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanManage(user) == false) return FieldPromoterErrors.Forbidden;
        if (request.Password is null || request.Password.Length is < MinPassword or > 64) return FieldPromoterErrors.InvalidAccount;
        var entity = await FieldPromoterAccess.PromotersInScope(db, user).Include(u => u.UserLogins).FirstOrDefaultAsync(u => u.Id == request.Id, ct);
        if (entity is null) return FieldPromoterErrors.PromoterNotFound;
        entity.PasswordHash = PasswordHash.HashPassword(request.Password);
        entity.InvalidateSessions();
        db.UserLogins.RemoveRange(entity.UserLogins);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> Handle(SuspendPromoterCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanManage(user) == false) return FieldPromoterErrors.Forbidden;
        if (request.Reason is { Length: > FieldPromoterProfile.MaxReasonLength }) return FieldPromoterErrors.InvalidAccount;
        var entity = await FieldPromoterAccess.PromotersInScope(db, user).Include(u => u.UserLogins).FirstOrDefaultAsync(u => u.Id == request.Id, ct);
        if (entity is null) return FieldPromoterErrors.PromoterNotFound;
        var profile = await db.FieldPromoterProfiles.FirstOrDefaultAsync(p => p.UserId == entity.Id, ct);
        if (profile is null)
        {
            profile = new FieldPromoterProfile(entity.Id, clock.UtcNowOffset);
            db.FieldPromoterProfiles.Add(profile);
        }
        if (request.Suspended)
        {
            profile.Suspend(request.Reason);
            entity.InvalidateSessions();
            db.UserLogins.RemoveRange(entity.UserLogins);
            var open = await db.SalesShifts.Where(s => s.UserId == entity.Id && s.EndedAtUtc == null).ToListAsync(ct);
            foreach (var s in open) s.End(clock.UtcNowOffset);
        }
        else
        {
            profile.Activate();
        }
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> Handle(SetPromoterTargetCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanManage(user) == false) return FieldPromoterErrors.Forbidden;
        if (request.Minutes is < 0 or > 1440) return FieldPromoterErrors.InvalidSettings;
        var exists = await FieldPromoterAccess.PromotersInScope(db, user).AnyAsync(u => u.Id == request.Id, ct);
        if (exists == false) return FieldPromoterErrors.PromoterNotFound;
        var profile = await db.FieldPromoterProfiles.FirstOrDefaultAsync(p => p.UserId == request.Id, ct);
        if (profile is null)
        {
            profile = new FieldPromoterProfile(request.Id, clock.UtcNowOffset);
            db.FieldPromoterProfiles.Add(profile);
        }
        profile.DailyWorkMinutesTarget = request.Minutes;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ───────── settings ─────────

public sealed record GetPromoterSettingsQuery : IRequest<Result<PromoterSettingsDto>>;
public sealed record SavePromoterSettingsCommand(PromoterSettingsDto Settings) : IRequest<Result<PromoterSettingsDto>>;

public sealed record PromoterSettingsDto(int DefaultDailyWorkMinutes, int StationaryRadiusMeters, int StationaryMinutes,
    string? SupportPhone, string? SupportWhatsapp, int LocationRetentionDays);

public class PromoterSettingsHandlers(IApplicationDbContext db, ICurrentUserService user) :
    IRequestHandler<GetPromoterSettingsQuery, Result<PromoterSettingsDto>>,
    IRequestHandler<SavePromoterSettingsCommand, Result<PromoterSettingsDto>>
{
    public async Task<Result<PromoterSettingsDto>> Handle(GetPromoterSettingsQuery request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanRead(user) == false) return FieldPromoterErrors.Forbidden;
        return ToDto(await FieldPromoterAccess.SettingsAsync(db, ct));
    }

    public async Task<Result<PromoterSettingsDto>> Handle(SavePromoterSettingsCommand request, CancellationToken ct)
    {
        if (FieldPromoterAccess.CanManage(user) == false) return FieldPromoterErrors.Forbidden;
        var s = request.Settings;
        if (s.DefaultDailyWorkMinutes is < 0 or > 1440 || s.StationaryRadiusMeters is < 30 or > 2000
            || s.StationaryMinutes is < 5 or > 600
            || (s.SupportPhone?.Length ?? 0) > FieldPromoterSettings.MaxSupportLength
            || (s.SupportWhatsapp?.Length ?? 0) > FieldPromoterSettings.MaxSupportLength)
            return FieldPromoterErrors.InvalidSettings;
        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        settings.DefaultDailyWorkMinutes = s.DefaultDailyWorkMinutes;
        settings.StationaryRadiusMeters = s.StationaryRadiusMeters;
        settings.StationaryMinutes = s.StationaryMinutes;
        settings.SupportPhone = string.IsNullOrWhiteSpace(s.SupportPhone) ? null : s.SupportPhone.Trim();
        settings.SupportWhatsapp = string.IsNullOrWhiteSpace(s.SupportWhatsapp) ? null : s.SupportWhatsapp.Trim();
        await db.SaveChangesAsync(ct);
        return ToDto(settings);
    }

    private static PromoterSettingsDto ToDto(FieldPromoterSettings s) => new(s.DefaultDailyWorkMinutes,
        s.StationaryRadiusMeters, s.StationaryMinutes, s.SupportPhone, s.SupportWhatsapp, FieldPromoterRules.LocationRetentionDays);
}
