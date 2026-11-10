using Application.SalesRequests.Common;

namespace Application.SalesRequests.Queries.App.GetByTab;

public record GetAppSalesRequestsByTabQuery : IRequest<Result<List<SalesRequestAppItemDto>>>
{
    public required string Tab { get; init; }
}

public static class SalesRequestStatusLabels
{
    public static string For(Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus s) => s switch
    {
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.New => "جديد",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Assigned => "مسند",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Viewed => "تمت المشاهدة",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Returned => "مُعاد",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.PreparedForSale => "مُجهّز للبيع",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.InProgress => "قيد التنفيذ",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Inspected => "تم الكشف",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Pending => "معلّق",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Rejected => "مرفوض",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.Completed => "تم البيع",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.LegalFollowup => "متابعة قانونية",
        Domain.Entities.SalesRequestAggregate.Enums.SalesRequestStatus.ConvertedToSale => "محوّل لمسودة بيع",
        _ => s.ToString()
    };
}
