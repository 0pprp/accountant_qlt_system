namespace Application.Safes.Commands.Admin.CreateCashDelivery;

public record CreateCashDeliveryCommand : IRequest<Result>
{
    public int SafeId { get; set; }
    public int SellerId { get; set; }
    public double Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
}

public record CreateCashDeliveryTransactionDto
{
    public double Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
}