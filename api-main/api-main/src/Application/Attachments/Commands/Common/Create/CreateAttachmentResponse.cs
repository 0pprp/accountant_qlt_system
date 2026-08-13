namespace Application.Attachments.Commands.Common.Create;

public record CreateAttachmentResponse
{
    public int Id { get; set; }
    public required string RelativePath { get; set; }
    public required string OriginalFileName { get; set; }
}