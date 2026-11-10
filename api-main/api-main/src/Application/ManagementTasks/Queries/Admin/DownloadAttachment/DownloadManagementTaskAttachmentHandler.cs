using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Queries.Admin.DownloadAttachment;

public class DownloadManagementTaskAttachmentHandler
    : IRequestHandler<DownloadManagementTaskAttachmentQuery, Result<ManagementTaskFileDownloadDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DownloadManagementTaskAttachmentHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<ManagementTaskFileDownloadDto>> Handle(
        DownloadManagementTaskAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.ManagementTask.ReadAttachment) == false)
            return ManagementTaskErrors.AttachmentForbidden;

        var task = await _db.ManagementTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, cancellationToken);

        if (task is null || ManagementTaskAccess.CanSeeTask(_currentUser, task) == false)
            return ManagementTaskErrors.NotFound;

        var attachment = await _db.ManagementTaskAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.AttachmentId && x.TaskId == request.TaskId,
                cancellationToken);

        if (attachment is null)
            return ManagementTaskErrors.AttachmentNotFound;

        return new ManagementTaskFileDownloadDto(
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.RelativePath);
    }
}
