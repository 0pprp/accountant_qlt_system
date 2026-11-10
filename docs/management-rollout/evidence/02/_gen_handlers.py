# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(r"C:\GitHub\accountant_qlt_system\api-main\api-main\src\Application\ManagementTasks")

def w(rel: str, content: str) -> None:
    path = ROOT / rel
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.lstrip("\n"), encoding="utf-8")
    print("W", path)

w(
    "Commands/Admin/Create/CreateManagementTaskCommand.cs",
    """
using Application.Common;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;
using FluentValidation;
using MediatR;

namespace Application.ManagementTasks.Commands.Admin.Create;

public record CreateManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ManagementTaskPriority Priority { get; init; } = ManagementTaskPriority.Medium;
    public DateOnly DueDate { get; init; }
    public DateTimeOffset? DueAtUtc { get; init; }
    public int BranchId { get; init; }
    public int AssigneeUserId { get; init; }
    public int? CustomerId { get; init; }
    public int? OrderId { get; init; }
}

public class CreateManagementTaskCommandValidator : AbstractValidator<CreateManagementTaskCommand>
{
    public CreateManagementTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(8000);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.AssigneeUserId).GreaterThan(0);
        RuleFor(x => x.Priority).IsInEnum();
    }
}
""",
)

w(
    "Commands/Admin/Create/CreateManagementTaskHandler.cs",
    """
using Application.Common;
using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.Create;

public class CreateManagementTaskHandler : IRequestHandler<CreateManagementTaskCommand, Result<ManagementTaskActionResultDto>>
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
            return branchError;

        if (Enum.IsDefined(request.Priority) == false
            || request.Priority is < ManagementTaskPriority.High or > ManagementTaskPriority.Low)
            return ManagementTaskErrors.InvalidPriority;

        if (request.DueDate == default && request.DueAtUtc is null)
            return ManagementTaskErrors.DueRequired;

        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            return ManagementTaskErrors.TitleRequired;

        var (assignee, assigneeError) = await ManagementTaskAccess.ResolveEligibleUserAsync(
            _db, request.AssigneeUserId, request.BranchId, cancellationToken);
        if (assigneeError is not null)
            return assigneeError;

        if (request.CustomerId is int customerId)
        {
            var customerOk = await _db.Customers.AsNoTracking()
                .AnyAsync(c => c.Id == customerId && c.BranchId == request.BranchId, cancellationToken);
            if (customerOk == false)
                return ManagementTaskErrors.LinkedEntityBranchMismatch;
        }

        if (request.OrderId is int orderId)
        {
            var orderOk = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && o.BranchId == request.BranchId, cancellationToken);
            if (orderOk == false)
                return ManagementTaskErrors.LinkedEntityBranchMismatch;
        }

        var at = _clock.UtcNowOffset;
        var dueDate = request.DueDate;
        if (dueDate == default && request.DueAtUtc is not null)
            dueDate = ManagementTaskMapper.IraqToday(request.DueAtUtc.Value);

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var entity = new ManagementTask(
            title,
            (request.Description ?? string.Empty).Trim(),
            request.Priority,
            dueDate,
            request.DueAtUtc,
            request.BranchId,
            actorId,
            assignee!.Id,
            assignee.FullName,
            request.CustomerId,
            request.OrderId);

        _db.ManagementTasks.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        ManagementTaskEventWriter.Append(
            _db, entity, ManagementTaskEventType.Created, at, actorId, actorName,
            null, ManagementTaskStatus.Sent, assignee.Id, assignee.FullName);

        ManagementTaskChangeOutboxWriter.Append(_db, entity, ManagementTaskChangeType.Created, at);

        await _activityLog.AddAsync(
            request.BranchId,
            ActivityType.ManagementTaskCreated,
            TargetEntityType.ManagementTask,
            entity.Id,
            details: $"status=Sent; assignee={assignee.Id}; before=none after=Sent");

        var save = await ManagementTaskConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct), cancellationToken);
        if (save.IsFailed)
            return save.Error!;

        return ManagementTaskMapper.ToActionResult(entity);
    }
}
""",
)

print("partial ok")
