using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Common;

/// <summary>
/// كل تغيير عهدة يكتب ثلاثة أشياء في حفظ واحد: صفّ سجل، صفّ بريد، وتحديث عدّادات الحزمة.
/// السجل هو مصدر سرعة التدفّق — لا الأختام المنسوخة على المعاملة.
/// </summary>
public static class ArchiveRecorder
{
    public static void Append(
        IApplicationDbContext db,
        ArchiveTransaction transaction,
        ArchiveEventType eventType,
        ArchiveTransactionStatus previousStatus,
        ArchiveStage? stage,
        int actorUserId,
        string actorName,
        DateTimeOffset atUtc,
        Guid operationId,
        string? note = null)
    {
        db.ArchiveTransactionEvents.Add(new ArchiveTransactionEvent(
            transaction.Id,
            eventType,
            previousStatus,
            transaction.Status,
            stage,
            actorUserId,
            actorName,
            atUtc,
            operationId,
            Trim(note)));
    }

    public static void Enqueue(
        IApplicationDbContext db,
        ArchiveBundle bundle,
        ArchiveTransaction? transaction,
        ArchiveEventType changeType,
        ArchiveStage? targetStage,
        DateTimeOffset atUtc)
    {
        db.ArchiveChangeOutboxes.Add(new ArchiveChangeOutbox(
            Guid.NewGuid(),
            bundle.Id,
            transaction?.Id,
            atUtc,
            changeType,
            targetStage,
            bundle.BranchId,
            bundle.SalesEmployeeUserId,
            transaction?.FollowerUserId));
    }

    /// <summary>
    /// يعيد بناء عدّادات الحزمة من معاملاتها. العدّاد ذاكرة مؤقّتة لا حقيقة ثانية،
    /// فإعادة بنائه من الصفوف دائماً تمنع الانحراف.
    /// </summary>
    public static async Task RecomputeCountersAsync(
        IApplicationDbContext db,
        ArchiveBundle bundle,
        CancellationToken cancellationToken)
    {
        var rows = await db.ArchiveTransactions
            .Where(x => x.BundleId == bundle.Id)
            .Select(x => new { x.Status, x.IsVoided, x.HasOpenReturn })
            .ToListAsync(cancellationToken);

        var live = rows.Where(x => x.IsVoided == false).ToList();

        bundle.ApplyCounters(
            itemCount: live.Count,
            sentCount: live.Count(x => x.Status != ArchiveTransactionStatus.WithSalesEmployee),
            archivedCount: live.Count(x => x.Status == ArchiveTransactionStatus.Archived),
            openReturnCount: live.Count(x => x.HasOpenReturn));
    }

    public static async Task<Result> SaveWithConflictGuardAsync(
        IApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return ArchiveErrors.ConcurrencyConflict;
        }
    }

    private static string? Trim(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;

        var trimmed = note.Trim();
        return trimmed.Length > ArchiveTransactionEvent.NoteMaxLength
            ? trimmed[..ArchiveTransactionEvent.NoteMaxLength]
            : trimmed;
    }
}
