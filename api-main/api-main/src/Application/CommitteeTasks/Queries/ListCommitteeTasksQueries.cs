using Application.Common.Interfaces;
using Application.CommitteeTasks.Common;
using Application.CommitteeTasks.Commands.Admin.Upsert;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.CommitteeTasks.Queries;

public sealed class ListAdminCommitteeTasksQuery : IRequest<Result<IReadOnlyList<CommitteeTaskRowDto>>>;

public sealed class ListAppCommitteeTasksQuery : IRequest<Result<IReadOnlyList<CommitteeTaskAppItemDto>>>;

public sealed class ListAdminCommitteeTasksHandler
    : IRequestHandler<ListAdminCommitteeTasksQuery, Result<IReadOnlyList<CommitteeTaskRowDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListAdminCommitteeTasksHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<IReadOnlyList<CommitteeTaskRowDto>>> Handle(
        ListAdminCommitteeTasksQuery request,
        CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CommitteeTask.Read) == false)
            return CommitteeTaskErrors.Forbidden;

        var rows = await _db.CommitteeTaskItems.AsNoTracking()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(ct);
        return rows.Select(CreateCommitteeTaskHandler.Map).ToList();
    }
}

public sealed class ListAppCommitteeTasksHandler
    : IRequestHandler<ListAppCommitteeTasksQuery, Result<IReadOnlyList<CommitteeTaskAppItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListAppCommitteeTasksHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<IReadOnlyList<CommitteeTaskAppItemDto>>> Handle(
        ListAppCommitteeTasksQuery request,
        CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CommitteeTask.AppRead) == false)
            return CommitteeTaskErrors.Forbidden;

        var rows = await _db.CommitteeTaskItems.AsNoTracking()
            .Where(x => x.IsEnabled && x.Body != "")
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new CommitteeTaskAppItemDto(x.Id, x.Body, x.SortOrder))
            .ToListAsync(ct);
        return rows;
    }
}
