using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetDelegateNotes;

public sealed class GetFollowerDelegateNotesHandler
    : IRequestHandler<GetFollowerDelegateNotesQuery, Result<FollowerDelegateNotesResponse>>
{
    private readonly FollowerDelegateNotesDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerDelegateNotesHandler(
        FollowerDelegateNotesDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerDelegateNotesResponse>> Handle(
        GetFollowerDelegateNotesQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.DelegateNotesRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        return await _data.GetAsync(
            request.FollowerId,
            FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId),
            request.BranchId,
            request.Page ?? 1,
            request.PageSize ?? FollowerDelegateNotesDataService.DefaultPageSize,
            cancellationToken);
    }
}
