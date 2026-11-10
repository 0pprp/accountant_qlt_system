using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.Counts;

public class GetAdminSalesRequestCountsHandler
    : IRequestHandler<GetAdminSalesRequestCountsQuery, Result<AdminSalesRequestCountsResponse>>
{
    public const string SentCaveat = "no_ingestion_path_wired";

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAdminSalesRequestCountsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<AdminSalesRequestCountsResponse>> Handle(
        GetAdminSalesRequestCountsQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var allowed = SalesRequestTabPermissions.AllowedWorkflowTabs(_currentUser);
        var allowedExtras = SalesRequestTabPermissions.AllowedExtraViews(_currentUser);

        if (branches.Count == 0 || (allowed.Count == 0 && allowedExtras.Count == 0))
        {
            return new AdminSalesRequestCountsResponse
            {
                Tabs = [],
                AllowedTabs = allowed,
                AllowedExtraViews = allowedExtras,
                ExtraViewsNotImplemented = SalesRequestTabMap.ExtraViewsNotImplemented
            };
        }

        if (request.BranchId is int branchId && !SalesRequestAccess.CanAccessBranch(_currentUser, branchId))
            return SalesRequestErrors.BranchForbidden;

        var tabs = new List<AdminSalesRequestTabCount>();
        foreach (var tab in allowed)
        {
            var count = await CountAsync(tab, branches, request, cancellationToken);
            if (count is null) continue;

            tabs.Add(new AdminSalesRequestTabCount
            {
                Tab = tab,
                Label = SalesRequestTabMap.ArabicLabel(tab),
                Count = count.Value
            });
        }

        if (allowed.Count > 0)
        {
            tabs.Add(new AdminSalesRequestTabCount
            {
                Tab = SalesRequestTabMap.All,
                Label = SalesRequestTabMap.ArabicLabel(SalesRequestTabMap.All),
                Count = tabs.Sum(t => t.Count)
            });
        }

        var extraViews = new List<AdminSalesRequestExtraViewCount>();
        foreach (var tab in allowedExtras)
        {
            var count = await CountAsync(tab, branches, request, cancellationToken);
            if (count is null) continue;

            extraViews.Add(new AdminSalesRequestExtraViewCount
            {
                Tab = tab,
                Label = SalesRequestTabMap.ArabicLabel(tab),
                Count = count.Value,
                Overlapping = true,
                IncludedInAll = false,
                Caveat = tab == SalesRequestTabMap.Sent ? SentCaveat : null
            });
        }

        return new AdminSalesRequestCountsResponse
        {
            Tabs = tabs,
            AllowedTabs = allowed,
            ExtraViews = extraViews,
            AllowedExtraViews = allowedExtras,
            ExtraViewsNotImplemented = SalesRequestTabMap.ExtraViewsNotImplemented
        };
    }

    private async Task<int?> CountAsync(
        string tab,
        IReadOnlyList<int> branches,
        GetAdminSalesRequestCountsQuery request,
        CancellationToken cancellationToken)
    {
        var tabResult = SalesRequestAdminQuery.ApplyTab(_db, _currentUser, branches, tab);
        if (tabResult.IsFailed || tabResult.Data is null) return null;

        var query = SalesRequestAdminQuery.ApplyFilters(
            tabResult.Data.Query,
            request.BranchId,
            request.ProvinceId,
            request.EmployeeId,
            request.SourceType,
            request.FromDate,
            request.ToDate,
            request.Search,
            _currentUser);

        return await query.CountAsync(cancellationToken);
    }
}
