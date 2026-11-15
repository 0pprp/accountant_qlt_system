using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.CreateFanout;

public class CreateManagementTasksFanoutHandler
    : IRequestHandler<CreateManagementTasksFanoutCommand, Result<ManagementTaskFanoutResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activityLog;

    public CreateManagementTasksFanoutHandler(
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

    public async Task<Result<ManagementTaskFanoutResultDto>> Handle(
        CreateManagementTasksFanoutCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.ManagementTask.Create) == false)
            return ManagementTaskErrors.ActionForbidden;

        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ManagementTaskErrors.ActionForbidden;

        if (Enum.IsDefined(request.Audience) == false)
            return ManagementTaskErrors.InvalidAudience;

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

        var at = _clock.UtcNowOffset;
        var (recipients, resolveError) = await ManagementTaskAudienceResolver.ResolveAsync(
            _db,
            request.Audience,
            request.BranchId,
            request.AssigneeUserId,
            at,
            cancellationToken);
        if (resolveError is not null || recipients is null)
            return resolveError ?? ManagementTaskErrors.AudienceEmpty;

        var customerId = ManagementTaskActionHelper.NormalizeLink(request.CustomerId);
        var orderId = ManagementTaskActionHelper.NormalizeLink(request.OrderId);

        var linkError = await ManagementTaskActionHelper.ValidateLinkedEntitiesAsync(
            _db, request.BranchId, customerId, orderId, cancellationToken);
        if (linkError is not null)
            return linkError.Value;

        var actorName = await ManagementTaskActionHelper.ResolveUserNameAsync(_db, actorId, cancellationToken);
        if (actorName is null)
            return ManagementTaskErrors.ActionForbidden;

        var description = request.Description.Trim();
        var createdIds = new List<int>(recipients.Count);

        foreach (var recipient in recipients)
        {
            var task = new ManagementTask(
                title,
                description,
                request.Priority,
                request.DueDate,
                request.DueAtUtc,
                recipient.BranchId,
                actorId,
                recipient.UserId,
                recipient.DisplayName,
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
                recipient.UserId,
                recipient.DisplayName);

            ManagementTaskChangeOutboxWriter.Append(_db, task, ManagementTaskChangeType.Created, at);

            await _activityLog.AddAsync(
                task.BranchId,
                ActivityType.ManagementTaskCreated,
                TargetEntityType.ManagementTask,
                task.Id,
                null,
                $"{task.Title} | {recipient.DisplayName}");

            await _db.SaveChangesAsync(cancellationToken);
            createdIds.Add(task.Id);
        }

        return new ManagementTaskFanoutResultDto(createdIds.Count, createdIds);
    }
}
