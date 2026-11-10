using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.Confirm;

public class ConfirmManagementTaskHandler
    : IRequestHandler<ConfirmManagementTaskCommand, Result<ManagementTaskActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public ConfirmManagementTaskHandler(
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
        ConfirmManagementTaskCommand request,
        CancellationToken cancellationToken)
    {
        var (context, error) = await ManagementTaskActionHelper.PrepareAsync(
            _db,
            _currentUser,
            SeedPermissions.ManagementTask.Confirm,
            request.TaskId,
            request.ExpectedVersion,
            cancellationToken);
        if (error is not null || context is null)
            return error ?? ManagementTaskErrors.NotFound;

        var previousStatus = context.Task.Status;
        var at = _clock.UtcNowOffset;

        if (context.Task.TryConfirm(context.ActorId, at, out var transitionError) == false)
            return ManagementTaskActionHelper.MapTransitionError(transitionError);

        return await ManagementTaskActionHelper.CommitAsync(
            _db,
            _activityLog,
            context,
            previousStatus,
            ManagementTaskEventType.Confirmed,
            ManagementTaskChangeType.Confirmed,
            at,
            null,
            null,
            null,
            cancellationToken);
    }
}
