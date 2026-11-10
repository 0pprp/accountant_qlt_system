using Application.Common.Interfaces;
using Application.Common.Models;
using Application.SalesRequests.Common;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.List;

public class GetAdminSalesRequestsHandler
    : IRequestHandler<GetAdminSalesRequestsQuery, Result<PaginatedList<AdminSalesRequestListItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAdminSalesRequestsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<AdminSalesRequestListItem>>> Handle(
        GetAdminSalesRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        if (branches.Count == 0)
            return Empty(request);

        var tabResult = SalesRequestAdminQuery.ApplyTab(_db, _currentUser, branches, request.Tab);
        if (tabResult.IsFailed || tabResult.Data is null)
            return tabResult.Error!;

        var query = tabResult.Data.Query;

        if (request.BranchId is int branchId && !SalesRequestAccess.CanAccessBranch(_currentUser, branchId))
            return SalesRequestErrors.BranchForbidden;

        var exceptionStatus = tabResult.Data.AppliedTabs.Contains(SalesRequestTabMap.Exceptions)
            ? request.ExceptionStatus
            : null;

        query = SalesRequestAdminQuery.ApplyFilters(
            query,
            request.BranchId,
            request.ProvinceId,
            request.EmployeeId,
            request.SourceType,
            request.FromDate,
            request.ToDate,
            request.Search,
            _currentUser,
            exceptionStatus);

        var deletedView = tabResult.Data.AppliedTabs.Contains(SalesRequestTabMap.Deleted);
        var ordered = deletedView
            ? query.OrderBy(x => (x.DeletedAtUtc ?? x.StatusChangedAtUtc ?? x.CreatedAt) == null).ThenByDescending(x => x.DeletedAtUtc ?? x.StatusChangedAtUtc ?? x.CreatedAt)
            : query.OrderBy(x => (x.StatusChangedAtUtc ?? x.CreatedAt) == null).ThenByDescending(x => x.StatusChangedAtUtc ?? x.CreatedAt);

        var projected = ordered
            .ThenByDescending(x => x.Id)
            .Select(AdminSalesRequestRowMapper.Projection);

        var page = await projected.ToPaginatedListAsync(request.Pagination, cancellationToken);

        var items = page.Items.Select(AdminSalesRequestRowMapper.ToItem).ToList();

        return new PaginatedList<AdminSalesRequestListItem>(
            items, page.TotalCount, page.PageIndex, page.PageSize);
    }

    private static PaginatedList<AdminSalesRequestListItem> Empty(GetAdminSalesRequestsQuery request)
        => new([], 0, request.Pagination.PageIndex, request.Pagination.PageSize);
}
