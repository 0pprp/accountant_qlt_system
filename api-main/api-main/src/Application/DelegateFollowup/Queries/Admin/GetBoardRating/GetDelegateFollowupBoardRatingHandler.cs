using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetBoardRating;

public class GetDelegateFollowupBoardRatingHandler
    : IRequestHandler<GetDelegateFollowupBoardRatingQuery, Result<DelegateFollowupBoardRatingDto>>
{
    private readonly DelegateFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetDelegateFollowupBoardRatingHandler(
        DelegateFollowupDataService data,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _data = data;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<DelegateFollowupBoardRatingDto>> Handle(
        GetDelegateFollowupBoardRatingQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = DelegateFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        if (await _data.ListExistsInBranchAsync(request.ListId, request.BranchId, cancellationToken) == false)
            return DelegateFollowupErrors.ListNotFound;

        var ratingDate = request.RatingDate ?? _clock.Today;
        return await _data.GetBoardRatingAsync(request.ListId, ratingDate, cancellationToken);
    }
}
