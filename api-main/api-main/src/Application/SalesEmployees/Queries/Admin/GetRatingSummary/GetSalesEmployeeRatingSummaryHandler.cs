using Application.Common.Interfaces;
using Application.SalesEmployees.Common;

namespace Application.SalesEmployees.Queries.Admin.GetRatingSummary;

public class GetSalesEmployeeRatingSummaryHandler
    : IRequestHandler<GetSalesEmployeeRatingSummaryQuery, Result<SalesEmployeeRatingSummaryDto>>
{
    private readonly SalesEmployeesDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetSalesEmployeeRatingSummaryHandler(
        SalesEmployeesDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesEmployeeRatingSummaryDto>> Handle(
        GetSalesEmployeeRatingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesEmployeeAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var scope = SalesEmployeeAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (scope.Count == 0)
            return SalesEmployeeErrors.BranchForbidden;

        return await _data.GetRatingSummaryAsync(
            request.EmployeeId,
            scope,
            request.Bucket,
            cancellationToken);
    }
}
