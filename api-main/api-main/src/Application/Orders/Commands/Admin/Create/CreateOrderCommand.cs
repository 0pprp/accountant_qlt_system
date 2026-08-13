using Application.Orders.Common;

namespace Application.Orders.Commands.Admin.Create;

public record CreateOrderCommand : IRequest<Result<CreateOrderResponse>>
{
    public required List<CreateOrderItemDto> OrderItems { get; set; }
    public int CustomerId { get; set; }
    public required List<CreateOrderAttachmentDto> Attachments { get; set; }
}