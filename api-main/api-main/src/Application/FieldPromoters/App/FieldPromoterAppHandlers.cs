using Application.Common.Interfaces;
using Application.FieldPromoters.Common;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.App.GetByTab;
using Application.SalesRequests.Realtime;
using Domain.Entities.FieldPromoterAggregate;
using Domain.Entities.SalesLocationAggregate;
using Domain.Entities.SalesLocationAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FieldPromoters.App;

// ───────── profile ─────────

public sealed record GetPromoterMeQuery : IRequest<Result<PromoterMeDto>>;

public sealed record PromoterBranchDto(int Id, string Name);

public sealed record PromoterMeDto(
    int Id, string FullName, string Username, IReadOnlyList<PromoterBranchDto> Branches,
    bool IsWorking, DateTimeOffset? ShiftStartedAt, int TodayPins, int TodayOrders, int TodayWorkMinutes,
    int TargetMinutes, int MonthOrders, int MonthSold, string? SupportPhone, string? SupportWhatsapp,
    IReadOnlyList<string> Products);

public class GetPromoterMeHandler(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock)
    : IRequestHandler<GetPromoterMeQuery, Result<PromoterMeDto>>
{
    public async Task<Result<PromoterMeDto>> Handle(GetPromoterMeQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var actor = access.Data!;
        var now = clock.UtcNowOffset;
        var today = FieldPromoterRules.IraqDay(now);

        var me = await db.Users.AsNoTracking().Where(u => u.Id == actor.UserId)
            .Select(u => new { u.Id, u.FullName, u.Username }).FirstAsync(ct);
        var branches = await db.Branches.AsNoTracking().Where(b => actor.BranchIds.Contains(b.Id))
            .OrderBy(b => b.Name).Select(b => new PromoterBranchDto(b.Id, b.Name)).ToListAsync(ct);
        var shift = await db.SalesShifts.AsNoTracking()
            .Where(s => s.UserId == actor.UserId && s.EndedAtUtc == null)
            .OrderByDescending(s => s.Id).Select(s => (DateTimeOffset?)s.StartedAtUtc).FirstOrDefaultAsync(ct);
        var facts = (await FieldPromoterDayFacts.ComputeAsync(db, [actor.UserId], today, ct))[actor.UserId];
        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        var target = (await FieldPromoterDayFacts.TargetsAsync(db, [actor.UserId], settings.DefaultDailyWorkMinutes, ct))[actor.UserId];

        var monthStart = FieldPromoterRules.DayBounds(new DateOnly(today.Year, today.Month, 1)).Start;
        var mine = db.SalesRequests.AsNoTracking().Where(r => r.CreatedByUserId == actor.UserId
            && r.SourceType == SalesRequestSourceType.FieldPromoter && r.CreatedAt >= monthStart);
        var monthOrders = await mine.CountAsync(ct);
        var monthSold = await mine.CountAsync(r => r.IsDeleted == false && r.Status == SalesRequestStatus.Completed, ct);

        return new PromoterMeDto(me.Id, me.FullName, me.Username, branches, shift is not null, shift,
            facts.Pins, facts.Orders, facts.WorkMinutes, target, monthOrders, monthSold,
            settings.SupportPhone, settings.SupportWhatsapp, FieldPromoterRules.Products);
    }
}

// ───────── work day ─────────

public sealed record StartPromoterShiftCommand : IRequest<Result<PromoterShiftDto>>;
public sealed record EndPromoterShiftCommand : IRequest<Result<PromoterShiftDto>>;
public sealed record GetPromoterTodayQuery : IRequest<Result<PromoterTodayDto>>;

public sealed record PromoterShiftDto(bool IsWorking, DateTimeOffset? StartedAt, DateTimeOffset? EndedAt);

public sealed record PromoterSessionDto(DateTimeOffset StartedAt, DateTimeOffset? EndedAt);

public sealed record PromoterPeriodDto(DateTimeOffset Start, DateTimeOffset End, int Minutes, int Events);

public sealed record PromoterRoutePointDto(double Lat, double Lng, DateTimeOffset At);

public sealed record PromoterTodayDto(
    bool IsWorking, DateTimeOffset? StartedAt, IReadOnlyList<PromoterSessionDto> Sessions,
    IReadOnlyList<PromoterPeriodDto> Periods, int WorkMinutes, int TargetMinutes, int Pins, int Orders,
    IReadOnlyList<PromoterRoutePointDto> Route);

public class PromoterShiftHandlers(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock) :
    IRequestHandler<StartPromoterShiftCommand, Result<PromoterShiftDto>>,
    IRequestHandler<EndPromoterShiftCommand, Result<PromoterShiftDto>>,
    IRequestHandler<GetPromoterTodayQuery, Result<PromoterTodayDto>>
{
    public async Task<Result<PromoterShiftDto>> Handle(StartPromoterShiftCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var shift = await FieldPromoterAccess.EnsureShiftAsync(db, access.Data!, clock.UtcNowOffset, ct);
        return new PromoterShiftDto(true, shift.StartedAtUtc, null);
    }

    public async Task<Result<PromoterShiftDto>> Handle(EndPromoterShiftCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var shift = await db.SalesShifts
            .Where(s => s.UserId == access.Data!.UserId && s.EndedAtUtc == null)
            .OrderByDescending(s => s.Id).FirstOrDefaultAsync(ct);
        if (shift is null) return FieldPromoterErrors.NoActiveShift;
        shift.End(clock.UtcNowOffset);
        await db.SaveChangesAsync(ct);
        return new PromoterShiftDto(false, shift.StartedAtUtc, shift.EndedAtUtc);
    }

    public async Task<Result<PromoterTodayDto>> Handle(GetPromoterTodayQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var actor = access.Data!;
        var today = FieldPromoterRules.IraqDay(clock.UtcNowOffset);
        var (start, end) = FieldPromoterRules.DayBounds(today);

        var sessions = await db.SalesShifts.AsNoTracking()
            .Where(s => s.UserId == actor.UserId && s.StartedAtUtc < end && (s.EndedAtUtc == null || s.EndedAtUtc >= start))
            .OrderBy(s => s.StartedAtUtc)
            .Select(s => new PromoterSessionDto(s.StartedAtUtc, s.EndedAtUtc))
            .ToListAsync(ct);
        var route = await db.SalesLocationPoints.AsNoTracking()
            .Where(p => p.UserId == actor.UserId && p.ActorKind == SalesLocationActorKind.FieldPromoter
                        && p.RecordedAtUtc >= start && p.RecordedAtUtc < end)
            .OrderBy(p => p.RecordedAtUtc)
            .Take(FieldPromoterRules.MaxDayPoints)
            .Select(p => new PromoterRoutePointDto(p.Latitude, p.Longitude, p.RecordedAtUtc))
            .ToListAsync(ct);
        var facts = (await FieldPromoterDayFacts.ComputeAsync(db, [actor.UserId], today, ct))[actor.UserId];
        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        var target = (await FieldPromoterDayFacts.TargetsAsync(db, [actor.UserId], settings.DefaultDailyWorkMinutes, ct))[actor.UserId];
        var open = sessions.LastOrDefault(s => s.EndedAt is null);

        return new PromoterTodayDto(open is not null, open?.StartedAt, sessions,
            facts.Periods.Select(p => new PromoterPeriodDto(p.Start, p.End, p.Minutes, p.Events)).ToList(),
            facts.WorkMinutes, target, facts.Pins, facts.Orders, route);
    }
}

// ───────── locations ─────────

public sealed record PromoterPointInput(double Lat, double Lng, double? Accuracy, DateTimeOffset RecordedAt, string ClientEventId);

public sealed record IngestPromoterLocationsCommand(IReadOnlyList<PromoterPointInput> Points) : IRequest<Result<PromoterIngestResult>>;

public sealed record PromoterIngestResult(int Accepted, int Duplicate, int Rejected, bool IsWorking);

public class IngestPromoterLocationsHandler(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock)
    : IRequestHandler<IngestPromoterLocationsCommand, Result<PromoterIngestResult>>
{
    public async Task<Result<PromoterIngestResult>> Handle(IngestPromoterLocationsCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        if (request.Points.Count > FieldPromoterRules.MaxBatchPoints) return FieldPromoterErrors.BatchTooLarge;
        var actor = access.Data!;

        var shift = await db.SalesShifts.AsNoTracking()
            .Where(s => s.UserId == actor.UserId && s.EndedAtUtc == null)
            .OrderByDescending(s => s.Id).FirstOrDefaultAsync(ct);
        if (shift is null)
            return new PromoterIngestResult(0, 0, request.Points.Count, false);

        var now = clock.UtcNowOffset;
        var keys = request.Points
            .Select(p => SalesLocations.Common.SalesLocationIngestHelper.NormalizeClientEventId(p.ClientEventId))
            .Where(k => k is not null).Cast<string>().Distinct().ToList();
        var existing = (await db.SalesLocationPoints.AsNoTracking()
            .Where(p => p.UserId == actor.UserId && p.ClientEventId != null && keys.Contains(p.ClientEventId))
            .Select(p => p.ClientEventId!).ToListAsync(ct)).ToHashSet();

        int accepted = 0, duplicate = 0, rejected = 0;
        var seen = new HashSet<string>();
        foreach (var point in request.Points)
        {
            var key = SalesLocations.Common.SalesLocationIngestHelper.NormalizeClientEventId(point.ClientEventId);
            if (key is null || FieldPromoterRules.InsideIraq(point.Lat, point.Lng) == false
                || point.RecordedAt > now.AddMinutes(5) || point.RecordedAt < shift.StartedAtUtc.AddMinutes(-2)
                || point.Accuracy is < 0 or > 5000)
            {
                rejected++;
                continue;
            }
            if (existing.Contains(key) || seen.Add(key) == false)
            {
                duplicate++;
                continue;
            }
            db.SalesLocationPoints.Add(new SalesLocationPoint(actor.UserId, shift.BranchId, point.Lat, point.Lng,
                point.RecordedAt.ToUniversalTime(), now, now - point.RecordedAt < TimeSpan.FromMinutes(3),
                SalesLocationActorKind.FieldPromoter, point.Accuracy, shift.Id, key));
            accepted++;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new PromoterIngestResult(0, request.Points.Count, 0, true);
        }
        return new PromoterIngestResult(accepted, duplicate, rejected, true);
    }
}

// ───────── sale requests ─────────

public sealed record SubmitPromoterOrderCommand : IRequest<Result<PromoterOrderReceipt>>
{
    public Guid SubmissionKey { get; init; }
    public int BranchId { get; init; }
    public string CustomerName { get; init; } = "";
    public string CustomerPhone { get; init; } = "";
    public string Product { get; init; } = "";
    public string Address { get; init; } = "";
    public string? Note { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public byte[]? Photo { get; init; }
}

public sealed record PromoterOrderReceipt(int Id, bool AlreadySubmitted);

public class SubmitPromoterOrderHandler(
    IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock, ISalesRequestSubmissionLock submissionLock)
    : IRequestHandler<SubmitPromoterOrderCommand, Result<PromoterOrderReceipt>>
{
    public async Task<Result<PromoterOrderReceipt>> Handle(SubmitPromoterOrderCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var actor = access.Data!;
        if (request.SubmissionKey == Guid.Empty) return FieldPromoterErrors.InvalidAccount;
        if (actor.BranchIds.Contains(request.BranchId) == false) return FieldPromoterErrors.BranchForbidden;

        var name = FieldPromoterRules.NormalizeTripleName(request.CustomerName);
        if (name is null) return FieldPromoterErrors.InvalidName;
        var phone = FieldPromoterRules.NormalizePhone(request.CustomerPhone);
        if (phone is null) return FieldPromoterErrors.InvalidPhone;
        var product = FieldPromoterRules.NormalizeProduct(request.Product);
        if (product is null) return FieldPromoterErrors.InvalidProduct;
        var address = (request.Address ?? "").Trim();
        if (address.Length is < 3 or > FieldPromoterRules.MaxAddressLength) return FieldPromoterErrors.InvalidAddress;
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        if (note is { Length: > FieldPromoterRules.MaxNoteLength }) return FieldPromoterErrors.InvalidNote;
        if ((request.Latitude is null) != (request.Longitude is null)) return FieldPromoterErrors.OutsideIraq;
        if (request.Latitude is double lat && FieldPromoterRules.InsideIraq(lat, request.Longitude!.Value) == false)
            return FieldPromoterErrors.OutsideIraq;
        (byte[] Bytes, string ContentType)? photo = null;
        if (request.Photo is { Length: > 0 })
        {
            photo = ShopPhotoSanitizer.Sanitize(request.Photo);
            if (photo is null) return FieldPromoterErrors.InvalidPhoto;
        }

        var now = clock.UtcNowOffset;
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await submissionLock.AcquireAsync(actor.UserId, request.SubmissionKey, ct);
        await submissionLock.AcquireAsync(0, PhoneProductKey(phone, product), ct);

        var previous = await db.SalesRequests.AsNoTracking().FirstOrDefaultAsync(
            x => x.CreatedByUserId == actor.UserId && x.SubmissionKey == request.SubmissionKey, ct);
        if (previous is not null)
            return new PromoterOrderReceipt(previous.Id, true);

        var duplicate = await db.SalesRequests.AsNoTracking().AnyAsync(x =>
            x.IsDeleted == false && x.CustomerPhone == phone && x.RequestedProduct == product
            && x.Status != SalesRequestStatus.Rejected && x.Status != SalesRequestStatus.Completed, ct);
        if (duplicate) return FieldPromoterErrors.DuplicateOpenOrder;

        var entity = new SalesRequest(name, phone, SalesRequestSourceType.FieldPromoter, request.BranchId, null, null, actor.UserId);
        entity.SetSubmission(address, product, note, request.SubmissionKey, request.SubmissionKey.ToString("N"), now);
        entity.CreatedAt = now;
        db.SalesRequests.Add(entity);
        await db.SaveChangesAsync(ct);

        if (request.Latitude is not null || photo is not null)
            db.FieldPromoterOrderShops.Add(new FieldPromoterOrderShop(entity.Id, request.Latitude, request.Longitude,
                photo?.Bytes, photo?.ContentType));
        SalesRequestTimelineWriter.Append(db, entity.Id, SalesRequestTimelineEventType.Created,
            actor.UserId, now, null, SalesRequestStatus.New, null, null, null, request.SubmissionKey);
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.Invalidation, now);
        await db.SaveChangesAsync(ct);
        await FieldPromoterAccess.EnsureShiftAsync(db, actor, now, ct);
        await transaction.CommitAsync(ct);
        return new PromoterOrderReceipt(entity.Id, false);
    }

    /// <summary>مفتاح قفل ثابت لكل (هاتف، منتج) حتى لا يمرّ طلبان متزامنان مكرّران.</summary>
    private static Guid PhoneProductKey(string phone, string product)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(phone + "|" + product));
        return new Guid(hash.AsSpan(0, 16));
    }
}

public sealed record GetPromoterOrdersQuery(string Tab, int Page) : IRequest<Result<PromoterOrdersPage>>;
public sealed record GetPromoterOrderCountsQuery : IRequest<Result<PromoterOrderCounts>>;
public sealed record GetPromoterOrderQuery(int Id) : IRequest<Result<PromoterOrderDto>>;
public sealed record CancelPromoterOrderCommand(int Id, string? Reason) : IRequest<Result>;

public sealed record PromoterSaleProgressDto(double Total, double Paid, double Remaining, int PaymentsCount,
    DateOnly? LastPaymentDate, int Percent);

public sealed record PromoterOrderDto(
    int Id, string CustomerName, string CustomerPhone, string Product, string Address, string? Note,
    string BranchName, string Stage, string StatusLabel, string? Reason, DateTimeOffset? CreatedAt,
    bool CanCancel, PromoterSaleProgressDto? Sale);

public sealed record PromoterOrdersPage(IReadOnlyList<PromoterOrderDto> Items, int Page, bool HasMore);

public sealed record PromoterOrderCounts(int InProgress, int Sold, int Finished, int Rejected);

public class PromoterOrderQueries(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock) :
    IRequestHandler<GetPromoterOrdersQuery, Result<PromoterOrdersPage>>,
    IRequestHandler<GetPromoterOrderCountsQuery, Result<PromoterOrderCounts>>,
    IRequestHandler<GetPromoterOrderQuery, Result<PromoterOrderDto>>,
    IRequestHandler<CancelPromoterOrderCommand, Result>
{
    private const int PageSize = 20;

    public async Task<Result<PromoterOrdersPage>> Handle(GetPromoterOrdersQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        if (request.Page is < 1 or > 10000) return FieldPromoterErrors.InvalidRange;
        var rows = await PromoterOrderReader.LoadAsync(db, Mine(access.Data!.UserId), ct);
        var filtered = rows.Where(r => request.Tab switch
        {
            "sold" => r.Stage == "sold",
            "finished" => r.Stage == "finished",
            "rejected" => r.Stage == "rejected",
            "all" => true,
            _ => r.Stage == "inProgress"
        }).ToList();
        var page = filtered.Skip((request.Page - 1) * PageSize).Take(PageSize).ToList();
        return new PromoterOrdersPage(page, request.Page, filtered.Count > request.Page * PageSize);
    }

    public async Task<Result<PromoterOrderCounts>> Handle(GetPromoterOrderCountsQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var rows = await PromoterOrderReader.LoadAsync(db, Mine(access.Data!.UserId), ct);
        return new PromoterOrderCounts(rows.Count(r => r.Stage == "inProgress"), rows.Count(r => r.Stage == "sold"),
            rows.Count(r => r.Stage == "finished"), rows.Count(r => r.Stage == "rejected"));
    }

    public async Task<Result<PromoterOrderDto>> Handle(GetPromoterOrderQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var rows = await PromoterOrderReader.LoadAsync(db, Mine(access.Data!.UserId).Where(r => r.Id == request.Id), ct);
        return rows.Count == 0 ? FieldPromoterErrors.OrderNotFound : rows[0];
    }

    public async Task<Result> Handle(CancelPromoterOrderCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var actor = access.Data!;
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var entity = await Mine(actor.UserId).FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (entity is null) return FieldPromoterErrors.OrderNotFound;
        if (entity.IsDeleted || entity.Status != SalesRequestStatus.New || entity.TargetEmployeeId is > 0)
            return FieldPromoterErrors.CannotCancel;
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "ألغاه المروّج" : "ألغاه المروّج: " + request.Reason.Trim();
        if (reason.Length > 200) reason = reason[..200];
        var now = clock.UtcNowOffset;
        entity.SoftDelete(reason, actor.UserId, now);
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.Invalidation, now);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result.Success();
    }

    private IQueryable<SalesRequest> Mine(int userId) => db.SalesRequests
        .Where(r => r.CreatedByUserId == userId && r.SourceType == SalesRequestSourceType.FieldPromoter);
}

public static class PromoterOrderReader
{
    public static async Task<List<PromoterOrderDto>> LoadAsync(IApplicationDbContext db, IQueryable<SalesRequest> source,
        CancellationToken ct)
    {
        var rows = await source.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Take(2000)
            .Select(r => new
            {
                r.Id, r.CustomerName, r.CustomerPhone, r.RequestedProduct, r.Address, r.SubmissionNote,
                Branch = r.Branch == null ? "" : r.Branch.Name, r.Status, r.IsDeleted, r.RejectReason, r.DeleteReason,
                r.CreatedAt, r.TargetEmployeeId, r.LinkedOrderId
            })
            .ToListAsync(ct);

        var orderIds = rows.Where(r => r.LinkedOrderId != null && r.Status == SalesRequestStatus.Completed)
            .Select(r => r.LinkedOrderId!.Value).ToList();
        var totals = await db.Orders.AsNoTracking().Where(o => orderIds.Contains(o.Id))
            .Select(o => new { o.Id, o.SellAmount }).ToDictionaryAsync(o => o.Id, o => o.SellAmount, ct);
        var payments = await db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId != null && orderIds.Contains(p.OrderId.Value))
            .GroupBy(p => p.OrderId!.Value)
            .Select(g => new { OrderId = g.Key, Paid = g.Sum(p => p.Amount), Count = g.Count(), Last = g.Max(p => p.Date) })
            .ToDictionaryAsync(g => g.OrderId, ct);

        return rows.Select(r =>
        {
            PromoterSaleProgressDto? sale = null;
            if (r.LinkedOrderId is int orderId && totals.TryGetValue(orderId, out var total))
            {
                payments.TryGetValue(orderId, out var p);
                var paid = p?.Paid ?? 0;
                var remaining = Math.Max(0, total - paid);
                var percent = total > 0 ? (int)Math.Clamp(Math.Round(paid / total * 100), 0, 100) : 0;
                sale = new PromoterSaleProgressDto(total, paid, remaining, p?.Count ?? 0, p?.Last, percent);
            }

            var stage = r.IsDeleted || r.Status == SalesRequestStatus.Rejected ? "rejected"
                : r.Status == SalesRequestStatus.Completed
                    ? (sale is { Total: > 0, Remaining: <= 0 } ? "finished" : "sold")
                    : "inProgress";
            var label = stage switch
            {
                "rejected" => r.IsDeleted ? "ملغى" : "لم يتم البيع",
                "finished" => "مكتمل التسديد",
                "sold" => "تم البيع — قيد التسديد",
                _ => SalesRequestStatusLabels.For(r.Status)
            };
            var reason = stage == "rejected" ? (string.IsNullOrWhiteSpace(r.RejectReason) ? r.DeleteReason : r.RejectReason) : null;
            return new PromoterOrderDto(r.Id, r.CustomerName, r.CustomerPhone ?? "", r.RequestedProduct ?? "",
                r.Address ?? "", r.SubmissionNote, r.Branch, stage, label, reason, r.CreatedAt,
                r.IsDeleted == false && r.Status == SalesRequestStatus.New && r.TargetEmployeeId is null or <= 0, sale);
        }).ToList();
    }
}

// ───────── shop pins ─────────

public sealed record PinInput(double Lat, double Lng, string Name, byte Interest, string? Phone, string? Note,
    double? AgentLat, double? AgentLng);

public sealed record CreatePinCommand(PinInput Input) : IRequest<Result<PinDto>>;
public sealed record UpdatePinCommand(int Id, PinInput Input) : IRequest<Result<PinDto>>;
public sealed record DeletePinCommand(int Id) : IRequest<Result>;
public sealed record GetPinsInBoxQuery(double MinLat, double MinLng, double MaxLat, double MaxLng) : IRequest<Result<List<PinMapDto>>>;
public sealed record GetMyPinsQuery(DateOnly? From, DateOnly? To, string? Q, int Page) : IRequest<Result<PinsPage>>;
public sealed record GetPinQuery(int Id) : IRequest<Result<PinDto>>;

/// <summary>ما يراه الزملاء: الموقع والاسم والاهتمام فقط — بلا هاتف ولا اسم المروّج.</summary>
public sealed record PinMapDto(int Id, double Lat, double Lng, string Name, byte Interest, bool Mine);

public sealed record PinDto(int Id, double Lat, double Lng, string Name, byte Interest, string InterestLabel,
    string? Phone, string? Note, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, bool Mine);

public sealed record PinsPage(IReadOnlyList<PinDto> Items, int Total, int Page, bool HasMore);

public class PromoterPinHandlers(IApplicationDbContext db, ICurrentUserService user, IDateTimeProvider clock) :
    IRequestHandler<CreatePinCommand, Result<PinDto>>,
    IRequestHandler<UpdatePinCommand, Result<PinDto>>,
    IRequestHandler<DeletePinCommand, Result>,
    IRequestHandler<GetPinsInBoxQuery, Result<List<PinMapDto>>>,
    IRequestHandler<GetMyPinsQuery, Result<PinsPage>>,
    IRequestHandler<GetPinQuery, Result<PinDto>>
{
    public async Task<Result<PinDto>> Handle(CreatePinCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var input = request.Input;
        if (FieldPromoterRules.InsideIraq(input.Lat, input.Lng) == false) return FieldPromoterErrors.OutsideIraq;
        var valid = Validate(input);
        if (valid.IsFailed) return valid.Error!.Value;
        var (name, phone, note) = valid.Data;
        double? agentLat = input.AgentLat is double a && input.AgentLng is double b && FieldPromoterRules.InsideIraq(a, b) ? a : null;
        double? agentLng = agentLat is null ? null : input.AgentLng;
        var now = clock.UtcNowOffset;
        var pin = new FieldPromoterPin(access.Data!.UserId, input.Lat, input.Lng, name, input.Interest, phone, note,
            agentLat, agentLng, now);
        db.FieldPromoterPins.Add(pin);
        await db.SaveChangesAsync(ct);
        await FieldPromoterAccess.EnsureShiftAsync(db, access.Data!, now, ct);
        return ToDto(pin, true);
    }

    public async Task<Result<PinDto>> Handle(UpdatePinCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var pin = await db.FieldPromoterPins.FirstOrDefaultAsync(p => p.Id == request.Id && p.IsDeleted == false, ct);
        if (pin is null) return FieldPromoterErrors.PinNotFound;
        if (pin.UserId != access.Data!.UserId) return FieldPromoterErrors.NotOwner;
        var valid = Validate(request.Input);
        if (valid.IsFailed) return valid.Error!.Value;
        var (name, phone, note) = valid.Data;
        pin.Update(name, request.Input.Interest, phone, note, clock.UtcNowOffset);
        await db.SaveChangesAsync(ct);
        return ToDto(pin, true);
    }

    public async Task<Result> Handle(DeletePinCommand request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var pin = await db.FieldPromoterPins.FirstOrDefaultAsync(p => p.Id == request.Id && p.IsDeleted == false, ct);
        if (pin is null) return FieldPromoterErrors.PinNotFound;
        if (pin.UserId != access.Data!.UserId) return FieldPromoterErrors.NotOwner;
        pin.Delete(clock.UtcNowOffset);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<List<PinMapDto>>> Handle(GetPinsInBoxQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var centerLat = (request.MinLat + request.MaxLat) / 2;
        var centerLng = (request.MinLng + request.MaxLng) / 2;
        var minLat = Math.Max(request.MinLat, centerLat - 0.5);
        var maxLat = Math.Min(request.MaxLat, centerLat + 0.5);
        var minLng = Math.Max(request.MinLng, centerLng - 0.5);
        var maxLng = Math.Min(request.MaxLng, centerLng + 0.5);
        var me = access.Data!.UserId;
        return await db.FieldPromoterPins.AsNoTracking()
            .Where(p => p.IsDeleted == false && p.Latitude >= minLat && p.Latitude <= maxLat
                        && p.Longitude >= minLng && p.Longitude <= maxLng)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(FieldPromoterRules.MaxBoxPins)
            .Select(p => new PinMapDto(p.Id, p.Latitude, p.Longitude, p.Name, p.Interest, p.UserId == me))
            .ToListAsync(ct);
    }

    public async Task<Result<PinsPage>> Handle(GetMyPinsQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        if (request.Page is < 1 or > 10000) return FieldPromoterErrors.InvalidRange;
        var query = db.FieldPromoterPins.AsNoTracking().Where(p => p.UserId == access.Data!.UserId && p.IsDeleted == false);
        if (request.From is DateOnly from) query = query.Where(p => p.CreatedAtUtc >= FieldPromoterRules.DayBounds(from).Start);
        if (request.To is DateOnly to) query = query.Where(p => p.CreatedAtUtc < FieldPromoterRules.DayBounds(to).End);
        if (string.IsNullOrWhiteSpace(request.Q) == false)
        {
            var q = request.Q.Trim();
            query = query.Where(p => p.Name.Contains(q) || (p.Phone != null && p.Phone.Contains(q)));
        }
        const int size = 30;
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.CreatedAtUtc).Skip((request.Page - 1) * size).Take(size).ToListAsync(ct);
        return new PinsPage(items.Select(p => ToDto(p, true)).ToList(), total, request.Page, total > request.Page * size);
    }

    public async Task<Result<PinDto>> Handle(GetPinQuery request, CancellationToken ct)
    {
        var access = await FieldPromoterAccess.RequireActivePromoterAsync(db, user, ct);
        if (access.IsFailed) return access.Error!.Value;
        var pin = await db.FieldPromoterPins.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id && p.IsDeleted == false, ct);
        if (pin is null) return FieldPromoterErrors.PinNotFound;
        return ToDto(pin, pin.UserId == access.Data!.UserId);
    }

    private static Result<(string Name, string? Phone, string? Note)> Validate(PinInput input)
    {
        var name = (input.Name ?? "").Trim();
        if (name.Length is < 2 or > FieldPromoterPin.MaxNameLength) return FieldPromoterErrors.InvalidPinName;
        if (input.Interest is < 1 or > 5) return FieldPromoterErrors.InvalidInterest;
        string? phone = null;
        if (string.IsNullOrWhiteSpace(input.Phone) == false)
        {
            phone = FieldPromoterRules.NormalizePhone(input.Phone);
            if (phone is null) return FieldPromoterErrors.InvalidPhone;
        }
        var note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
        if (note is { Length: > FieldPromoterPin.MaxNoteLength }) return FieldPromoterErrors.InvalidNote;
        return (name, phone, note);
    }

    private static PinDto ToDto(FieldPromoterPin p, bool mine) => new(p.Id, p.Latitude, p.Longitude, p.Name, p.Interest,
        FieldPromoterRules.InterestLabel(p.Interest), mine ? p.Phone : null, mine ? p.Note : null,
        p.CreatedAtUtc, p.UpdatedAtUtc, mine);
}
