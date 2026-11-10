using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetDeclining;

public sealed class GetFollowerDecliningHandler
    : IRequestHandler<GetFollowerDecliningQuery, Result<FollowerDecliningResponse>>
{
    private readonly FollowerDecliningDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerDecliningHandler(
        FollowerDecliningDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerDecliningResponse>> Handle(
        GetFollowerDecliningQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.DecliningRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        var scope = FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId);
        return await _data.GetDecliningForAdminAsync(
            request.FollowerId,
            scope,
            request.BranchId,
            request.Page ?? 1,
            request.PageSize ?? FollowerDecliningDataService.DefaultPageSize,
            cancellationToken);
    }
}
