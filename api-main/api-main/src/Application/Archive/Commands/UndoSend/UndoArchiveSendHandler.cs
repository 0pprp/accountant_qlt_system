using Application.SalesRequests.Common;
using Application.Archive.Common;
using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Archive.Commands.UndoSend;

public class UndoArchiveSendHandler
    : IRequestHandler<UndoArchiveSendCommand, Result<ArchiveActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public UndoArchiveSendHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        IActivityLogService activityLog)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _activityLog = activityLog;
    }

    public async Task<Result<ArchiveActionResultDto>> Handle(
        UndoArchiveSendCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ArchiveErrors.ActionForbidden;

        if (_currentUser.HasPermission(SeedPermissions.Archive.UndoSend) == false)
            return ArchiveErrors.ActionForbidden;

        var ids = request.TransactionIds.Distinct().ToList();
        var rows = await _db.ArchiveTransactions
            .Include(x => x.Bundle)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return ArchiveErrors.TransactionNotFound;

        foreach (var bundle in rows.Select(x => x.Bundle!).DistinctBy(x => x.Id))
        {
            var denied = ArchiveAccess.EnsureCanSend(_currentUser, request.FromStage, bundle);
            if (denied is not null)
                return denied.Value;
        }

        var at = _clock.UtcNowOffset;
        var operationId = Guid.NewGuid();
        var actorName = await ResolveActorNameAsync(actorId, cancellationToken);
        var undone = 0;

        foreach (var transaction in rows)
        {
            var previous = transaction.Status;
            if (transaction.TryUndoSend(request.FromStage, at, out _) == false)
                continue;

            ArchiveRecorder.Append(
                _db, transaction, ArchiveEventType.SendUndone, previous, request.FromStage,
                actorId, actorName, at, operationId, request.Note);
            undone++;
        }

        if (undone == 0)
            return ArchiveErrors.TransactionNotFound;

        var bundles = rows.Select(x => x.Bundle!).DistinctBy(x => x.Id).ToList();
        foreach (var bundle in bundles)
        {
            await ArchiveRecorder.RecomputeCountersAsync(_db, bundle, cancellationToken);
            ArchiveRecorder.Enqueue(_db, bundle, null, ArchiveEventType.SendUndone, request.FromStage, at);

            await _activityLog.AddAsync(
                bundle.BranchId,
                ActivityType.ArchiveCustodyChanged,
                TargetEntityType.ArchiveBundle,
                bundle.Id,
                null,
                $"إلغاء إرسال {undone} معاملة من {ArchiveLabels.Stage(request.FromStage)}");
        }

        var saved = await ArchiveRecorder.SaveWithConflictGuardAsync(_db, cancellationToken);
        if (saved.IsFailed)
            return saved.Error!.Value;

        return new ArchiveActionResultDto
        {
            BundleId = bundles.Count == 1 ? bundles[0].Id : 0,
            AffectedCount = undone,
            SkippedCount = ids.Count - undone,
            Message = $"أُلغي إرسال {undone} معاملة وعادت إلى عهدتك"
        };
    }

    private async Task<string> ResolveActorNameAsync(int actorId, CancellationToken cancellationToken)
    {
        var name = await _db.Users.AsNoTracking()
            .Where(x => x.Id == actorId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(name) ? "—" : name;
    }
}
