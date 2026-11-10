using Domain.Common;
using Domain.Entities.AnnouncementAggregate.Enums;

namespace Domain.Entities.AnnouncementAggregate;

/// <summary>
/// إعلان بانر للتطبيقات الميدانية — منفصل عن AppNote وNotification وPush.
/// </summary>
public class Announcement : Entity
{
    private Announcement()
    {
    }

    public Announcement(
        AnnouncementAppKey appKey,
        AnnouncementContentType contentType,
        string textBody,
        string? bannerStorageKey,
        string? zoomStorageKey,
        bool hasLink,
        string? linkUrl,
        bool isEnabled,
        int sortOrder,
        int createdByUserId,
        string? createdByName,
        DateTimeOffset createdAtUtc)
    {
        AppKey = appKey;
        ApplyContent(contentType, textBody, bannerStorageKey, zoomStorageKey, hasLink, linkUrl);
        IsEnabled = isEnabled;
        SortOrder = sortOrder;
        CreatedByUserId = createdByUserId;
        CreatedByName = createdByName;
        UpdatedByUserId = createdByUserId;
        UpdatedByName = createdByName;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        ConcurrencyStamp = 0;
    }

    public AnnouncementAppKey AppKey { get; private set; }
    public AnnouncementContentType ContentType { get; private set; }
    public string TextBody { get; private set; } = "";
    public string? BannerStorageKey { get; private set; }
    public string? ZoomStorageKey { get; private set; }
    public bool HasLink { get; private set; }
    public string? LinkUrl { get; private set; }
    public bool IsEnabled { get; private set; }
    public int SortOrder { get; private set; }
    public int CreatedByUserId { get; private set; }
    public string? CreatedByName { get; private set; }
    public int UpdatedByUserId { get; private set; }
    public string? UpdatedByName { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public void Update(
        AnnouncementContentType contentType,
        string textBody,
        string? bannerStorageKey,
        string? zoomStorageKey,
        bool hasLink,
        string? linkUrl,
        bool isEnabled,
        int actorId,
        string? actorName,
        int expectedVersion,
        DateTimeOffset at)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        ApplyContent(contentType, textBody, bannerStorageKey, zoomStorageKey, hasLink, linkUrl);
        IsEnabled = isEnabled;
        UpdatedByUserId = actorId;
        UpdatedByName = actorName;
        UpdatedAtUtc = at;
        ConcurrencyStamp++;
    }

    public void SetEnabled(bool enabled, int actorId, string? actorName, int expectedVersion, DateTimeOffset at)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        IsEnabled = enabled;
        UpdatedByUserId = actorId;
        UpdatedByName = actorName;
        UpdatedAtUtc = at;
        ConcurrencyStamp++;
    }

    private void ApplyContent(
        AnnouncementContentType contentType,
        string textBody,
        string? bannerStorageKey,
        string? zoomStorageKey,
        bool hasLink,
        string? linkUrl)
    {
        ContentType = contentType;
        if (contentType == AnnouncementContentType.Text)
        {
            var body = (textBody ?? "").Trim();
            if (body.Length is < 1 or > 2000)
                throw new InvalidOperationException("نص الإعلان مطلوب وبحد أقصى 2000 حرف.");
            TextBody = body;
            BannerStorageKey = null;
            ZoomStorageKey = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(bannerStorageKey) || string.IsNullOrWhiteSpace(zoomStorageKey))
                throw new InvalidOperationException("صورة البانر وصورة التكبير مطلوبتان.");
            TextBody = "";
            BannerStorageKey = bannerStorageKey.Trim();
            ZoomStorageKey = zoomStorageKey.Trim();
        }

        HasLink = hasLink;
        if (hasLink)
        {
            var url = (linkUrl ?? "").Trim();
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) == false
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("الرابط يجب أن يكون http أو https.");
            LinkUrl = url;
        }
        else
        {
            LinkUrl = null;
        }
    }
}
