using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Queries.Admin.GetNotes;

public sealed class GetFollowerNotesHandler
    : IRequestHandler<GetFollowerNotesQuery, Result<FollowerNotesResponse>>
{
    private readonly FollowerNotesDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerNotesHandler(FollowerNotesDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerNotesResponse>> Handle(
        GetFollowerNotesQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.NotesRead) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        return await _data.GetNotesAsync(
            request.FollowerId,
            FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId),
            request.BranchId,
            request.Page ?? 1,
            request.PageSize ?? FollowerNotesDataService.DefaultPageSize,
            cancellationToken);
    }
}
