using Application.Common.Utilities;

namespace Application.InstallmentPayments.Queries.App.GetDailyPdfReport;

public record GetDailyInstallmentPaymentsPdfReportQuery : IRequest<Result<GetDailyInstallmentPaymentsPdfReportResponse>>
{
    public int UserId { get; set; }
    public PdfSettings? PdfSettings { get; set; }
    public bool ShowPaidInstallments { get; set; }
}

public record GetDailyInstallmentPaymentsPdfReportFilter(bool ShowPaidInstallments);