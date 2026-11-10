using Application.Common.Interfaces;
using Application.ManagementSummary.Common;

namespace Application.ManagementSummary.Queries.Admin.GetDetails;

public class GetManagementSummaryDetailsHandler
    : IRequestHandler<GetManagementSummaryDetailsQuery, Result<ManagementSummaryDetailsResponse>>
{
    private readonly ManagementSummaryDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetManagementSummaryDetailsHandler(ManagementSummaryDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ManagementSummaryDetailsResponse>> Handle(
        GetManagementSummaryDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (ManagementSummaryRules.IsValidDay(request.Year, request.Month, request.Day) == false)
            return ManagementSummaryErrors.InvalidDay;

        if (ManagementSummaryMetricParser.TryParse(request.Metric, out var metric) == false)
            return ManagementSummaryErrors.InvalidMetric;

        var branchError = ManagementSummaryAccess.ValidateBranchRequired(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var page = ManagementSummaryRules.ClampPage(request.Page);
        var pageSize = ManagementSummaryRules.ClampPageSize(request.PageSize);

        return await _data.GetDetailsAsync(
            request.Year,
            request.Month,
            request.Day,
            metric,
            request.BranchId,
            page,
            pageSize,
            cancellationToken);
    }
}
