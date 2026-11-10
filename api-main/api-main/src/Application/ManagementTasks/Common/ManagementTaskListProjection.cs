using Application.Common.Models;
using Domain.Entities.ManagementTaskAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskListProjection
{
    public static IQueryable<ManagementTask> ApplyOrdering(IQueryable<ManagementTask> query)
    {
        return query
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.Id);
    }

    public static IQueryable<ManagementTask> ApplySearch(IQueryable<ManagementTask> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim();
        if (int.TryParse(term, out var idTerm))
        {
            return query.Where(x =>
                x.Id == idTerm
                || x.Title.Contains(term)
                || x.Description.Contains(term)
                || x.AssigneeDisplayName.Contains(term)
                || x.CurrentHolderDisplayName.Contains(term));
        }

        return query.Where(x =>
            x.Title.Contains(term)
            || x.Description.Contains(term)
            || x.AssigneeDisplayName.Contains(term)
            || x.CurrentHolderDisplayName.Contains(term));
    }

    public static async Task<PaginatedList<ManagementTaskListItemDto>> ToPageAsync(
        IQueryable<ManagementTask> query,
        Pagination pagination,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        var pageSize = pagination.PageSize < 1 ? 10 : pagination.PageSize;

        var page = await ApplyOrdering(query)
            .Select(x => new
            {
                Task = x,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                CreatedByName = x.CreatedByUser != null ? x.CreatedByUser.FullName : null
            })
            .ToPaginatedListAsync(pageIndex, pageSize, cancellationToken);

        var items = page.Items
            .Select(x => ManagementTaskMapper.ToListItem(x.Task, utcNow, x.BranchName, x.CreatedByName))
            .ToList();

        return new PaginatedList<ManagementTaskListItemDto>(items, page.TotalCount, page.PageIndex, page.PageSize);
    }
}
