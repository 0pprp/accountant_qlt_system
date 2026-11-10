using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Application.SalesRequests.Common;
using Application.SalesSummary.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesSummary.Queries.Admin.GetDetails;

public class GetSalesSummaryDetailsHandler
    : IRequestHandler<GetSalesSummaryDetailsQuery, Result<SalesSummaryDetailsResponse>>
{
    private readonly ManagementSummaryDataService _managementData;
    private readonly ICurrentUserService _currentUser;

    public GetSalesSummaryDetailsHandler(
        ManagementSummaryDataService managementData,
        ICurrentUserService currentUser)
    {
        _managementData = managementData;
        _currentUser = currentUser;
    }

    public async Task<Result<SalesSummaryDetailsResponse>> Handle(
        GetSalesSummaryDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.SalesSummary.Drilldown) == false)
            return SalesSummaryErrors.DrilldownDenied;

        // Drilldown alone must not imply matrix read — caller needs Drilldown explicitly.
        if (ManagementSummaryRules.IsValidDay(request.Year, request.Month, request.Day) == false)
            return SalesSummaryErrors.InvalidDay;

        if (!string.IsNullOrWhiteSpace(request.Metric)
            && !string.Equals(request.Metric.Trim(), "salePrice", StringComparison.OrdinalIgnoreCase))
            return SalesSummaryErrors.MetricRejected;

        if (request.BranchId <= 0 || ManagementSummaryAccess.CanAccessBranch(_currentUser, request.BranchId) == false)
            return SalesSummaryErrors.BranchForbidden;

        var page = ManagementSummaryRules.ClampPage(request.Page);
        var pageSize = ManagementSummaryRules.ClampPageSize(request.PageSize);

        var details = await _managementData.GetDetailsAsync(
            request.Year,
            request.Month,
            request.Day,
            ManagementSummaryMetric.SalePrice,
            request.BranchId,
            page,
            pageSize,
            cancellationToken);

        if (details.IsFailed || details.Data is null)
            return details.Error ?? SalesSummaryErrors.BranchForbidden;

        var d = details.Data;
        return new SalesSummaryDetailsResponse(
            d.Year,
            d.Month,
            d.Day,
            d.BranchId,
            d.BranchName,
            "salePrice",
            "سعر البيع",
            d.CellTotal,
            d.Currency,
            "SaleDate يوم الخلية أو CreatedAt ضمن حدود بغداد 00:00",
            d.Items.Select(r =>
            {
                var orderId = 0;
                if (r.Reference is { } pref && pref.StartsWith("order:", StringComparison.OrdinalIgnoreCase))
                    _ = int.TryParse(pref["order:".Length..], out orderId);
                return new SalesSummaryDetailRowDto(orderId, r.Title, r.Subtitle, r.Amount, r.OccurredAt);
            }).ToList(),
            d.Page,
            d.PageSize,
            d.TotalCount,
            d.GeneratedAt);
    }
}
