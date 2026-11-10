using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesEmployees.Common;
using Domain.Entities.SalesLocationAggregate;
using Domain.Entities.SalesLocationAggregate.Enums;
using Domain.Entities.SalesShiftAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesLocations.Common;

public static class SalesLocationIngestHelper
{
    public static async Task<(SalesShift? Shift, Error? Error)> RequireActiveShiftAsync(
        IApplicationDbContext db,
        int userId,
        CancellationToken ct)
    {
        var shift = await db.SalesShifts
            .Where(s => s.UserId == userId && s.EndedAtUtc == null)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(ct);
        if (shift is null)
            return (null, SalesLocationErrors.ActiveShiftRequired);
        return (shift, null);
    }

    /// <summary>
    /// مسار تطبيق المبيعات يتطلب تكليف SalesEmployeeAssignment نشط على فرع الدوام.
    /// مسار المتابع لا يستخدم هذه البوابة.
    /// </summary>
    public static async Task<Error?> RequireSalesWorkforceIfNeededAsync(
        IApplicationDbContext db,
        int userId,
        int branchId,
        SalesLocationActorKind actorKind,
        DateTimeOffset utcNow,
        CancellationToken ct)
    {
        if (actorKind != SalesLocationActorKind.SalesEmployee)
            return null;

        var ok = await SalesWorkforce.HasActiveAssignmentAsync(db, userId, branchId, utcNow, ct);
        return ok ? null : SalesLocationErrors.SalesAssignmentRequired;
    }

    public static Error? ValidateCaptureTime(
        DateTimeOffset capturedAt,
        DateTimeOffset receivedAt,
        DateTimeOffset shiftStartedAt)
    {
        if (capturedAt > receivedAt.Add(FollowerLocationRules.MaxFutureSkew))
            return SalesLocationErrors.CapturedAtInFuture;
        if (capturedAt < shiftStartedAt.Subtract(FollowerLocationRules.MaxPastBeforeShift))
            return SalesLocationErrors.CapturedAtBeforeShift;
        return null;
    }

    public static async Task<bool> IsDuplicateAsync(
        IApplicationDbContext db,
        int userId,
        string? clientEventId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientEventId))
            return false;
        var key = clientEventId.Trim();
        if (key.Length > SalesLocationPoint.MaxClientEventIdLength)
            key = key[..SalesLocationPoint.MaxClientEventIdLength];
        return await db.SalesLocationPoints.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.ClientEventId == key, ct);
    }

    public static string? NormalizeClientEventId(string? clientEventId)
    {
        if (string.IsNullOrWhiteSpace(clientEventId))
            return null;
        var key = clientEventId.Trim();
        return key.Length > SalesLocationPoint.MaxClientEventIdLength
            ? key[..SalesLocationPoint.MaxClientEventIdLength]
            : key;
    }
}

public static class SalesLocationErrors
{
    public static readonly Error ActiveShiftRequired = new(
        "يلزم دوام نشط لإرسال الموقع.",
        "SalesLocation.ActiveShiftRequired");

    public static readonly Error InvalidCoordinates = new(
        "إحداثيات الموقع غير صالحة.",
        "SalesLocation.InvalidCoordinates");

    public static readonly Error CapturedAtInFuture = new(
        "وقت التقاط الموقع في المستقبل بشكل غير مقبول.",
        "SalesLocation.CapturedAtInFuture");

    public static readonly Error CapturedAtBeforeShift = new(
        "وقت التقاط الموقع قبل بدء الدوام.",
        "SalesLocation.CapturedAtBeforeShift");

    public static readonly Error BatchTooLarge = new(
        "عدد النقاط أكبر من الحد المسموح.",
        "SalesLocation.BatchTooLarge");

    public static readonly Error DuplicateEvent = new(
        "نقطة مكررة.",
        "SalesLocation.Duplicate");

    public static readonly Error SalesAssignmentRequired = new(
        "لا تكليف مبيعات ساري على فرع الدوام — الإرسال مرفوض.",
        "SalesLocation.SalesAssignmentRequired");
}
