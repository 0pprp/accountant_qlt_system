using Application.Common.Interfaces;
using Application.ManagementRatings.Common;
using Application.ManagementSummary.Common;

namespace Application.ManagementRatings.Queries.Admin.GetListPaymentsSum;

public class GetManagementRatingsListPaymentsSumHandler
    : IRequestHandler<GetManagementRatingsListPaymentsSumQuery, Result<ManagementRatingsListPaymentsSumResponse>>
{
    private readonly ManagementRatingsDataService _data;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetManagementRatingsListPaymentsSumHandler(
        ManagementRatingsDataService data,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _data = data;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ManagementRatingsListPaymentsSumResponse>> Handle(
        GetManagementRatingsListPaymentsSumQuery request,
        CancellationToken cancellationToken)
    {
        var err = ManagementRatingsAccess.ValidateBranchRequired(_currentUser, request.BranchId);
        if (err is not null)
            return err.Value;

        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        var today = DateOnly.FromDateTime(local.DateTime);
        var yesterday = today.AddDays(-1);
        var from = request.FromDate ?? yesterday;
        var to = request.ToDate ?? yesterday;

        return await _data.GetListPaymentsSumAsync(
            request.BranchId,
            request.ListId,
            from,
            to,
            cancellationToken);
    }
}
