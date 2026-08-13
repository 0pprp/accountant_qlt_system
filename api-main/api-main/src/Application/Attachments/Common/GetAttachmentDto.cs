using Domain.Entities.AttachmentAggregate.Enums;

namespace Application.Attachments.Common;

public record GetAttachmentDto
{
    public int Id { get; set; }
    public required string OriginalFileName { get; set; }
    public required string RelativePath { get; set; }
    public long FileSizeInByte { get; set; }
    public AttachmentType Type { get; set; }
}