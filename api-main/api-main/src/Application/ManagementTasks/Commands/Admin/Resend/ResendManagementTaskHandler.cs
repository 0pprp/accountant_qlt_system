using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.Resend;

public class ResendManagementTaskHandler
    : IRequestHandler<ResendManagementTaskCommand, Result<ManagementTaskActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public ResendManagementTaskHandler(
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
        ResendManagementTaskCommand request,
        CancellationToken cancellationToken)
    {
        var (context, error) = await ManagementTaskActionHelper.PrepareAsync(
            _db,
            _currentUser,
            SeedPermissions.ManagementTask.Assign,
            request.TaskId,
            request.ExpectedVersion,
            cancellationToken);
        if (error is not null || context is null)
            return error ?? ManagementTaskErrors.NotFound;

        var task = context.Task;

        if (task.Status != ManagementTaskStatus.Returned)
            return ManagementTaskErrors.InvalidTransition;

        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return ManagementTaskErrors.TitleRequired;

        if (request.DueDate == default)
            return ManagementTaskErrors.DueRequired;

        if (Enum.IsDefined(request.Priority) == false)
            return ManagementTaskErrors.InvalidPriority;

        var (assignee, assigneeError) = await ManagementTaskAccess.ResolveEligibleUserAsync(
            _db, request.AssigneeUserId, task.BranchId, cancellationToken);
        if (assigneeError is not null || assignee is null)
            return assigneeError ?? ManagementTaskErrors.AssigneeNotEligible;

        var customerId = ManagementTaskActionHelper.NormalizeLink(request.CustomerId);
        var orderId = ManagementTaskActionHelper.NormalizeLink(request.OrderId);

        var linkError = await ManagementTaskActionHelper.ValidateLinkedEntitiesAsync(
            _db, task.BranchId, customerId, orderId, cancellationToken);
        if (linkError is not null)
            return linkError.Value;

        var previousStatus = task.Status;
        var at = _clock.UtcNowOffset;

        if (task.TryResend(
                assignee.Id,
                assignee.FullName,
                title,
                request.Description.Trim(),
                request.Priority,
                request.DueDate,
                request.DueAtUtc,
                customerId,
                orderId,
                at,
                out var transitionError) == false)
            return ManagementTaskActionHelper.MapTransitionError(transitionError);

        return await ManagementTaskActionHelper.CommitAsync(
            _db,
            _activityLog,
            context,
            previousStatus,
            ManagementTaskEventType.Updated,
            ManagementTaskChangeType.Updated,
            at,
            assignee.Id,
            assignee.FullName,
            null,
            cancellationToken);
    }
}
