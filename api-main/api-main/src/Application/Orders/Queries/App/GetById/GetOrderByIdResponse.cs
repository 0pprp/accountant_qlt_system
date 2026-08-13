using Application.Attachments.Common;
using Application.Orders.Common;
using Application.Orders.Queries.Admin.GetById;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Queries.App.GetById;

public record GetOrderByIdResponse
{
    public int Id { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PrepaymentAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public double PaidAmount { get; set; }
    public double OverdueAmount { get; set; }
    public double UnpaidAmount => SellAmount - PaidAmount;
    public Location? Location { get; set; }
    public string? CreationAddress { get; set; }
    public DateOnly? SaleDate { get; set; }
    public TimeOnly? SaleTime { get; set; }
    public OrderStep Step { get; set; }
    public OrderExecutionStatus ExecutionStatus { get; set; }
    public OrderApprovalStatus ApprovalStatus { get; set; }
    public required SellerDto Seller { get; set; }
    public OrderListDto? OrderList { get; set; }
    public required List<GetAttachmentDto> Attachments { get; set; }
    public required List<OrderItemDto> OrderItems { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}