using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Queries.App.GetAll;

public record GetAllOrdersResponse
{
    public int Id { get; set; }
    public double SellAmount { get; set; }
    public double PaidAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreationAddress { get; set; }
    public OrderStep Step { get; set; }
    public OrderApprovalStatus ApprovalStatus { get; set; }
    public int? OrderListId { get; set; }
    public required CustomerDto Customer { get; set; }
    public int? InstallmentPaymentId { get; set; }
    public required List<GetMinimalOrderItemDto> OrderItems { get; set; }
}

public record CustomerDto
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
}

public record GetMinimalOrderItemDto
{
    public int Id { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public double SellAmount { get; set; }
}