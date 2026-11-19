using Application.SalesRequests.Common;
using Application.Archive.Common;
using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Archive.Commands.ReturnTransactions;

public class ReturnArchiveTransactionsHandler
    : IRequestHandler<ReturnArchiveTransactionsCommand, Result<ArchiveActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public ReturnArchiveTransactionsHandler(
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
        ReturnArchiveTransactionsCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ArchiveErrors.ActionForbidden;

        if (_currentUser.HasPermission(SeedPermissions.Archive.Return) == false)
            return ArchiveErrors.ActionForbidden;

        if (request.Destination == ArchiveStage.Archive)
            return ArchiveErrors.InvalidReturnDestination;

        var ids = request.TransactionIds.Distinct().ToList();
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
        var returned = 0;

        foreach (var transaction in rows)
        {
            var previous = transaction.Status;
            if (transaction.TryReturn(request.Destination, request.Reason, at, out _) == false)
                continue;

            ArchiveRecorder.Append(
                _db, transaction, ArchiveEventType.Returned, previous, request.Destination,
                actorId, actorName, at, operationId, request.Reason);
            returned++;
        }

        if (returned == 0)
            return ArchiveErrors.TransactionNotFound;

        var bundles = rows.Select(x => x.Bundle!).DistinctBy(x => x.Id).ToList();
        foreach (var bundle in bundles)
        {
            await ArchiveRecorder.RecomputeCountersAsync(_db, bundle, cancellationToken);
            ArchiveRecorder.Enqueue(_db, bundle, null, ArchiveEventType.Returned, request.Destination, at);

            await _activityLog.AddAsync(
                bundle.BranchId,
                ActivityType.ArchiveCustodyChanged,
                TargetEntityType.ArchiveBundle,
                bundle.Id,
                null,
                $"استرجاع {returned} معاملة إلى {ArchiveLabels.Stage(request.Destination)}: {request.Reason}");
        }

        var saved = await ArchiveRecorder.SaveWithConflictGuardAsync(_db, cancellationToken);
        if (saved.IsFailed)
            return saved.Error!.Value;

        return new ArchiveActionResultDto
        {
            BundleId = bundles.Count == 1 ? bundles[0].Id : 0,
            AffectedCount = returned,
            SkippedCount = ids.Count - returned,
            Message = $"استُرجِعت {returned} معاملة إلى {ArchiveLabels.Stage(request.Destination)}"
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
