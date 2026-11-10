using Application.SalesRequests.Common;

namespace Application.SalesRequests.Commands.App.EnsureDraft;

/// <summary>
/// يضمن LinkedOrderId للطلب المملوك — idempotent.
/// ينشئ مسودة Order فارغة العناصر إن وُجد ExistingCustomerId ولم يُربط بعد.
/// </summary>
public record EnsureSalesRequestDraftCommand(int RequestId)
    : IRequest<Result<SalesRequestAppItemDto>>;
