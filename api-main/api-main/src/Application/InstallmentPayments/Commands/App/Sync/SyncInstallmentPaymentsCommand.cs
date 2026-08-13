namespace Application.InstallmentPayments.Commands.App.Sync;

public record SyncInstallmentPaymentsCommand : IRequest<Result>
{
    public required List<InstallmentPaymentDto> InstallmentPayments { get; set; }
    public int UserId { get; set; }
}

public record SyncInstallmentPaymentsDto(List<InstallmentPaymentDto> InstallmentPayments);

public record InstallmentPaymentDto
{
    public int OrderId { get; set; }
    public double Amount { get; set; }
    public DateOnly Date { get; set; }
}