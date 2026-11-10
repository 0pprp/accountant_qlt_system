using Application.Common.Interfaces;
using Application.Common.Models;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.GetUnassigned;

public class GetUnassignedSalesRequestsHandler
    : IRequestHandler<GetUnassignedSalesRequestsQuery, Result<PaginatedList<GetUnassignedSalesRequestItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetUnassignedSalesRequestsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<GetUnassignedSalesRequestItem>>> Handle(
        GetUnassignedSalesRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        if (branches.Count == 0)
            return new PaginatedList<GetUnassignedSalesRequestItem>(
                [], 0, request.Pagination.PageIndex, request.Pagination.PageSize);

        var filter = request.Filter;
        var query = SalesRequestAccess.UnassignedInScope(_db, branches);

        if (filter.BranchId is int branchId)
        {
            if (!SalesRequestAccess.CanAccessBranch(_currentUser, branchId))
                return SalesRequestErrors.BranchForbidden;
            query = query.Where(x => x.BranchId == branchId);
        }

        if (filter.ProvinceId is int provinceId)
            query = query.Where(x => x.ProvinceId == provinceId);

        if (filter.SourceType is { } source)
            query = query.Where(x => x.SourceType == source);

        if (filter.FromDate is DateOnly from)
        {
            var fromDto = from.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.CreatedAt != null && x.CreatedAt.Value.DateTime >= fromDto);
        }

        if (filter.ToDate is DateOnly to)
        {
            var toDto = to.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(x => x.CreatedAt != null && x.CreatedAt.Value.DateTime <= toDto);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            if (int.TryParse(term, out var idTerm))
            {
                query = query.Where(x =>
                    x.Id == idTerm
                    || (x.CustomerName != null && x.CustomerName.Contains(term))
                    || (x.CustomerPhone != null && x.CustomerPhone.Contains(term))
                    || (x.OriginalExternalId != null && x.OriginalExternalId.Contains(term)));
            }
            else
            {
                query = query.Where(x =>
                    (x.CustomerName != null && x.CustomerName.Contains(term))
                    || (x.CustomerPhone != null && x.CustomerPhone.Contains(term))
                    || (x.OriginalExternalId != null && x.OriginalExternalId.Contains(term)));
            }
        }

        var projected = query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new GetUnassignedSalesRequestItem
            {
                Id = x.Id,
                CustomerName = x.CustomerName,
                CustomerPhone = x.CustomerPhone,
                SourceType = x.SourceType,
                SourceTypeLabel = "",
                OriginalExternalId = x.OriginalExternalId,
                BranchId = x.BranchId,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province != null ? x.Province.Name : null,
                CreatedAt = x.CreatedAt,
                CreatedByName = x.CreatedByUser != null ? x.CreatedByUser.FullName : null,
                StatusLabel = "غير مسند"
            });

        var page = await projected.ToPaginatedListAsync(request.Pagination, cancellationToken);
        var withLabels = page.Items.Select(x => x with
        {
            SourceTypeLabel = SalesRequestSourceLabels.For(x.SourceType)
        }).ToList();

        return new PaginatedList<GetUnassignedSalesRequestItem>(
            withLabels,
            page.TotalCount,
            page.PageIndex,
            page.PageSize);
    }
}
