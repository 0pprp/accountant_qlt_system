using Domain.Common;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Domain.Entities.ArchiveAggregate;

/// <summary>
/// سجل إلحاقي لا يُعدَّل. هو المصدر الوحيد لسرعة التدفّق ولتاريخ العهدة،
/// لأن أختام <see cref="ArchiveTransaction"/> تُكتب فوق بعضها عند الاسترجاع.
/// <c>OperationId</c> يجمع صفوف العملية الواحدة (استلام دفعة مثلاً) في مجموعة واحدة.
/// </summary>
public class ArchiveTransactionEvent : Entity
{
    public const int NoteMaxLength = 500;

    private ArchiveTransactionEvent()
    {
    }

    public ArchiveTransactionEvent(
        int transactionId,
        ArchiveEventType eventType,
        ArchiveTransactionStatus previousStatus,
        ArchiveTransactionStatus newStatus,
        ArchiveStage? stage,
        int actorUserId,
        string actorName,
        DateTimeOffset atUtc,
        Guid operationId,
        string? note = null)
    {
        TransactionId = transactionId;
        EventType = eventType;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Stage = stage;
        ActorUserId = actorUserId;
        ActorName = actorName;
        AtUtc = atUtc;
        OperationId = operationId;
        Note = note;
    }

    public int TransactionId { get; private set; }
    public ArchiveEventType EventType { get; private set; }
    public ArchiveTransactionStatus PreviousStatus { get; private set; }
    public ArchiveTransactionStatus NewStatus { get; private set; }
    public ArchiveStage? Stage { get; private set; }
    public int ActorUserId { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public DateTimeOffset AtUtc { get; private set; }
    public Guid OperationId { get; private set; }
    public string? Note { get; private set; }

    public ArchiveTransaction? Transaction { get; private set; }
}
