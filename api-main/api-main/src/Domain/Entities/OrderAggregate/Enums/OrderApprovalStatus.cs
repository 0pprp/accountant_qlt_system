namespace Domain.Entities.OrderAggregate.Enums;

public enum OrderApprovalStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}