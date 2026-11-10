namespace Application.SalesEmployees.Commands.Admin.EndAssignment;

public sealed class EndSalesEmployeeAssignmentCommand : IRequest<Result>
{
    public int AssignmentId { get; init; }
}
