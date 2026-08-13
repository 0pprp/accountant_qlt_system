namespace Application.Orders.Commands.Admin.SetSellerInfo;

public record SetOrderSellerInfoCommand : IRequest<Result>
{
    public int OrderId { get; set; }
    public required string CreationAddress { get; set; }
    public required DateOnly SaleDate { get; set; }
    public required TimeOnly SaleTime { get; set; }
    public int SellerId { get; set; }
    public int OrderListId { get; set; }
}

public record SetOrderSellerInfoDto
{
    public required string CreationAddress { get; set; }
    public required DateOnly SaleDate { get; set; }
    public required TimeOnly SaleTime { get; set; }
    public int SellerId { get; set; }
    public int OrderListId { get; set; }
}