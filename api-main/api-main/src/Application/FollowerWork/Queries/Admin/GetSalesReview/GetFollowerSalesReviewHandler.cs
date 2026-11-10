using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetSalesReview;

public sealed class GetFollowerSalesReviewHandler
    : IRequestHandler<GetFollowerSalesReviewQuery, Result<FollowerSalesReviewWorkspaceDto>>
{
    private readonly FollowerSalesReviewDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerSalesReviewHandler(
        FollowerSalesReviewDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerSalesReviewWorkspaceDto>> Handle(
        GetFollowerSalesReviewQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.SalesReviewRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        var scope = FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId);
        return await _data.GetSalesReviewAsync(
            request.FollowerId,
            scope,
            request.BranchId,
            cancellationToken);
    }
}
