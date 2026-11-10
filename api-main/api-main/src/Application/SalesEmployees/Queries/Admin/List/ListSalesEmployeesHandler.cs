using Application.Common.Interfaces;
using Application.SalesEmployees.Common;

namespace Application.SalesEmployees.Queries.Admin.List;

public class ListSalesEmployeesHandler
    : IRequestHandler<ListSalesEmployeesQuery, Result<ListSalesEmployeesResponse>>
{
    private readonly SalesEmployeesDataService _data;
    private readonly ICurrentUserService _currentUser;

    public ListSalesEmployeesHandler(SalesEmployeesDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ListSalesEmployeesResponse>> Handle(
        ListSalesEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesEmployeeAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        if (!string.IsNullOrWhiteSpace(request.ShiftState)
            && !SalesEmployeeIdentity.AllowedShiftStates.Contains(request.ShiftState.Trim()))
            return SalesEmployeeErrors.InvalidShiftState;

        var sort = string.IsNullOrWhiteSpace(request.Sort) ? "name" : request.Sort.Trim();
        if (!SalesEmployeeIdentity.AllowedSorts.Contains(sort))
            return SalesEmployeeErrors.InvalidSort;

        var scope = SalesEmployeeAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (scope.Count == 0)
            return SalesEmployeeErrors.BranchForbidden;

        return await _data.ListAsync(
            scope,
            request.Search,
            string.IsNullOrWhiteSpace(request.ShiftState) ? null : request.ShiftState.Trim(),
            request.Page,
            request.PageSize,
            sort,
            cancellationToken);
    }
}
