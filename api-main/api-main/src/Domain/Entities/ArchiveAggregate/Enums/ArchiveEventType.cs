namespace Domain.Entities.ArchiveAggregate.Enums;

/// <summary>
/// أنواع صفوف السجل الإلحاقي. سرعة التدفّق تُحسب من هذه الصفوف لا من أختام المعاملة،
/// لأن الاسترجاع يُمرّ المعاملة بالمرحلة الواحدة أكثر من مرة فتُكتب الأختام فوق بعضها.
/// </summary>
public enum ArchiveEventType : byte
{
    Created = 0,
    Sent = 1,
    Received = 2,
    SendUndone = 3,
    Returned = 4,
    Voided = 5,
    Revived = 6,
    Rebucketed = 7
}
