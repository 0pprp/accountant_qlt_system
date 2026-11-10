using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Queries.Admin.GetEmployeeDetail;

public record GetSalesFollowupEmployeeDetailQuery : IRequest<Result<SalesFollowupDetailDto>>
{
    public int EmployeeId { get; init; }
    public int BranchId { get; init; }
}
