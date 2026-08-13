namespace Application.Purchases.Commands.Admin.Update;

public record UpdatePurchaseCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required List<PurchaseItemDto> PurchaseItems { get; set; }
}

public record UpdatePurchaseDto
{
    public required List<PurchaseItemDto> PurchaseItems { get; set; }
}

public record PurchaseItemDto
{
    public int? Id { get; set; }
    public int? ProductId { get; set; }
    public string? ForeignProductName { get; set; }
    public required int Quantity { get; set; }
    public required double Amount { get; set; }
}