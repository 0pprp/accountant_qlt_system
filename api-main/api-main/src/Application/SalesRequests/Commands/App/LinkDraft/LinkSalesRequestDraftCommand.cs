using Application.SalesRequests.Common;

namespace Application.SalesRequests.Commands.App.LinkDraft;

/// <summary>
/// ربط idempotent لـ Order مملوك بالموظف مع الطلب — لا إنشاء Order هنا إن لم يُمرَّر orderId.
/// يمنع تكرار الربط عبر فهرس LinkedOrderId الفريد.
/// </summary>
public record LinkSalesRequestDraftCommand : IRequest<Result<SalesRequestAppItemDto>>
{
    public int RequestId { get; init; }
    public int OrderId { get; init; }
}
