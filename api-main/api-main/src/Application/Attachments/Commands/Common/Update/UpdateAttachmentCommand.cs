using Microsoft.AspNetCore.Http;

namespace Application.Attachments.Commands.Common.Update;

public record UpdateAttachmentCommand : IRequest<Result<UpdateAttachmentResponse>>
{
    public int Id { get; set; }
    public required IFormFile File { get; set; }
}

public record UpdateAttachmentDto(IFormFile File);