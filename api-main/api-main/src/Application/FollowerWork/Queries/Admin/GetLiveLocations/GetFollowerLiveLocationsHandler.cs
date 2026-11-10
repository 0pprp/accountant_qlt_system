using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetLiveLocations;

public sealed class GetFollowerLiveLocationsHandler
    : IRequestHandler<GetFollowerLiveLocationsQuery, Result<FollowerLiveLocationsResponse>>
{
    private readonly FollowerTrackingDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerLiveLocationsHandler(
        FollowerTrackingDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerLiveLocationsResponse>> Handle(
        GetFollowerLiveLocationsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.LocationRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        return await _data.GetLatestAsync(
            FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId),
            request.BranchId,
            request.FollowerId,
            cancellationToken);
    }
}
