using Application.InstallmentPayments.Common;
using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.InstallmentPayments.Commands.App.Sync;

public class SyncInstallmentPaymentsHandler
    : IRequestHandler<SyncInstallmentPaymentsCommand, Result<SyncInstallmentPaymentsResultDto>>
{
    private readonly AppInstallmentPaymentWriter _writer;

    public SyncInstallmentPaymentsHandler(AppInstallmentPaymentWriter writer) => _writer = writer;

    public async Task<Result<SyncInstallmentPaymentsResultDto>> Handle(
        SyncInstallmentPaymentsCommand request,
        CancellationToken cancellationToken)
    {
        var items = new List<InstallmentPaymentWriteResultDto>();
        foreach (var row in request.InstallmentPayments)
        {
            var result = await _writer.WriteAsync(
                request.UserId,
                row.OrderId,
                row.Amount,
                row.Date,
                row.ClientPaymentId,
                ActivityType.InstallmentPaymentsSynced,
                cancellationToken);
            if (!result.IsSucceeded || result.Data is null)
                return result.Error!;

            items.Add(result.Data);
        }

        return new SyncInstallmentPaymentsResultDto(items);
    }
}
