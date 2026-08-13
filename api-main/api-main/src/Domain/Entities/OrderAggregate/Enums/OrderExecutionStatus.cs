namespace Domain.Entities.OrderAggregate.Enums;

public enum OrderExecutionStatus : byte
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}