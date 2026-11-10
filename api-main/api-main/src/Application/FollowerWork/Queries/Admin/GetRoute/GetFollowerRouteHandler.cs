using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetRoute;

public sealed class GetFollowerRouteHandler
    : IRequestHandler<GetFollowerRouteQuery, Result<FollowerRouteResponse>>
{
    private readonly FollowerTrackingDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerRouteHandler(
        FollowerTrackingDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerRouteResponse>> Handle(
        GetFollowerRouteQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.RouteRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        return await _data.GetRouteAsync(
            request.FollowerId,
            FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId),
            request.BranchId,
            request.Date,
            request.Page ?? 1,
            request.PageSize ?? FollowerLocationRules.DefaultRoutePageSize,
            cancellationToken);
    }
}
