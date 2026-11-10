namespace Domain.Entities.ManagementTaskAggregate.Enums;

public enum ManagementTaskEventType : byte
{
    Created = 1,
    Updated = 2,
    Accepted = 3,
    Rejected = 4,
    Transferred = 5,
    Completed = 6,
    Confirmed = 7
}
