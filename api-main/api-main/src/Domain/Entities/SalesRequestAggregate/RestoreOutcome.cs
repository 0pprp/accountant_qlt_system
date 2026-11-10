namespace Domain.Entities.SalesRequestAggregate;

public readonly record struct RestoreOutcome(bool AssigneeCleared, bool ApprovedHoldDowngraded);
