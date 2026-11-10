using Application.Common.Interfaces;
using Application.SalesEmployeeTracking.Common;
using Application.SalesEmployees.Common;

namespace Application.SalesEmployeeTracking.Queries.Admin.GetLatest;

public class GetSalesEmployeeLiveLocationsHandler
    : IRequestHandler<GetSalesEmployeeLiveLocationsQuery, Result<SalesEmployeeLiveLocationsResponse>>
{
    private readonly SalesEmployeeTrackingDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetSalesEmployeeLiveLocationsHandler(
        SalesEmployeeTrackingDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesEmployeeLiveLocationsResponse>> Handle(
        GetSalesEmployeeLiveLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var err = SalesEmployeeAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (err is not null)
            return err;

        var scope = SalesEmployeeAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (scope.Count == 0)
            return SalesEmployeeErrors.BranchForbidden;

        return await _data.GetLatestAsync(scope, request.BranchId, request.EmployeeId, cancellationToken);
    }
}
