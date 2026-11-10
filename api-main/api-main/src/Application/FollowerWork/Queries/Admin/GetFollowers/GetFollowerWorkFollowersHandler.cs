using Application.Common.Interfaces;
using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetFollowers;

public sealed class GetFollowerWorkFollowersHandler
    : IRequestHandler<GetFollowerWorkFollowersQuery, Result<FollowerWorkFollowersResponse>>
{
    private readonly FollowerWorkDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetFollowerWorkFollowersHandler(FollowerWorkDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<FollowerWorkFollowersResponse>> Handle(
        GetFollowerWorkFollowersQuery request,
        CancellationToken cancellationToken)
    {
        if (FollowerWorkAccess.CanReadDirectory(_currentUser) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        if (FollowerWorkRules.TryParseSort(request.Sort, out var sort) == false
            && string.IsNullOrWhiteSpace(request.Sort) == false)
            return FollowerWorkErrors.InvalidSort;

        var scope = FollowerWorkAccess.ResolveBranchScope(_currentUser, request.BranchId);
        var page = FollowerWorkRules.ClampPage(request.Page);
        var pageSize = FollowerWorkRules.ClampPageSize(request.PageSize);
        var includeMetrics = FollowerWorkAccess.CanReadCardMetrics(_currentUser);

        return await _data.GetFollowersAsync(
            scope,
            request.Search,
            page,
            pageSize,
            sort,
            includeMetrics,
            cancellationToken);
    }
}
