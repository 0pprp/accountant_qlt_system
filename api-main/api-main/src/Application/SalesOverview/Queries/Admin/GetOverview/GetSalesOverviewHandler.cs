using Application.Common.Interfaces;
using Application.SalesOverview.Common;

namespace Application.SalesOverview.Queries.Admin.GetOverview;

public class GetSalesOverviewHandler
    : IRequestHandler<GetSalesOverviewQuery, Result<SalesOverviewResponse>>
{
    private readonly SalesOverviewDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetSalesOverviewHandler(SalesOverviewDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesOverviewResponse>> Handle(
        GetSalesOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesOverviewAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var scope = SalesOverviewAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (scope.Count == 0)
            return SalesOverviewErrors.BranchForbidden;

        return await _data.BuildAsync(scope, request.BranchId, cancellationToken);
    }
}
