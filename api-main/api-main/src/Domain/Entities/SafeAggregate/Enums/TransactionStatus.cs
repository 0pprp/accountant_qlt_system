namespace Domain.Entities.SafeAggregate.Enums;

public enum TransactionStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}