using Domain.Entities.AttachmentAggregate.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Orders.Common;

public record CreateOrderAttachmentDto
{
    public required IFormFile File { get; set; }
    public AttachmentType Type { get; set; }
}