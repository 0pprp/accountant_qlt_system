using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.Create;

public class CreateManagementTaskHandler
    : IRequestHandler<CreateManagementTaskCommand, Result<ManagementTaskActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public CreateManagementTaskHandler(
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
        CreateManagementTaskCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.ManagementTask.Create) == false)
            return ManagementTaskErrors.ActionForbidden;

        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ManagementTaskErrors.ActionForbidden;

        var branchError = ManagementTaskAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return ManagementTaskErrors.TitleRequired;

        if (request.DueDate == default)
            return ManagementTaskErrors.DueRequired;

        if (Enum.IsDefined(request.Priority) == false)
            return ManagementTaskErrors.InvalidPriority;

        var (assignee, assigneeError) = await ManagementTaskAccess.ResolveEligibleUserAsync(
            _db, request.AssigneeUserId, request.BranchId, cancellationToken);
        if (assigneeError is not null || assignee is null)
            return assigneeError ?? ManagementTaskErrors.AssigneeNotEligible;

        var customerId = ManagementTaskActionHelper.NormalizeLink(request.CustomerId);
        var orderId = ManagementTaskActionHelper.NormalizeLink(request.OrderId);

        var linkError = await ManagementTaskActionHelper.ValidateLinkedEntitiesAsync(
            _db, request.BranchId, customerId, orderId, cancellationToken);
        if (linkError is not null)
            return linkError.Value;

        var actorName = await ManagementTaskActionHelper.ResolveUserNameAsync(_db, actorId, cancellationToken);
        if (actorName is null)
            return ManagementTaskErrors.ActionForbidden;

        var at = _clock.UtcNowOffset;

        var task = new ManagementTask(
            title,
            request.Description.Trim(),
            request.Priority,
            request.DueDate,
            request.DueAtUtc,
            request.BranchId,
            actorId,
            assignee.Id,
            assignee.FullName,
            customerId,
            orderId);

        _db.ManagementTasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        ManagementTaskEventWriter.Append(
            _db,
            task,
            ManagementTaskEventType.Created,
            at,
            actorId,
            actorName,
            null,
            task.Status,
            assignee.Id,
            assignee.FullName);

        ManagementTaskChangeOutboxWriter.Append(_db, task, ManagementTaskChangeType.Created, at);

        await _activityLog.AddAsync(
            task.BranchId,
            ActivityType.ManagementTaskCreated,
            TargetEntityType.ManagementTask,
            task.Id,
            null,
            $"{task.Title} | {assignee.FullName}");

        await _db.SaveChangesAsync(cancellationToken);

        return ManagementTaskMapper.ToActionResult(task);
    }
}
