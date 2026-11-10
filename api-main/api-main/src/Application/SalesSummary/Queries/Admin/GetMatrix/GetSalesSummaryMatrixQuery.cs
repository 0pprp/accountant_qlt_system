using Application.SalesSummary.Common;

namespace Application.SalesSummary.Queries.Admin.GetMatrix;

public sealed class GetSalesSummaryMatrixQuery : IRequest<Result<SalesSummaryMatrixResponse>>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int? BranchId { get; init; }
    /// <summary>يُرفض أي قيمة غير salePrice إن وُجدت — الواجهة لا ترسل مقياسًا.</summary>
    public string? Metric { get; init; }
}
