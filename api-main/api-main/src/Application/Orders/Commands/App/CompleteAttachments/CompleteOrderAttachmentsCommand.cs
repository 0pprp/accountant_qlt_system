namespace Application.Orders.Commands.App.CompleteAttachments;

public record CompleteOrderAttachmentsCommand : IRequest<Result>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}
