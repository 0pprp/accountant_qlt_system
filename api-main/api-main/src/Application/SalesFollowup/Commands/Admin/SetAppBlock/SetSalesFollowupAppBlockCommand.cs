using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Commands.Admin.SetAppBlock;

public record SetSalesFollowupAppBlockCommand : IRequest<Result<SalesFollowupAppBlockDto>>
{
    public int EmployeeId { get; init; }
    public int BranchId { get; init; }
    public bool IsBlocked { get; init; }
}
