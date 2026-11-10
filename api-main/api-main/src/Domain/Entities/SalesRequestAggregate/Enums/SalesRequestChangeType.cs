namespace Domain.Entities.SalesRequestAggregate.Enums;

public enum SalesRequestChangeType : byte
{
    Assigned = 1,
    Reassigned = 2,
    StatusChanged = 3,
    NoteChanged = 4,
    DraftLinked = 5,
    Completed = 6,
    Invalidation = 7,
}
