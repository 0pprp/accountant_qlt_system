namespace Domain.Entities.SalesRequestAggregate.Enums;

public enum SalesRequestTimelineEventType : byte
{
    LegalFollowupSent = 40,
    LegalFollowupCancelled = 41,
    Created = 18,
    Assigned = 1,
    Reassigned = 2,
    Viewed = 3,
    ViewSeen = 4,
    Prepared = 5,
    Pend = 6,
    Rejected = 7,
    Inspected = 8,
    DraftLinked = 9,
    Completed = 10,
    Returned = 11,
    ExceptionRequested = 12,
    ExceptionApproved = 13,
    ExceptionRejected = 14,
    ExceptionConsumed = 15,
    SoftDeleted = 16,
    Restored = 17,
}
