using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetSummary;

public class GetDelegateFollowupSummaryHandler
    : IRequestHandler<GetDelegateFollowupSummaryQuery, Result<DelegateFollowupSummaryDto>>
{
    private readonly DelegateFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetDelegateFollowupSummaryHandler(DelegateFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<DelegateFollowupSummaryDto>> Handle(
        GetDelegateFollowupSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = DelegateFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var summary = await _data.GetSummaryAsync(request.ListId, request.BranchId, cancellationToken);
        if (summary is null)
            return DelegateFollowupErrors.ListNotFound;

        return summary;
    }
}
