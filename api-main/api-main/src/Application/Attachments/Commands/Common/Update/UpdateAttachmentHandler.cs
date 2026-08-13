using Application.Attachments.Common;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.AttachmentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Attachments.Commands.Common.Update;

public class UpdateAttachmentHandler : IRequestHandler<UpdateAttachmentCommand, Result<UpdateAttachmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileManager _fileManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public UpdateAttachmentHandler(IApplicationDbContext dbContext, IFileManager fileManager, ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<UpdateAttachmentResponse>> Handle(UpdateAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.Attachments.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (attachment is null)
            return AttachmentErrors.AttachmentNotFound;

        _fileManager.Delete(attachment.RelativePath);
        
        var fileDto = await _fileManager.SaveFileAsync(request.File, Path.Combine(AttachmentPath.Root, GetSubfolderName(attachment)),
            cancellationToken);
        
        attachment.Update(request.File.FileName, fileDto.Name, fileDto.RelativePath, request.File.ContentType, fileDto.Extension, 
            request.File.Length, AttachmentUtility.GetFileType(request.File.ContentType));

        await _activityLogService.AddAsync(
            _currentUserService.BranchIds!.First(),
            ActivityType.AttachmentUpdated,
            TargetEntityType.Attachment,
            request.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAttachmentResponse
        {
            OriginalFileName = attachment.OriginalFileName,
            RelativePath = attachment.RelativePath
        };
    }

    private static string GetSubfolderName(Attachment attachment)
    {
        if (attachment.UserId is not null)
            return AttachmentPath.UsersSubfolder(attachment.UserId.Value);

        if (attachment.OrderId is not null)
            return AttachmentPath.OrdersSubfolder(attachment.OrderId.Value);
        
        if (attachment.CustomerId is not null)
            return AttachmentPath.CustomersSubfolder(attachment.CustomerId.Value);

        return string.Empty;
    }
}