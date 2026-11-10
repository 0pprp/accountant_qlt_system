using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetLists;

public class GetDelegateFollowupListsHandler
    : IRequestHandler<GetDelegateFollowupListsQuery, Result<GetDelegateFollowupListsResponse>>
{
    private readonly DelegateFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetDelegateFollowupListsHandler(DelegateFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<GetDelegateFollowupListsResponse>> Handle(
        GetDelegateFollowupListsQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = DelegateFollowupAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var scope = DelegateFollowupAccess.ResolveBranchScope(_currentUser, request.BranchId);
        var lists = await _data.ListActiveListsAsync(scope, cancellationToken);

        return new GetDelegateFollowupListsResponse { Lists = lists };
    }
}
