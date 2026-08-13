namespace Application.Attachments.Commands.Common.Update;

public record UpdateAttachmentResponse
{
    public required string RelativePath { get; set; }
    public required string OriginalFileName { get; set; }
}