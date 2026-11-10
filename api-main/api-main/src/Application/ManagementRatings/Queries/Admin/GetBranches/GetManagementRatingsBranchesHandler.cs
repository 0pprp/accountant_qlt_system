using Application.Common.Interfaces;
using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetBranches;

public class GetManagementRatingsBranchesHandler
    : IRequestHandler<GetManagementRatingsBranchesQuery, Result<ManagementRatingsBranchesResponse>>
{
    private readonly ManagementRatingsDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetManagementRatingsBranchesHandler(ManagementRatingsDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ManagementRatingsBranchesResponse>> Handle(
        GetManagementRatingsBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var err = ManagementRatingsAccess.ValidateBranchOrAll(_currentUser, null);
        if (err is not null)
            return err.Value;

        var scope = ManagementRatingsAccess.ResolveBranchScope(_currentUser, null);
        return await _data.GetBranchesAsync(scope, cancellationToken);
    }
}
