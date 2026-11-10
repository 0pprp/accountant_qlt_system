using Application.Common.Interfaces;
using Application.ManagementRatings.Common;
using Application.ManagementSummary.Common;

namespace Application.ManagementRatings.Queries.Admin.GetTrend;

public class GetManagementRatingsTrendHandler
    : IRequestHandler<GetManagementRatingsTrendQuery, Result<ManagementRatingsTrendResponse>>
{
    private readonly ManagementRatingsDataService _data;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetManagementRatingsTrendHandler(
        ManagementRatingsDataService data,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _data = data;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ManagementRatingsTrendResponse>> Handle(
        GetManagementRatingsTrendQuery request,
        CancellationToken cancellationToken)
    {
        var err = ManagementRatingsAccess.ValidateBranchRequired(_currentUser, request.BranchId);
        if (err is not null)
            return err.Value;

        if (ManagementRatingsScopeParser.TryParse(request.Scope, out var scope) == false)
            return ManagementRatingsErrors.InvalidScope;

        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        var today = DateOnly.FromDateTime(local.DateTime);
        var from = request.FromDate ?? new DateOnly(today.Year, today.Month, 1);
        var to = request.ToDate ?? today;
        if (to < from)
            return ManagementRatingsErrors.InvalidPeriod;

        return await _data.GetTrendAsync(request.BranchId, scope, request.ResourceId, from, to, cancellationToken);
    }
}
