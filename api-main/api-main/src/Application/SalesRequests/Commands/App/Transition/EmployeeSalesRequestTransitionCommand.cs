namespace Application.SalesRequests.Commands.App.Transition;

public enum EmployeeSalesRequestAction : byte
{
    Prepare = 1,
    Pend = 2,
    Reject = 3,
    Inspect = 4,
}

public record EmployeeSalesRequestTransitionCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public EmployeeSalesRequestAction Action { get; init; }
    public string? Note { get; init; }
}
