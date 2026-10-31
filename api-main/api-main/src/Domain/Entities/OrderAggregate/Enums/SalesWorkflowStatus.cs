namespace Domain.Entities.OrderAggregate.Enums;

public enum SalesWorkflowStatus : byte
{
    Pending = 0,
    Ready = 1,
    Disclosure = 2,
    OnHold = 3,
    Rejected = 4,
    Sold = 5
}
