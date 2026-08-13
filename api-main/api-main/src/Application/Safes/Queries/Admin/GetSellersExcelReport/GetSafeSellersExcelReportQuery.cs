using Application.Safes.Queries.Admin.GetSellers;

namespace Application.Safes.Queries.Admin.GetSellersExcelReport;

public record GetSafeSellersExcelReportQuery : IRequest<Result<GetSafeSellersExcelReportResponse>>
{
    public int SafeId { get; set; }
    public required GetSafeSellersFilter Filter { get; set; }
}