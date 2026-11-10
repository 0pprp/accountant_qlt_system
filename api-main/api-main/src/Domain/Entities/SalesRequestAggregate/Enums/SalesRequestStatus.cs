namespace Domain.Entities.SalesRequestAggregate.Enums;

/// <summary>
/// حالات طلب البيع — ليست مرادفة لـ SalesWorkflowStatus على Order.
/// «غير المسند» = New أو بلا مستهدف، وليست Pending على Order.
/// «طلبات البيع» في تطبيق الموظف = Assigned/Viewed/Returned — وليست tab=pending على Order.
/// ConvertedToSale = مسودة Order مربوطة (LinkedOrderId) — ليست «تم البيع».
/// Completed = إكمال بيع ناجح فقط.
/// </summary>
public enum SalesRequestStatus : byte
{
    New = 0,
    Assigned = 1,
    Viewed = 2,
    Returned = 3,
    PreparedForSale = 4,
    InProgress = 5,
    Inspected = 6,
    Pending = 7,
    Rejected = 8,
    Completed = 9,
    LegalFollowup = 10,
    /// <summary>مسودة بيع مربوطة — مرادف معنى ConvertedToSale في النظام القديم.</summary>
    ConvertedToSale = 11,
}
