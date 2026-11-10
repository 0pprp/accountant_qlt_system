using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesEmployeeTracking.Common;
using Application.SalesEmployees.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesEmployeeTracking.Queries.Admin.GetRoute;

public class GetSalesEmployeeRouteHandler
    : IRequestHandler<GetSalesEmployeeRouteQuery, Result<SalesEmployeeRouteResponse>>
{
    private readonly SalesEmployeeTrackingDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetSalesEmployeeRouteHandler(
        SalesEmployeeTrackingDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesEmployeeRouteResponse>> Handle(
        GetSalesEmployeeRouteQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.SalesRoute.Read) == false)
            return new Error("غير مخول بقراءة مسارات موظفي المبيعات.", "SalesRoute.Forbidden");

        var err = SalesEmployeeAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (err is not null)
            return err;

        var scope = SalesEmployeeAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (scope.Count == 0)
            return SalesEmployeeErrors.BranchForbidden;

        return await _data.GetRouteAsync(
            request.EmployeeId,
            scope,
            request.BranchId,
            request.Date,
            request.Page ?? 1,
            request.PageSize ?? FollowerLocationRules.DefaultRoutePageSize,
            cancellationToken);
    }
}
