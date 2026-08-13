namespace Application.InstallmentPayments.Commands.App.Create;

public record CreateInstallmentPaymentCommand : IRequest<Result>
{
    public int OrderId { get; set; }
    public double Amount { get; set; }
    public int UserId { get; set; }
}

public record CreateInstallmentPaymentDto(double Amount);