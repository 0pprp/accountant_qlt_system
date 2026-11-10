using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.Transfer;

public class TransferManagementTaskHandler
    : IRequestHandler<TransferManagementTaskCommand, Result<ManagementTaskActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public TransferManagementTaskHandler(
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

    public async Task<Result<ManagementTaskActionResultDto>> Handle(
        TransferManagementTaskCommand request,
        CancellationToken cancellationToken)
    {
        var (context, error) = await ManagementTaskActionHelper.PrepareAsync(
            _db,
            _currentUser,
            SeedPermissions.ManagementTask.Transfer,
            request.TaskId,
            request.ExpectedVersion,
            cancellationToken);
        if (error is not null || context is null)
            return error ?? ManagementTaskErrors.NotFound;

        var task = context.Task;

        if (request.TargetUserId <= 0 || request.TargetUserId == context.ActorId)
            return ManagementTaskErrors.TransferTargetNotEligible;

        var (target, targetError) = await ManagementTaskAccess.ResolveEligibleUserAsync(
            _db, request.TargetUserId, task.BranchId, cancellationToken);
        if (targetError is not null || target is null)
            return ManagementTaskErrors.TransferTargetNotEligible;

        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        var previousStatus = task.Status;
        var at = _clock.UtcNowOffset;

        if (task.TryTransfer(
                context.ActorId,
                target.Id,
                target.FullName,
                note,
                at,
                out var transitionError) == false)
            return ManagementTaskActionHelper.MapTransitionError(transitionError);

        return await ManagementTaskActionHelper.CommitAsync(
            _db,
            _activityLog,
            context,
            previousStatus,
            ManagementTaskEventType.Transferred,
            ManagementTaskChangeType.Transferred,
            at,
            target.Id,
            target.FullName,
            note,
            cancellationToken);
    }
}
