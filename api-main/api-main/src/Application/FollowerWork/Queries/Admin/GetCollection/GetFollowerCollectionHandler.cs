using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetCollection;

public sealed class GetFollowerCollectionHandler
    : IRequestHandler<GetFollowerCollectionQuery, Result<FollowerCollectionResponse>>
{
    private readonly FollowerCollectionDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerCollectionHandler(
        FollowerCollectionDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerCollectionResponse>> Handle(
        GetFollowerCollectionQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.CollectionRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        return await _data.GetAsync(
            request.FollowerId,
            FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId),
            request.BranchId,
            request.FromDate,
            request.ToDate,
            request.PaymentsPage ?? 1,
            request.PaymentsPageSize ?? FollowerCollectionDataService.DefaultPaymentsPageSize,
            cancellationToken);
    }
}
