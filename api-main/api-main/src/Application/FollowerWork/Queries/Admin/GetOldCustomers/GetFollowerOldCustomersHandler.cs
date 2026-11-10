using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetOldCustomers;

public sealed class GetFollowerOldCustomersHandler
    : IRequestHandler<GetFollowerOldCustomersQuery, Result<FollowerOldCustomersResponse>>
{
    private readonly FollowerOldCustomersDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerOldCustomersHandler(
        FollowerOldCustomersDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerOldCustomersResponse>> Handle(
        GetFollowerOldCustomersQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.OldCustomersRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        return await _data.GetForAdminAsync(
            request.FollowerId,
            FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId),
            request.BranchId,
            request.Page ?? 1,
            request.PageSize ?? FollowerOldCustomersDataService.DefaultPageSize,
            cancellationToken);
    }
}
