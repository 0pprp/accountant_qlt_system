using Application.Common.Interfaces;
using Application.ManagementSummary.Common;

namespace Application.ManagementSummary.Queries.Admin.GetMatrix;

public class GetManagementSummaryMatrixHandler
    : IRequestHandler<GetManagementSummaryMatrixQuery, Result<ManagementSummaryMatrixResponse>>
{
    private readonly ManagementSummaryDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetManagementSummaryMatrixHandler(ManagementSummaryDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ManagementSummaryMatrixResponse>> Handle(
        GetManagementSummaryMatrixQuery request,
        CancellationToken cancellationToken)
    {
        if (ManagementSummaryRules.IsValidPeriod(request.Year, request.Month) == false)
            return ManagementSummaryErrors.InvalidPeriod;

        if (ManagementSummaryMetricParser.TryParse(request.Metric, out var metric) == false)
            return ManagementSummaryErrors.InvalidMetric;

        var branchError = ManagementSummaryAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var scope = ManagementSummaryAccess.ResolveBranchScope(_currentUser, request.BranchId);
        var matrix = await _data.GetMatrixAsync(
            request.Year,
            request.Month,
            metric,
            scope,
            _currentUser,
            cancellationToken);

        return matrix;
    }
}
