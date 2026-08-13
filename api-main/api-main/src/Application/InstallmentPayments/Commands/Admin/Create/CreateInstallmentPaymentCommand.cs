namespace Application.InstallmentPayments.Commands.Admin.Create;

public record CreateInstallmentPaymentCommand : IRequest<Result>
{
    public int OrderId { get; set; }
    public DateOnly Date { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
}