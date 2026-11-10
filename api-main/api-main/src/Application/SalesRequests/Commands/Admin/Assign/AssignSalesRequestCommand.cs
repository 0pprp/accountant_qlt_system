namespace Application.SalesRequests.Commands.Admin.Assign;

public record AssignSalesRequestCommand : IRequest<Result>
{
    public int RequestId { get; init; }
    public int EmployeeId { get; init; }
    public string? Reason { get; init; }
}
