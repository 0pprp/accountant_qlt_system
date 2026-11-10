using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Queries.Admin.GetAppBlock;

public record GetSalesFollowupAppBlockQuery : IRequest<Result<SalesFollowupAppBlockDto>>
{
    public int EmployeeId { get; init; }
    public int BranchId { get; init; }
}
