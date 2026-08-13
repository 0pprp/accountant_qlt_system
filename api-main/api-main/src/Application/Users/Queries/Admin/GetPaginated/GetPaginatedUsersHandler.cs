using System.Linq.Expressions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Common;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.Admin.GetPaginated;

public class GetPaginatedUsersHandler : IRequestHandler<GetPaginatedUsersQuery, Result<GetPaginatedUsersResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedUsersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetPaginatedUsersResponse>> Handle(GetPaginatedUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsNoTracking()
            .Where(x => x.UserRoles.Any(ur => ur.Role!.Name == Application.Common.SeedData.Roles.Admin.Name) == false)
            .Where(x => x.UserBranches.Any(ub => ub.BranchId == request.Filter.BranchId))
            .When(request.Filter.RoleIds is not null, x => x.UserRoles.Any(ur => request.Filter.RoleIds!.Contains(ur.RoleId)))
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null,
                x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null,
                x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var selectedFieldKeys = request.SelectedColumns?.Count > 0
            ? request.SelectedColumns
            : UserColumns.GetAllKeys().ToList();

        var fieldSelectors = new Dictionary<string, Expression<Func<User, object?>>>();
        foreach (var key in selectedFieldKeys)
        {
            var fieldDef = UserColumns.GetField(key);
            if (fieldDef != null)
            {
                fieldSelectors[key] = fieldDef.Selector;
            }
        }

        var paginatedQuery = query
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt!.Value)
            .Skip((request.Pagination.PageIndex - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize);

        var items = await paginatedQuery.ProjectToDynamicAsync(fieldSelectors, cancellationToken);

        var paginatedList = new PaginatedList<Dictionary<string, object?>>(
            items,
            totalCount,
            request.Pagination.PageIndex,
            request.Pagination.PageSize);

        return new GetPaginatedUsersResponse
        {
            PaginatedUsers = paginatedList
        };
    }
}
