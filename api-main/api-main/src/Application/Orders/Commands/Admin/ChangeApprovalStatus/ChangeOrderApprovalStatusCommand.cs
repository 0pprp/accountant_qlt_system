using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Commands.Admin.ChangeApprovalStatus;

public record ChangeOrderApprovalStatusCommand : IRequest<Result>
{
    public int OrderId { get; set; }
    public OrderApprovalStatus ApprovalStatus { get; set; }
}

public record ChangeOrderApprovalStatusDto(OrderApprovalStatus ApprovalStatus);