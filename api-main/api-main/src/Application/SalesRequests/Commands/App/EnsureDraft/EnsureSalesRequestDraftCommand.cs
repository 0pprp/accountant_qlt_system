using Application.SalesRequests.Common;

namespace Application.SalesRequests.Commands.App.EnsureDraft;

/// <summary>
/// يضمن LinkedOrderId للطلب المملوك — idempotent.
/// ينشئ مسودة Order فارغة العناصر؛ يربط ExistingCustomerId إن وُجد أو يُنشئ زبوناً مؤقتاً من لقطة الطلب.
/// </summary>
public record EnsureSalesRequestDraftCommand(int RequestId)
    : IRequest<Result<SalesRequestAppItemDto>>;
