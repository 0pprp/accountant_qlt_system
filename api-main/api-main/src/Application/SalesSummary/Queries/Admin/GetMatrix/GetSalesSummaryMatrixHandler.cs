using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRequests.Common;
using Application.SalesSummary.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesSummary.Queries.Admin.GetMatrix;

public class GetSalesSummaryMatrixHandler
    : IRequestHandler<GetSalesSummaryMatrixQuery, Result<SalesSummaryMatrixResponse>>
{
    private readonly ManagementSummaryDataService _managementData;
    private readonly ICurrentUserService _currentUser;

    public GetSalesSummaryMatrixHandler(
        ManagementSummaryDataService managementData,
        ICurrentUserService currentUser)
    {
        _managementData = managementData;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesSummaryMatrixResponse>> Handle(
        GetSalesSummaryMatrixQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.SalesSummary.Read) == false)
            return SalesSummaryErrors.ReadDenied;

        if (ManagementSummaryRules.IsValidPeriod(request.Year, request.Month) == false)
            return SalesSummaryErrors.InvalidPeriod;

        if (!string.IsNullOrWhiteSpace(request.Metric)
            && !string.Equals(request.Metric.Trim(), "salePrice", StringComparison.OrdinalIgnoreCase))
            return SalesSummaryErrors.MetricRejected;

        var branchError = ManagementSummaryAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var scope = ManagementSummaryAccess.ResolveBranchScope(_currentUser, request.BranchId);
        // إعادة استخدام قاعدة SalePrice من ManagementSummary — بدون صناديق/مقاييس أخرى
        var matrix = await _managementData.GetMatrixAsync(
            request.Year,
            request.Month,
            ManagementSummaryMetric.SalePrice,
            scope,
            _currentUser,
            cancellationToken);

        return new SalesSummaryMatrixResponse(
            matrix.Year,
            matrix.Month,
            matrix.DaysInMonth,
            matrix.TimeZone,
            matrix.Currency,
            "salePrice",
            "سعر البيع",
            "SaleDate (تقويم) مع fallback CreatedAt بحدود يوم بغداد 00:00 — ليس قطع بيع 03:00",
            matrix.Branches.Select(b => new SalesSummaryBranchDto(b.Id, b.Name)).ToList(),
            matrix.Cells.Select(c => new SalesSummaryCellDto(c.Day, c.BranchId, c.Amount)).ToList(),
            matrix.DayTotals.Select(d => new SalesSummaryDayTotalDto(d.Day, d.Amount)).ToList(),
            matrix.BranchTotals.Select(b => new SalesSummaryBranchTotalDto(b.BranchId, b.Amount)).ToList(),
            matrix.MonthTotal,
            matrix.GeneratedAt);
    }
}
