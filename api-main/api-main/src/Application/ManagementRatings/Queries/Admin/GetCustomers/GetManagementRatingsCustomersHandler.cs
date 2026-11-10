using Application.Common.Interfaces;
using Application.ManagementRatings.Common;
using Application.ManagementSummary.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementRatings.Queries.Admin.GetCustomers;

public class GetManagementRatingsCustomersHandler
    : IRequestHandler<GetManagementRatingsCustomersQuery, Result<ManagementRatingsCustomersResponse>>
{
    private const int DefaultPageSize = 100;
    private const int MaxPageSize = 500;

    private readonly ManagementRatingsDataService _data;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetManagementRatingsCustomersHandler(
        ManagementRatingsDataService data,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _data = data;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ManagementRatingsCustomersResponse>> Handle(
        GetManagementRatingsCustomersQuery request,
        CancellationToken cancellationToken)
    {
        // DetailsRead alone must not open data — require Read as well.
        if (_currentUser.HasPermission(SeedPermissions.ManagementRatings.Read) == false)
            return ManagementRatingsErrors.AccessDenied;
        if (_currentUser.HasPermission(SeedPermissions.ManagementRatings.DetailsRead) == false)
            return ManagementRatingsErrors.DetailsDenied;

        var err = ManagementRatingsAccess.ValidateBranchRequired(_currentUser, request.BranchId);
        if (err is not null)
            return err.Value;

        if (ManagementRatingsScopeParser.TryParse(request.Scope, out var scope) == false)
            return ManagementRatingsErrors.InvalidScope;

        if (ManagementRatingsLevelParser.TryParse(request.Level, out var level) == false)
            return ManagementRatingsErrors.InvalidLevel;

        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        var today = DateOnly.FromDateTime(local.DateTime);
        var from = request.FromDate ?? new DateOnly(today.Year, today.Month, 1);
        var to = request.ToDate ?? today;
        var page = request.Page is null or < 1 ? 1 : request.Page.Value;
        var pageSize = request.PageSize is null or <= 0
            ? DefaultPageSize
            : Math.Min(request.PageSize.Value, MaxPageSize);

        return await _data.GetCustomersAsync(
            request.BranchId,
            scope,
            request.ResourceId,
            level,
            from,
            to,
            page,
            pageSize,
            request.Q,
            cancellationToken);
    }
}
