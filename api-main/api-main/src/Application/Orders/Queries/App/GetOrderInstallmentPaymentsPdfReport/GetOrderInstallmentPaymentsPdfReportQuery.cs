using Application.Common.Utilities;

namespace Application.Orders.Queries.App.GetOrderInstallmentPaymentsPdfReport;

public record GetOrderInstallmentPaymentsPdfReportQuery : IRequest<Result<GetOrderInstallmentPaymentsPdfReportResponse>>
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public PdfSettings? PdfSettings { get; set; }
}