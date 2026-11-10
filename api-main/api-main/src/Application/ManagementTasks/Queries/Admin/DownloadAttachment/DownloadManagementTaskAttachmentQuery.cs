namespace Application.ManagementTasks.Queries.Admin.DownloadAttachment;

public sealed record ManagementTaskFileDownloadDto(string FileName, string ContentType, string RelativePath);

public record DownloadManagementTaskAttachmentQuery(int TaskId, int AttachmentId)
    : IRequest<Result<ManagementTaskFileDownloadDto>>;
