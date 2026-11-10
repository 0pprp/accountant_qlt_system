using Application.AppNotes.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.AppNoteAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.AppNotes.Queries;

public sealed class ListAppNotesQuery : IRequest<Result<AppNoteListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool UnreadOnly { get; init; }
    public int? BranchId { get; init; }
    public string? SourceApp { get; init; }
    public string? Search { get; init; }
}

public sealed class GetAppNoteByIdQuery : IRequest<Result<AppNoteDetailDto>>
{
    public int Id { get; init; }
}

public sealed class GetAppNoteUnreadCountQuery : IRequest<Result<int>>;

public sealed class ListAppNotesHandler : IRequestHandler<ListAppNotesQuery, Result<AppNoteListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListAppNotesHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<AppNoteListResponse>> Handle(ListAppNotesQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.AppNote.Read) == false)
            return AppNoteErrors.Forbidden;

        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _db.AppNotes.AsNoTracking().AsQueryable();
        if (request.UnreadOnly)
            query = query.Where(x => x.Status == AppNoteStatus.Unread);
        if (request.BranchId is > 0)
            query = query.Where(x => x.BranchId == request.BranchId);
        if (!string.IsNullOrWhiteSpace(request.SourceApp)
            && Enum.TryParse<AppNoteSourceApp>(request.SourceApp, true, out var src))
            query = query.Where(x => x.SourceApp == src);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(x =>
                x.Subject.Contains(s) || x.Body.Contains(s) || x.SenderDisplayName.Contains(s));
        }

        var total = await query.CountAsync(ct);
        var unread = await _db.AppNotes.AsNoTracking()
            .CountAsync(x => x.Status == AppNoteStatus.Unread, ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(ct);

        return new AppNoteListResponse(
            items.Select(AppNoteMapper.ToSummary).ToList(),
            total,
            page,
            size,
            unread);
    }
}

public sealed class GetAppNoteByIdHandler : IRequestHandler<GetAppNoteByIdQuery, Result<AppNoteDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetAppNoteByIdHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<AppNoteDetailDto>> Handle(GetAppNoteByIdQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.AppNote.Read) == false)
            return AppNoteErrors.Forbidden;

        var note = await _db.AppNotes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return note is null ? AppNoteErrors.NotFound : AppNoteMapper.ToDetail(note);
    }
}

public sealed class GetAppNoteUnreadCountHandler : IRequestHandler<GetAppNoteUnreadCountQuery, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetAppNoteUnreadCountHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<int>> Handle(GetAppNoteUnreadCountQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.AppNote.Read) == false)
            return AppNoteErrors.Forbidden;
        return await _db.AppNotes.AsNoTracking().CountAsync(x => x.Status == AppNoteStatus.Unread, ct);
    }
}
