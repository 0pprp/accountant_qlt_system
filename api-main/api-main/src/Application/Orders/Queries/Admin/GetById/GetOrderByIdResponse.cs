using Application.Attachments.Common;
using Application.Orders.Common;

namespace Application.Orders.Queries.Admin.GetById;

public record GetOrderByIdResponse
{
    public required OrderInfoDto OrderInfo { get; set; }
    public SellerInfoDto? SellerInfo { get; set; }
}

public record OrderInfoDto
{
    public int Id { get; set; }
    public required List<OrderItemDto> OrderItems { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PrepaymentAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public required List<GetAttachmentDto> Attachments { get; set; }
}

public record SellerInfoDto
{
    public required SellerDto Seller { get; set; }
    public required OrderListDto OrderList { get; set; }
    public string? CreationAddress { get; set; }
    public DateOnly SaleDate { get; set; }
    public TimeOnly SaleTime { get; set; }
}

public record SellerDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public record OrderListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}