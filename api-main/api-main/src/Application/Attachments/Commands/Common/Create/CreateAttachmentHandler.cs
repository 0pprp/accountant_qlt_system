using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.AttachmentAggregate;

namespace Application.Attachments.Commands.Common.Create;

public class CreateAttachmentHandler : IRequestHandler<CreateAttachmentCommand, Result<CreateAttachmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileManager _fileManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public CreateAttachmentHandler(IApplicationDbContext dbContext, IFileManager fileManager, ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }
    
    public async Task<Result<CreateAttachmentResponse>> Handle(CreateAttachmentCommand request, CancellationToken cancellationToken)
    {
        var fileDto = await _fileManager.SaveFileAsync(request.File, Path.Combine(AttachmentPath.Root, GetSubfolderName(request)), cancellationToken);

        var attachment = new Attachment(request.File.FileName, fileDto.Name, fileDto.RelativePath, request.File.ContentType,
            fileDto.Extension, request.File.Length, request.Type, AttachmentUtility.GetFileType(request.File.ContentType))
        {
            UserId = request.UserId,
            OrderId = request.OrderId,
            CustomerId = request.CustomerId
        };
        
        _dbContext.Attachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            _currentUserService.BranchIds!.First(),
            ActivityType.AttachmentCreated,
            TargetEntityType.Attachment,
            attachment.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateAttachmentResponse
        {
            Id = attachment.Id,
            OriginalFileName = attachment.OriginalFileName,
            RelativePath = attachment.RelativePath
        };
    }

    private static string GetSubfolderName(CreateAttachmentCommand request)
    {
        if (request.UserId is not null)
            return AttachmentPath.UsersSubfolder(request.UserId.Value);

        if (request.OrderId is not null)
            return AttachmentPath.OrdersSubfolder(request.OrderId.Value);
        
        if (request.CustomerId is not null)
            return AttachmentPath.CustomersSubfolder(request.CustomerId.Value);

        return string.Empty;
    }
}