using Application.Common.Interfaces;
using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Queries.Admin.ListEmployees;

public class ListSalesFollowupEmployeesHandler
    : IRequestHandler<ListSalesFollowupEmployeesQuery, Result<ListSalesFollowupEmployeesResponse>>
{
    private readonly SalesFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public ListSalesFollowupEmployeesHandler(SalesFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ListSalesFollowupEmployeesResponse>> Handle(
        ListSalesFollowupEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesFollowupAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var scope = SalesFollowupAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (scope.Count == 0)
            return new ListSalesFollowupEmployeesResponse { Items = [], Marker = SalesFollowupRules.DeployMarker };

        var items = await _data.ListEmployeesAsync(scope, cancellationToken);
        return new ListSalesFollowupEmployeesResponse
        {
            Items = items.ToList(),
            Marker = SalesFollowupRules.DeployMarker,
        };
    }
}
