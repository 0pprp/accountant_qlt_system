namespace Application.InstallmentPayments.Commands.Admin.Delete;

public record DeleteInstallmentPaymentCommand(int Id) : IRequest<Result>;