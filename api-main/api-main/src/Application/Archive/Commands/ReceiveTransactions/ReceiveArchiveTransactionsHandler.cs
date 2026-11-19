using Application.Archive.Common;
using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Commands.ReceiveTransactions;

public class ReceiveArchiveTransactionsHandler
    : IRequestHandler<ReceiveArchiveTransactionsCommand, Result<ArchiveActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public ReceiveArchiveTransactionsHandler(
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
        ReceiveArchiveTransactionsCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ArchiveErrors.ActionForbidden;

        if (ArchiveAccess.CanReceiveAt(_currentUser, request.AtStage) == false)
            return ArchiveErrors.StageForbidden;

        var ids = request.TransactionIds.Distinct().ToList();
        if (ids.Count == 0)
            return ArchiveErrors.NothingToReceive;

        var expected = ArchiveTransaction.ReceiveSourceStatus(request.AtStage);
        if (expected is null)
            return ArchiveErrors.StageForbidden;

        var rows = await _db.ArchiveTransactions
            .Include(x => x.Bundle)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return ArchiveErrors.TransactionNotFound;

        var branches = ArchiveAccess.AuthorizedBranches(_currentUser);
        if (rows.Any(x => branches.Contains(x.Bundle!.BranchId) == false))
            return ArchiveErrors.BranchForbidden;

        var at = _clock.UtcNowOffset;
        var operationId = Guid.NewGuid();
        var actorName = await ResolveActorNameAsync(actorId, cancellationToken);
        var received = 0;

        foreach (var transaction in rows)
        {
            var previous = transaction.Status;
            if (transaction.TryReceive(request.AtStage, actorId, at, out _) == false)
                continue;

            ArchiveRecorder.Append(
                _db, transaction, ArchiveEventType.Received, previous, request.AtStage,
                actorId, actorName, at, operationId, request.Note);
            received++;
        }

        var bundles = rows.Select(x => x.Bundle!).DistinctBy(x => x.Id).ToList();
        foreach (var bundle in bundles)
        {
            await ArchiveRecorder.RecomputeCountersAsync(_db, bundle, cancellationToken);
            ArchiveRecorder.Enqueue(_db, bundle, null, ArchiveEventType.Received, request.AtStage, at);

            await _activityLog.AddAsync(
                bundle.BranchId,
                ActivityType.ArchiveCustodyChanged,
                TargetEntityType.ArchiveBundle,
                bundle.Id,
                null,
                $"استلام {received} معاملة في {ArchiveLabels.Stage(request.AtStage)}");
        }

        var saved = await ArchiveRecorder.SaveWithConflictGuardAsync(_db, cancellationToken);
        if (saved.IsFailed)
            return saved.Error!.Value;

        var skipped = ids.Count - received;
        return new ArchiveActionResultDto
        {
            BundleId = bundles.Count == 1 ? bundles[0].Id : 0,
            AffectedCount = received,
            SkippedCount = skipped,
            Message = skipped == 0
                ? $"استُلمت {received} معاملة"
                : $"استُلمت {received} معاملة، وتُجووِزت {skipped} مستلمة سلفاً أو ليست بانتظارك"
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
