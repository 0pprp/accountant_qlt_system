using Application.Common.Interfaces;
using Domain.Entities.FieldPromoterAggregate;
using Domain.Entities.SalesLocationAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.FieldPromoters.Common;

public sealed record MaintenanceReport(int ClosedShifts, int RaisedAlerts, int ResolvedAlerts, int FrozenDays, int DeletedPoints);

/// <summary>
/// دورة دورية للمروّجين: إغلاق دوام الأيام السابقة، وتنبيهات الوقوف، وتجميد الأيام المنتهية،
/// وحذف نقاط الموقع الأقدم من مدة الاحتفاظ مرة في اليوم.
/// </summary>
public class FieldPromoterMaintenance(IApplicationDbContext db)
{
    private const int MaxFreezeDaysPerRun = 7;

    public async Task<MaintenanceReport> RunAsync(DateTimeOffset now, CancellationToken ct)
    {
        var promoterIds = await db.Users.AsNoTracking()
            .Where(u => u.UserRoles.Any(r => r.Role!.Name == SeedRoles.FieldPromoter.Name))
            .Select(u => u.Id).ToListAsync(ct);
        if (promoterIds.Count == 0)
            return new MaintenanceReport(0, 0, 0, 0, 0);

        var settings = await FieldPromoterAccess.SettingsAsync(db, ct);
        var closed = await CloseStaleShiftsAsync(promoterIds, now, ct);
        var (raised, resolved) = await DetectStationaryAsync(promoterIds, settings, now, ct);
        var frozen = await FreezeDaysAsync(promoterIds, settings, now, ct);
        var deleted = await ApplyRetentionAsync(settings, now, ct);
        return new MaintenanceReport(closed, raised, resolved, frozen, deleted);
    }

    private async Task<int> CloseStaleShiftsAsync(List<int> promoterIds, DateTimeOffset now, CancellationToken ct)
    {
        var todayStart = FieldPromoterRules.DayBounds(FieldPromoterRules.IraqDay(now)).Start;
        var stale = await db.SalesShifts
            .Where(s => promoterIds.Contains(s.UserId) && s.EndedAtUtc == null && s.StartedAtUtc < todayStart)
            .ToListAsync(ct);
        foreach (var shift in stale)
        {
            var dayEnd = FieldPromoterRules.DayBounds(FieldPromoterRules.IraqDay(shift.StartedAtUtc)).End.AddSeconds(-1);
            var lastPoint = await db.SalesLocationPoints.AsNoTracking()
                .Where(p => p.ShiftId == shift.Id && p.RecordedAtUtc <= dayEnd)
                .MaxAsync(p => (DateTimeOffset?)p.RecordedAtUtc, ct);
            shift.End(lastPoint ?? dayEnd);
        }
        if (stale.Count > 0) await db.SaveChangesAsync(ct);
        return stale.Count;
    }

    private async Task<(int Raised, int Resolved)> DetectStationaryAsync(List<int> promoterIds, FieldPromoterSettings settings,
        DateTimeOffset now, CancellationToken ct)
    {
        var open = await db.SalesShifts.AsNoTracking()
            .Where(s => promoterIds.Contains(s.UserId) && s.EndedAtUtc == null)
            .Select(s => new { s.Id, s.UserId }).ToListAsync(ct);
        var alerts = await db.FieldPromoterAlerts.Where(a => promoterIds.Contains(a.UserId) && a.ResolvedAtUtc == null).ToListAsync(ct);
        int raised = 0, resolved = 0;

        foreach (var alert in alerts.Where(a => open.All(s => s.Id != a.ShiftId)))
        {
            alert.Resolve(now);
            resolved++;
        }

        var window = now.AddMinutes(-(settings.StationaryMinutes + 30));
        foreach (var shift in open)
        {
            var samples = await db.SalesLocationPoints.AsNoTracking()
                .Where(p => p.ShiftId == shift.Id && p.ActorKind == SalesLocationActorKind.FieldPromoter && p.RecordedAtUtc >= window)
                .OrderByDescending(p => p.RecordedAtUtc).Take(600)
                .Select(p => new LocationSample(p.Latitude, p.Longitude, p.RecordedAtUtc)).ToListAsync(ct);
            var stay = StationaryDetector.Detect(samples, settings.StationaryRadiusMeters, settings.StationaryMinutes, now);
            var current = alerts.FirstOrDefault(a => a.ShiftId == shift.Id && a.ResolvedAtUtc == null);
            if (stay is null)
            {
                if (current is not null && samples.Count > 0 && now - samples[0].At <= FieldPromoterRules.StationaryFreshness)
                {
                    current.Resolve(now);
                    resolved++;
                }
                continue;
            }
            if (current is not null)
            {
                current.Touch(stay.Until);
                continue;
            }
            db.FieldPromoterAlerts.Add(new FieldPromoterAlert(shift.UserId, shift.Id, stay.Latitude, stay.Longitude, stay.Since, now));
            raised++;
        }
        await db.SaveChangesAsync(ct);
        return (raised, resolved);
    }

    private async Task<int> FreezeDaysAsync(List<int> promoterIds, FieldPromoterSettings settings, DateTimeOffset now, CancellationToken ct)
    {
        var yesterday = FieldPromoterRules.IraqDay(now).AddDays(-1);
        if (settings.LastFrozenDay is null)
        {
            settings.LastFrozenDay = yesterday;
            await db.SaveChangesAsync(ct);
            return 0;
        }

        var frozen = 0;
        var day = settings.LastFrozenDay.Value.AddDays(1);
        for (var i = 0; i < MaxFreezeDaysPerRun && day <= yesterday; i++, day = day.AddDays(1))
        {
            var existing = await db.FieldPromoterDays.AsNoTracking().Where(d => d.Day == day).Select(d => d.UserId).ToListAsync(ct);
            var pending = promoterIds.Except(existing).ToList();
            var facts = await FieldPromoterDayFacts.ComputeAsync(db, pending, day, ct);
            var targets = await FieldPromoterDayFacts.TargetsAsync(db, pending, settings.DefaultDailyWorkMinutes, ct);
            foreach (var id in pending)
            {
                var f = facts[id];
                var status = FieldPromoterRules.DayStatus(targets[id], f.WorkMinutes, f.Worked);
                db.FieldPromoterDays.Add(new FieldPromoterDay(id, day, f.WorkMinutes, targets[id], f.Pins, f.Orders, status));
                frozen++;
            }
            settings.LastFrozenDay = day;
            await db.SaveChangesAsync(ct);
        }
        return frozen;
    }

    private async Task<int> ApplyRetentionAsync(FieldPromoterSettings settings, DateTimeOffset now, CancellationToken ct)
    {
        var today = FieldPromoterRules.IraqDay(now);
        if (settings.LastRetentionRunDay == today)
            return 0;
        var cutoff = now.AddDays(-FieldPromoterRules.LocationRetentionDays);
        var old = await db.SalesLocationPoints
            .Where(p => p.ActorKind == SalesLocationActorKind.FieldPromoter && p.RecordedAtUtc < cutoff)
            .OrderBy(p => p.Id).Take(20000).ToListAsync(ct);
        db.SalesLocationPoints.RemoveRange(old);
        settings.LastRetentionRunDay = today;
        await db.SaveChangesAsync(ct);
        return old.Count;
    }
}
