using Application.Announcements.Common;
using Application.Common.Content;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.AnnouncementAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Announcements.Queries;

public sealed class ListAnnouncementsQuery : IRequest<Result<AnnouncementListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? AppKey { get; init; }
    public bool? IsEnabled { get; init; }
}

public sealed class ListAnnouncementsHandler
    : IRequestHandler<ListAnnouncementsQuery, Result<AnnouncementListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListAnnouncementsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<AnnouncementListResponse>> Handle(ListAnnouncementsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.Read) == false)
            return AnnouncementErrors.Forbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var q = _db.Announcements.AsNoTracking().AsQueryable();

        if (string.IsNullOrWhiteSpace(request.AppKey) == false)
        {
            if (AnnouncementLabels.TryParseAppKey(request.AppKey, out var key) == false)
                return AnnouncementErrors.InvalidAppKey;
            q = q.Where(x => x.AppKey == key);
        }

        if (request.IsEnabled is bool en)
            q = q.Where(x => x.IsEnabled == en);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.AppKey)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new AnnouncementListResponse(
            items.Select(a => AnnouncementMapper.ToAdmin(a, AnnouncementPaths.PublicUrl)).ToList(),
            total, page, pageSize);
    }
}

public sealed class GetAnnouncementByIdQuery : IRequest<Result<AnnouncementAdminDto>>
{
    public int Id { get; init; }
}

public sealed class GetAnnouncementByIdHandler
    : IRequestHandler<GetAnnouncementByIdQuery, Result<AnnouncementAdminDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetAnnouncementByIdHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<AnnouncementAdminDto>> Handle(GetAnnouncementByIdQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.Read) == false)
            return AnnouncementErrors.Forbidden;
        var a = await _db.Announcements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return a is null
            ? AnnouncementErrors.NotFound
            : AnnouncementMapper.ToAdmin(a, AnnouncementPaths.PublicUrl);
    }
}

public sealed class ListPublicAnnouncementsQuery : IRequest<Result<IReadOnlyList<AnnouncementPublicDto>>>
{
    public required string AppKey { get; init; }
}

public sealed class ListPublicAnnouncementsHandler
    : IRequestHandler<ListPublicAnnouncementsQuery, Result<IReadOnlyList<AnnouncementPublicDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ListPublicAnnouncementsHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<AnnouncementPublicDto>>> Handle(
        ListPublicAnnouncementsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.AppRead) == false)
            return AnnouncementErrors.Forbidden;
        if (AnnouncementLabels.TryParseAppKey(request.AppKey, out var key) == false)
            return AnnouncementErrors.InvalidAppKey;

        var allowed = await ContentAppAudience.ResolveAnnouncementAsync(_db, _current, _clock, ct);
        if (allowed is null || ContentAppAudience.AllowsAnnouncement(allowed.Value, key) == false)
            return AnnouncementErrors.AudienceDenied;

        var items = await _db.Announcements.AsNoTracking()
            .Where(x => x.AppKey == key && x.IsEnabled)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        return items.Select(a => AnnouncementMapper.ToPublic(a, AnnouncementPaths.PublicUrl)).ToList();
    }
}

public sealed record AnnouncementMediaDto(string RelativePath, string ContentType, string FileName);

public sealed class GetAnnouncementMediaQuery : IRequest<Result<AnnouncementMediaDto>>
{
    public required string FileName { get; init; }
}

public sealed class GetAnnouncementMediaHandler
    : IRequestHandler<GetAnnouncementMediaQuery, Result<AnnouncementMediaDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public GetAnnouncementMediaHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<AnnouncementMediaDto>> Handle(GetAnnouncementMediaQuery request, CancellationToken ct)
    {
        var canAdminRead = _current.HasPermission(SeedPermissions.Announcement.Read);
        var canAppRead = _current.HasPermission(SeedPermissions.Announcement.AppRead);
        if (canAdminRead == false && canAppRead == false)
            return AnnouncementErrors.Forbidden;

        var safeName = Path.GetFileName(request.FileName ?? "");
        if (string.IsNullOrWhiteSpace(safeName) || safeName != (request.FileName ?? "").Trim())
            return AnnouncementErrors.NotFound;

        var rows = await _db.Announcements.AsNoTracking()
            .Where(a => a.BannerStorageKey != null || a.ZoomStorageKey != null)
            .Select(a => new { a.AppKey, a.IsEnabled, a.BannerStorageKey, a.ZoomStorageKey })
            .ToListAsync(ct);

        var hit = rows.FirstOrDefault(r =>
            (r.BannerStorageKey != null && Path.GetFileName(r.BannerStorageKey) == safeName)
            || (r.ZoomStorageKey != null && Path.GetFileName(r.ZoomStorageKey) == safeName));

        if (hit is null)
            return AnnouncementErrors.NotFound;

        // قارئ التطبيق: جمهور الجلسة فقط + مفعّل. لوحة الإدارة (Read) بلا تقييد جمهور.
        if (canAdminRead == false)
        {
            if (hit.IsEnabled == false)
                return AnnouncementErrors.NotFound;
            var allowed = await ContentAppAudience.ResolveAnnouncementAsync(_db, _current, _clock, ct);
            if (allowed is null || ContentAppAudience.AllowsAnnouncement(allowed.Value, hit.AppKey) == false)
                return AnnouncementErrors.AudienceDenied;
        }

        var storageKey =
            hit.BannerStorageKey != null && Path.GetFileName(hit.BannerStorageKey) == safeName
                ? hit.BannerStorageKey
                : hit.ZoomStorageKey!;

        var ext = Path.GetExtension(safeName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        return new AnnouncementMediaDto(storageKey, contentType, safeName);
    }
}
