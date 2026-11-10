namespace Domain.Entities.ManagementTaskAggregate.Enums;

public enum ManagementTaskStatus : byte
{
    Sent = 0,
    Transferred = 1,
    Accepted = 2,
    Returned = 3,
    AwaitingDelegatedConfirm = 4,
    Completed = 5
}
