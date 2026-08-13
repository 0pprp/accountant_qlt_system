using Domain.Entities.AttachmentAggregate.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Attachments.Commands.Common.Create;

public record CreateAttachmentCommand : IRequest<Result<CreateAttachmentResponse>>
{
    public required IFormFile File { get; set; }
    public AttachmentType Type { get; set; }
    public int? UserId { get; set; }
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
}