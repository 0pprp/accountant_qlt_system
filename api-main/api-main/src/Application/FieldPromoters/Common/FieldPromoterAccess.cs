using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.FieldPromoterAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.SalesShiftAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.FieldPromoters.Common;

public sealed record PromoterActor(int UserId, int HomeBranchId, IReadOnlyList<int> BranchIds);

public static class FieldPromoterAccess
{
    /// <summary>مروّج نشط: صلاحية التطبيق، غير موقوف، وله فرع واحد على الأقل.</summary>
    public static async Task<Result<PromoterActor>> RequireActivePromoterAsync(
        IApplicationDbContext db, ICurrentUserService user, CancellationToken ct)
    {
        if (user.UserId is not int userId || user.HasPermission(SeedPermissions.FieldPromoter.App) == false)
            return FieldPromoterErrors.Forbidden;
        if (await db.FieldPromoterProfiles.AsNoTracking().AnyAsync(p => p.UserId == userId && p.IsSuspended, ct))
            return FieldPromoterErrors.Suspended;
        var branches = await db.UserBranches.AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.BranchId)
            .Select(b => b.BranchId)
            .ToListAsync(ct);
        if (branches.Count == 0)
            return FieldPromoterErrors.NoBranch;
        return new PromoterActor(userId, branches[0], branches);
    }

    /// <summary>الدوام يبدأ تلقائياً مع أول طلب أو أول محل.</summary>
    public static async Task<SalesShift> EnsureShiftAsync(IApplicationDbContext db, PromoterActor actor,
        DateTimeOffset now, CancellationToken ct)
    {
        var open = await db.SalesShifts
            .Where(s => s.UserId == actor.UserId && s.EndedAtUtc == null)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(ct);
        if (open is not null)
            return open;
        var shift = new SalesShift(actor.UserId, actor.HomeBranchId, now);
        db.SalesShifts.Add(shift);
        await db.SaveChangesAsync(ct);
        return shift;
    }

    public static async Task<FieldPromoterSettings> SettingsAsync(IApplicationDbContext db, CancellationToken ct)
    {
        var settings = await db.FieldPromoterSettings.OrderBy(s => s.Id).FirstOrDefaultAsync(ct);
        if (settings is not null)
            return settings;
        settings = new FieldPromoterSettings();
        db.FieldPromoterSettings.Add(settings);
        await db.SaveChangesAsync(ct);
        return settings;
    }

    /// <summary>المروّجون الذين يشتركون مع المستخدم الحالي في فرع.</summary>
    public static IQueryable<Domain.Entities.UserAggregate.User> PromotersInScope(
        IApplicationDbContext db, ICurrentUserService user, int? branchId = null)
    {
        var scope = (user.BranchIds ?? []).ToList();
        if (branchId is int only)
            scope = scope.Contains(only) ? [only] : [];
        return db.Users.Where(u =>
            u.UserRoles.Any(r => r.Role!.Name == SeedRoles.FieldPromoter.Name)
            && u.UserBranches.Any(b => scope.Contains(b.BranchId)));
    }

    public static bool CanRead(ICurrentUserService user) =>
        user.HasPermission(SeedPermissions.FieldPromoter.Read);

    public static bool CanManage(ICurrentUserService user) =>
        user.HasPermission(SeedPermissions.FieldPromoter.Manage);

    public static bool CanAlerts(ICurrentUserService user) =>
        user.HasPermission(SeedPermissions.FieldPromoter.Alerts) || user.HasPermission(SeedPermissions.FieldPromoter.Read);
}

public sealed record DayFact(
    int UserId,
    int WorkMinutes,
    int Pins,
    int Orders,
    bool Worked,
    DateTimeOffset? FirstShiftStart,
    List<WorkPeriod> Periods);

public static class FieldPromoterDayFacts
{
    public static async Task<Dictionary<int, DayFact>> ComputeAsync(
        IApplicationDbContext db, IReadOnlyCollection<int> userIds, DateOnly day, CancellationToken ct)
    {
        var (start, end) = FieldPromoterRules.DayBounds(day);
        var ids = userIds.ToList();

        var pins = await db.FieldPromoterPins.AsNoTracking()
            .Where(p => ids.Contains(p.UserId) && p.CreatedAtUtc >= start && p.CreatedAtUtc < end)
            .Select(p => new { p.UserId, p.CreatedAtUtc })
            .ToListAsync(ct);
        var orders = await db.SalesRequests.AsNoTracking()
            .Where(r => r.SourceType == SalesRequestSourceType.FieldPromoter
                        && r.CreatedByUserId != null && ids.Contains(r.CreatedByUserId.Value)
                        && r.CreatedAt >= start && r.CreatedAt < end)
            .Select(r => new { UserId = r.CreatedByUserId!.Value, r.CreatedAt })
            .ToListAsync(ct);
        var shifts = await db.SalesShifts.AsNoTracking()
            .Where(s => ids.Contains(s.UserId) && s.StartedAtUtc < end && (s.EndedAtUtc == null || s.EndedAtUtc >= start))
            .Select(s => new { s.UserId, s.StartedAtUtc })
            .ToListAsync(ct);

        var result = new Dictionary<int, DayFact>();
        foreach (var id in ids)
        {
            var events = pins.Where(p => p.UserId == id).Select(p => p.CreatedAtUtc)
                .Concat(orders.Where(o => o.UserId == id && o.CreatedAt != null).Select(o => o.CreatedAt!.Value))
                .ToList();
            var periods = WorkPeriods.From(events);
            var myShifts = shifts.Where(s => s.UserId == id).ToList();
            result[id] = new DayFact(
                id,
                WorkPeriods.TotalMinutes(periods),
                pins.Count(p => p.UserId == id),
                orders.Count(o => o.UserId == id),
                myShifts.Count > 0 || events.Count > 0,
                myShifts.Count == 0 ? null : myShifts.Min(s => s.StartedAtUtc),
                periods);
        }
        return result;
    }

    public static async Task<Dictionary<int, int>> TargetsAsync(
        IApplicationDbContext db, IReadOnlyCollection<int> userIds, int defaultMinutes, CancellationToken ct)
    {
        var ids = userIds.ToList();
        var own = await db.FieldPromoterProfiles.AsNoTracking()
            .Where(p => ids.Contains(p.UserId) && p.DailyWorkMinutesTarget != null)
            .ToDictionaryAsync(p => p.UserId, p => p.DailyWorkMinutesTarget!.Value, ct);
        return ids.ToDictionary(id => id, id => own.TryGetValue(id, out var m) ? m : defaultMinutes);
    }
}
