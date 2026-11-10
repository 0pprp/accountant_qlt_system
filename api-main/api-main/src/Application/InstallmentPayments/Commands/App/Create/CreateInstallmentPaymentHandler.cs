using Application.InstallmentPayments.Common;
using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.InstallmentPayments.Commands.App.Create;

public class CreateInstallmentPaymentHandler
    : IRequestHandler<CreateInstallmentPaymentCommand, Result<InstallmentPaymentWriteResultDto>>
{
    private readonly AppInstallmentPaymentWriter _writer;

    public CreateInstallmentPaymentHandler(AppInstallmentPaymentWriter writer) => _writer = writer;

    public Task<Result<InstallmentPaymentWriteResultDto>> Handle(
        CreateInstallmentPaymentCommand request,
        CancellationToken cancellationToken)
        => _writer.WriteAsync(
            request.UserId,
            request.OrderId,
            request.Amount,
            request.Date,
            request.ClientPaymentId,
            ActivityType.InstallmentPaymentCreated,
            cancellationToken);
}
