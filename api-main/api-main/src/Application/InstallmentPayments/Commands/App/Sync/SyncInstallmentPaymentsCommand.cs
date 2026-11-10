using Application.InstallmentPayments.Common;

namespace Application.InstallmentPayments.Commands.App.Sync;

public record SyncInstallmentPaymentsCommand : IRequest<Result<SyncInstallmentPaymentsResultDto>>
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
    public Guid? ClientPaymentId { get; set; }
}

public record SyncInstallmentPaymentsResultDto(
    IReadOnlyList<InstallmentPaymentWriteResultDto> Items);
