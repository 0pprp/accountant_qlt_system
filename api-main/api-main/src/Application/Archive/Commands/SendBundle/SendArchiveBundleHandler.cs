using Application.Archive.Common;
using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Commands.SendBundle;

public class SendArchiveBundleHandler
    : IRequestHandler<SendArchiveBundleCommand, Result<ArchiveActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public SendArchiveBundleHandler(
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
        SendArchiveBundleCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ArchiveErrors.ActionForbidden;

        var bundle = await _db.ArchiveBundles
            .FirstOrDefaultAsync(x => x.Id == request.BundleId, cancellationToken);

        if (bundle is null)
            return ArchiveErrors.BundleNotFound;

        var denied = ArchiveAccess.EnsureCanSend(_currentUser, request.FromStage, bundle);
        if (denied is not null)
            return denied.Value;

        // بوابة العشرة تخصّ المصدر وحده — المراحل الوسطى تمرّر ما استلمته متى شاءت.
        var baghdadToday = ArchiveIntake.ToBaghdadDate(_clock.UtcNowOffset);
        if (request.FromStage == ArchiveStage.SalesEmployee && bundle.CanSendOn(baghdadToday) == false)
            return ArchiveErrors.PeriodNotClosed;

        var sourceStatus = Domain.Entities.ArchiveAggregate.ArchiveTransaction.SendSourceStatus(request.FromStage);
        if (sourceStatus is null)
            return ArchiveErrors.StageForbidden;

        var candidates = await _db.ArchiveTransactions
            .Where(x => x.BundleId == bundle.Id && x.Status == sourceStatus && x.IsVoided == false)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
            return ArchiveErrors.NothingToSend;

        var at = _clock.UtcNowOffset;
        var operationId = Guid.NewGuid();
        var actorName = await ResolveActorNameAsync(actorId, cancellationToken);
        var moved = 0;

        foreach (var transaction in candidates)
        {
            var previous = transaction.Status;
            if (transaction.TrySend(request.FromStage, at, out _) == false)
                continue;

            ArchiveRecorder.Append(
                _db, transaction, ArchiveEventType.Sent, previous, request.FromStage,
                actorId, actorName, at, operationId, request.Note);
            moved++;
        }

        if (moved == 0)
            return ArchiveErrors.NothingToSend;

        if (request.FromStage == ArchiveStage.SalesEmployee)
            bundle.MarkFirstSent(at);

        await ArchiveRecorder.RecomputeCountersAsync(_db, bundle, cancellationToken);
        ArchiveRecorder.Enqueue(_db, bundle, null, ArchiveEventType.Sent, NextStage(request.FromStage), at);

        await _activityLog.AddAsync(
            bundle.BranchId,
            ActivityType.ArchiveCustodyChanged,
            TargetEntityType.ArchiveBundle,
            bundle.Id,
            null,
            $"إرسال {moved} معاملة من {ArchiveLabels.Stage(request.FromStage)}");

        var saved = await ArchiveRecorder.SaveWithConflictGuardAsync(_db, cancellationToken);
        if (saved.IsFailed)
            return saved.Error!.Value;

        return new ArchiveActionResultDto
        {
            BundleId = bundle.Id,
            AffectedCount = moved,
            SkippedCount = candidates.Count - moved,
            Message = $"أُرسِلت {moved} معاملة إلى {ArchiveLabels.Stage(NextStage(request.FromStage)!.Value)}"
        };
    }

    private static ArchiveStage? NextStage(ArchiveStage from) => from switch
    {
        ArchiveStage.SalesEmployee => ArchiveStage.BranchManagement,
        ArchiveStage.BranchManagement => ArchiveStage.Follower,
        ArchiveStage.Follower => ArchiveStage.Archive,
        _ => null
    };

    private async Task<string> ResolveActorNameAsync(int actorId, CancellationToken cancellationToken)
    {
        var name = await _db.Users.AsNoTracking()
            .Where(x => x.Id == actorId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(name) ? "—" : name;
    }
}
