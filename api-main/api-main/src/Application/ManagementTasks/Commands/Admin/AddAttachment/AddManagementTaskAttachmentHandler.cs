using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Commands.Admin.AddAttachment;

public class AddManagementTaskAttachmentHandler
    : IRequestHandler<AddManagementTaskAttachmentCommand, Result<ManagementTaskAttachmentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IFileManager _fileManager;
    private readonly IActivityLogService _activityLog;

    public AddManagementTaskAttachmentHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        IFileManager fileManager,
        IActivityLogService activityLog)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _fileManager = fileManager;
        _activityLog = activityLog;
    }

    public async Task<Result<ManagementTaskAttachmentDto>> Handle(
        AddManagementTaskAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.ManagementTask.Create) == false)
            return ManagementTaskErrors.ActionForbidden;

        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return ManagementTaskErrors.ActionForbidden;

        if (request.File is null || request.File.Length <= 0)
            return ManagementTaskErrors.AttachmentRequired;

        if (request.File.Length > ManagementTaskMapper.MaxAttachmentBytes)
            return ManagementTaskErrors.AttachmentTooLarge;

        var extension = Path.GetExtension(request.File.FileName);
        if (ManagementTaskMapper.IsAllowedExtension(extension) == false)
            return ManagementTaskErrors.AttachmentTypeDenied;

        var task = await _db.ManagementTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);

        if (task is null || ManagementTaskAccess.CanSeeTask(_currentUser, task) == false)
            return ManagementTaskErrors.NotFound;

        var at = _clock.UtcNowOffset;

        var saved = await _fileManager.SaveFileAsync(
            request.File,
            Path.Combine(AttachmentPath.Root, AttachmentPath.ManagementTasksSubfolder(task.Id)),
            cancellationToken);

        var attachment = new ManagementTaskAttachment(
            task.Id,
            request.File.FileName,
            saved.Name,
            saved.RelativePath,
            request.File.ContentType,
            saved.Extension,
            request.File.Length,
            actorId,
            at);

        try
        {
            _db.ManagementTaskAttachments.Add(attachment);

            _db.ManagementTaskChangeOutboxes.Add(new ManagementTaskChangeOutbox(
                Guid.NewGuid(),
                task.Id,
                task.ConcurrencyStamp,
                at,
                ManagementTaskChangeType.AttachmentAdded,
                task.BranchId,
                task.CurrentHolderUserId,
                task.AssigneeUserId));

            await _activityLog.AddAsync(
                task.BranchId,
                ActivityType.ManagementTaskUpdated,
                TargetEntityType.ManagementTask,
                task.Id,
                null,
                $"AttachmentAdded: {task.Title} | {attachment.OriginalFileName}");

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _fileManager.Delete(saved.RelativePath);
            throw;
        }

        return ManagementTaskMapper.ToAttachment(attachment);
    }
}
