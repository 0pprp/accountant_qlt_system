using Application.InstallmentPayments.Queries.App.GetPaginated;

namespace Application.InstallmentPayments.Queries.App.GetSummary;

public record GetInstallmentPaymentsSummaryQuery : IRequest<Result<GetInstallmentPaymentsSummaryResponse>>
{
    public required GetPaginatedInstallmentPaymentsFilter Filter { get; set; }
    public int UserId { get; set; }
}