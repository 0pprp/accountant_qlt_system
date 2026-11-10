namespace Application.SalesEmployees.Commands.Admin.GrantAssignment;

public sealed class GrantSalesEmployeeAssignmentCommand : IRequest<Result<GrantSalesEmployeeAssignmentResponse>>
{
    public int UserId { get; init; }
    public int BranchId { get; init; }
    public string? Note { get; init; }
}

public sealed class GrantSalesEmployeeAssignmentResponse
{
    public required int AssignmentId { get; init; }
    public required int UserId { get; init; }
    public required int BranchId { get; init; }
    public required DateTimeOffset StartsAtUtc { get; init; }
}
