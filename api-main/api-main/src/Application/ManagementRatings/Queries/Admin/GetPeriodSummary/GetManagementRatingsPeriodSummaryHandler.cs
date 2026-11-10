using Application.Common.Interfaces;
using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetPeriodSummary;

public class GetManagementRatingsPeriodSummaryHandler
    : IRequestHandler<GetManagementRatingsPeriodSummaryQuery, Result<ManagementRatingsPeriodSummaryResponse>>
{
    private readonly ManagementRatingsDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetManagementRatingsPeriodSummaryHandler(ManagementRatingsDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ManagementRatingsPeriodSummaryResponse>> Handle(
        GetManagementRatingsPeriodSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var err = ManagementRatingsAccess.ValidateBranchRequired(_currentUser, request.BranchId);
        if (err is not null)
            return err.Value;

        if (ManagementRatingsScopeParser.TryParse(request.Scope, out var scope) == false)
            return ManagementRatingsErrors.InvalidScope;

        return await _data.GetPeriodSummaryAsync(request.BranchId, scope, request.ResourceId, cancellationToken);
    }
}
