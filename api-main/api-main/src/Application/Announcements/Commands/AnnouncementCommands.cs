using Application.Announcements.Common;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.SalesRequests.Common;
using Domain.Entities.AnnouncementAggregate;
using Domain.Entities.AnnouncementAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Announcements.Commands;

public sealed class CreateAnnouncementCommand : IRequest<Result<AnnouncementAdminDto>>
{
    public required string AppKey { get; init; }
    public required string ContentType { get; init; }
    public string? TextBody { get; init; }
    public string? BannerStorageKey { get; init; }
    public string? ZoomStorageKey { get; init; }
    public bool HasLink { get; init; }
    public string? LinkUrl { get; init; }
    public bool IsEnabled { get; init; } = true;
}

public sealed class UpdateAnnouncementCommand : IRequest<Result<AnnouncementAdminDto>>
{
    public int Id { get; init; }
    public required string ContentType { get; init; }
    public string? TextBody { get; init; }
    public string? BannerStorageKey { get; init; }
    public string? ZoomStorageKey { get; init; }
    public bool HasLink { get; init; }
    public string? LinkUrl { get; init; }
    public bool IsEnabled { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class DeleteAnnouncementCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class UploadAnnouncementImageCommand : IRequest<Result<AnnouncementUploadResult>>
{
    public required IFormFile File { get; init; }
}

file static class AnnouncementSupport
{
    public const long MaxImageBytes = 8 * 1024 * 1024;

    public static bool TryParseContentType(string? raw, out AnnouncementContentType type)
    {
        type = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.Trim().ToLowerInvariant();
        type = s switch
        {
            "text" or "نص" => AnnouncementContentType.Text,
            "image" or "صورة" => AnnouncementContentType.Image,
            _ => default
        };
        return type != default;
    }

}

public sealed class CreateAnnouncementHandler
    : IRequestHandler<CreateAnnouncementCommand, Result<AnnouncementAdminDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public CreateAnnouncementHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<AnnouncementAdminDto>> Handle(CreateAnnouncementCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.Create) == false)
            return AnnouncementErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return AnnouncementErrors.Forbidden;
        if (AnnouncementLabels.TryParseAppKey(request.AppKey, out var appKey) == false)
            return AnnouncementErrors.InvalidAppKey;
        if (AnnouncementSupport.TryParseContentType(request.ContentType, out var contentType) == false)
            return AnnouncementErrors.Validation;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var nextSort = (await _db.Announcements.AsNoTracking()
            .Where(a => a.AppKey == appKey)
            .Select(a => (int?)a.SortOrder)
            .MaxAsync(ct) ?? 0) + 1;

        Announcement entity;
        try
        {
            entity = new Announcement(
                appKey, contentType, request.TextBody ?? "",
                request.BannerStorageKey, request.ZoomStorageKey,
                request.HasLink, request.LinkUrl, request.IsEnabled, nextSort,
                actorId, actorName, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException)
        {
            return AnnouncementErrors.Validation;
        }

        _db.Announcements.Add(entity);
        await _db.SaveChangesAsync(ct);
        return AnnouncementMapper.ToAdmin(entity, AnnouncementPaths.PublicUrl);
    }
}

public sealed class UpdateAnnouncementHandler
    : IRequestHandler<UpdateAnnouncementCommand, Result<AnnouncementAdminDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IFileManager _files;

    public UpdateAnnouncementHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock, IFileManager files)
    {
        _db = db;
        _current = current;
        _clock = clock;
        _files = files;
    }

    public async Task<Result<AnnouncementAdminDto>> Handle(UpdateAnnouncementCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.Update) == false)
            return AnnouncementErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return AnnouncementErrors.Forbidden;
        if (AnnouncementSupport.TryParseContentType(request.ContentType, out var contentType) == false)
            return AnnouncementErrors.Validation;

        var entity = await _db.Announcements.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return AnnouncementErrors.NotFound;

        var oldBanner = entity.BannerStorageKey;
        var oldZoom = entity.ZoomStorageKey;
        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);

        var banner = string.IsNullOrWhiteSpace(request.BannerStorageKey) ? entity.BannerStorageKey : request.BannerStorageKey;
        var zoom = string.IsNullOrWhiteSpace(request.ZoomStorageKey) ? entity.ZoomStorageKey : request.ZoomStorageKey;

        try
        {
            entity.Update(
                contentType, request.TextBody ?? "", banner, zoom,
                request.HasLink, request.LinkUrl, request.IsEnabled,
                actorId, actorName, request.ExpectedVersion, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return AnnouncementErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return AnnouncementErrors.Validation;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return AnnouncementErrors.Concurrency;
        }

        // حذف الملفات القديمة بعد نجاح الحفظ فقط
        if (oldBanner is not null && oldBanner != entity.BannerStorageKey)
            TryDelete(oldBanner);
        if (oldZoom is not null && oldZoom != entity.ZoomStorageKey)
            TryDelete(oldZoom);

        return AnnouncementMapper.ToAdmin(entity, AnnouncementPaths.PublicUrl);
    }

    private void TryDelete(string key)
    {
        try { _files.Delete(key); } catch { /* best-effort orphan cleanup */ }
    }
}

public sealed class DeleteAnnouncementHandler : IRequestHandler<DeleteAnnouncementCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IFileManager _files;

    public DeleteAnnouncementHandler(IApplicationDbContext db, ICurrentUserService current, IFileManager files)
    {
        _db = db;
        _current = current;
        _files = files;
    }

    public async Task<Result> Handle(DeleteAnnouncementCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.Delete) == false)
            return AnnouncementErrors.Forbidden;

        var entity = await _db.Announcements.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return AnnouncementErrors.NotFound;
        if (entity.MatchesExpectedVersion(request.ExpectedVersion) == false)
            return AnnouncementErrors.Concurrency;

        var banner = entity.BannerStorageKey;
        var zoom = entity.ZoomStorageKey;
        _db.Announcements.Remove(entity);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return AnnouncementErrors.Concurrency;
        }

        if (banner is not null) try { _files.Delete(banner); } catch { }
        if (zoom is not null) try { _files.Delete(zoom); } catch { }
        return Result.Success();
    }
}

public sealed class UploadAnnouncementImageHandler
    : IRequestHandler<UploadAnnouncementImageCommand, Result<AnnouncementUploadResult>>
{
    private readonly ICurrentUserService _current;
    private readonly IFileManager _files;

    public UploadAnnouncementImageHandler(ICurrentUserService current, IFileManager files)
    {
        _current = current;
        _files = files;
    }

    public async Task<Result<AnnouncementUploadResult>> Handle(
        UploadAnnouncementImageCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Announcement.Create) == false
            && _current.HasPermission(SeedPermissions.Announcement.Update) == false)
            return AnnouncementErrors.Forbidden;

        var file = request.File;
        if (file is null || file.Length <= 0 || file.Length > AnnouncementSupport.MaxImageBytes)
            return AnnouncementErrors.InvalidFile;

        var contentType = (file.ContentType ?? "").Trim().ToLowerInvariant();
        // لا نثق بالامتداد وحده — نطابق قائمة صور AttachmentUtility مع استبعاد svg
        var allowed = AttachmentUtility.ImageContentTypes
            .Where(x => x is not "image/svg+xml")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (allowed.Contains(contentType) == false)
            return AnnouncementErrors.InvalidFile;

        var folder = Path.Combine(AttachmentPath.Root, AttachmentPath.AnnouncementsSubfolder());
        var saved = await _files.SaveFileAsync(file, folder, ct);
        return new AnnouncementUploadResult(saved.RelativePath, contentType, file.Length);
    }
}
