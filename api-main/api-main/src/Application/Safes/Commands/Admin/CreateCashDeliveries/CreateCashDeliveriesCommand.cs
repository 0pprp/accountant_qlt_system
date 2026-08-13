namespace Application.Safes.Commands.Admin.CreateCashDeliveries;

public record CreateCashDeliveriesCommand : IRequest<Result>
{
    public int SafeId { get; set; }
    public required int[] SellerIds { get; set; }
}

public record CreateCashDeliveriesDto(int[] SellerIds);