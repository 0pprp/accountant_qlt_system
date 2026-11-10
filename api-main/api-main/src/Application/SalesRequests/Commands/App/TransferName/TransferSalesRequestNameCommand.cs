using Application.SalesRequests.Common;

namespace Application.SalesRequests.Commands.App.TransferName;

/// <summary>
/// عقد جديد صريح: نقل ملكية طلب البيع من الموظف الحالي إلى زميل مؤهل في نفس الفرع.
/// ليس SalesWorkflow/transfer (حالة Order) وليس إسنادًا إداريًا.
/// </summary>
public sealed class TransferSalesRequestNameCommand : IRequest<Result<SalesRequestAppItemDto>>
{
    public required int RequestId { get; init; }
    public required int ToEmployeeId { get; init; }
    public required string Reason { get; init; }
}
