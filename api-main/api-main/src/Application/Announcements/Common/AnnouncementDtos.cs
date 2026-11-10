using Domain.Entities.AnnouncementAggregate.Enums;

namespace Application.Announcements.Common;

public sealed record AnnouncementAdminDto(
    int Id,
    string AppKey,
    string AppKeyLabel,
    string ContentType,
    string ContentTypeLabel,
    string TextBody,
    string? BannerUrl,
    string? ZoomUrl,
    bool HasLink,
    string? LinkUrl,
    bool IsEnabled,
    int SortOrder,
    string? CreatedByName,
    string? UpdatedByName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int ExpectedVersion);

public sealed record AnnouncementListResponse(
    IReadOnlyList<AnnouncementAdminDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AnnouncementPublicDto(
    int Id,
    string AppKey,
    string ContentType,
    string TextBody,
    string? BannerUrl,
    string? ZoomUrl,
    bool HasLink,
    string? LinkUrl,
    int SortOrder);

public sealed record AnnouncementUploadResult(string StorageKey, string ContentType, long SizeBytes);

public static class AnnouncementErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "Announcement.Forbidden");
    public static readonly Error NotFound = new("الإعلان غير موجود.", "Announcement.NotFound");
    public static readonly Error Validation = new("بيانات غير صالحة.", "Announcement.Validation");
    public static readonly Error Concurrency = new("تم تعديل الإعلان من جهة أخرى.", "Announcement.Concurrency");
    public static readonly Error InvalidAppKey = new("جمهور التطبيق غير صالح.", "Announcement.InvalidAppKey");
    public static readonly Error InvalidFile = new("ملف غير مسموح أو يتجاوز الحجم.", "Announcement.InvalidFile");
    public static readonly Error AudienceDenied = new("الإعلان خارج نطاق تطبيقك.", "Announcement.AudienceDenied");
}

public static class AnnouncementPaths
{
    public static string? PublicUrl(string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) return null;
        var name = Path.GetFileName(storageKey);
        return string.IsNullOrWhiteSpace(name)
            ? null
            : $"/api/v1/app/Announcements/media/{Uri.EscapeDataString(name)}";
    }
}

public static class AnnouncementMapper
{
    public static AnnouncementAdminDto ToAdmin(
        Domain.Entities.AnnouncementAggregate.Announcement a,
        Func<string?, string?> url) =>
        new(
            a.Id,
            a.AppKey.ToString().ToLowerInvariant(),
            AnnouncementLabels.AppKeyArabic(a.AppKey),
            a.ContentType.ToString().ToLowerInvariant(),
            AnnouncementLabels.ContentTypeArabic(a.ContentType),
            a.TextBody,
            url(a.BannerStorageKey),
            url(a.ZoomStorageKey),
            a.HasLink,
            a.LinkUrl,
            a.IsEnabled,
            a.SortOrder,
            a.CreatedByName,
            a.UpdatedByName,
            a.CreatedAtUtc,
            a.UpdatedAtUtc,
            a.ConcurrencyStamp);

    public static AnnouncementPublicDto ToPublic(
        Domain.Entities.AnnouncementAggregate.Announcement a,
        Func<string?, string?> url) =>
        new(
            a.Id,
            a.AppKey.ToString().ToLowerInvariant(),
            a.ContentType.ToString().ToLowerInvariant(),
            a.TextBody,
            url(a.BannerStorageKey),
            url(a.ZoomStorageKey),
            a.HasLink,
            a.LinkUrl,
            a.SortOrder);
}
