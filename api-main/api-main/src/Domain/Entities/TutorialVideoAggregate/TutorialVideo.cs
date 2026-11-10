using Domain.Common;
using Domain.Entities.TutorialVideoAggregate.Enums;

namespace Domain.Entities.TutorialVideoAggregate;

/// <summary>
/// فيديو تعليمي كسؤال + رابط خارجي — لا رفع ملف ولا تتبع مشاهدة.
/// </summary>
public class TutorialVideo : Entity
{
    private TutorialVideo()
    {
    }

    public TutorialVideo(
        TutorialAppKey appKey,
        string questionAr,
        string videoUrl,
        bool isEnabled,
        int sortOrder,
        int actorUserId,
        string? actorName,
        DateTimeOffset at)
    {
        AppKey = appKey;
        Apply(questionAr, videoUrl, isEnabled);
        SortOrder = sortOrder;
        CreatedByUserId = actorUserId;
        CreatedByName = actorName;
        UpdatedByUserId = actorUserId;
        UpdatedByName = actorName;
        CreatedAtUtc = at;
        UpdatedAtUtc = at;
        ConcurrencyStamp = 0;
    }

    public TutorialAppKey AppKey { get; private set; }
    public string QuestionAr { get; private set; } = "";
    public string VideoUrl { get; private set; } = "";
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
        string questionAr,
        string videoUrl,
        bool isEnabled,
        int actorId,
        string? actorName,
        int expectedVersion,
        DateTimeOffset at)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        Apply(questionAr, videoUrl, isEnabled);
        UpdatedByUserId = actorId;
        UpdatedByName = actorName;
        UpdatedAtUtc = at;
        ConcurrencyStamp++;
    }

    private void Apply(string questionAr, string videoUrl, bool isEnabled)
    {
        var q = (questionAr ?? "").Trim();
        if (q.Length is < 1 or > 400)
            throw new InvalidOperationException("السؤال مطلوب وبحد أقصى 400 حرف.");
        var url = (videoUrl ?? "").Trim();
        if (TutorialVideoUrlRules.IsAllowed(url) == false)
            throw new InvalidOperationException(
                "رابط الفيديو يجب أن يكون https لمضيف مدعوم (YouTube / Vimeo).");
        QuestionAr = q;
        VideoUrl = url;
        IsEnabled = isEnabled;
    }
}

/// <summary>تشديد أمني: روابط خارجية مقيّدة بالأنواع المدعومة فقط.</summary>
public static class TutorialVideoUrlRules
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "youtube.com",
        "www.youtube.com",
        "m.youtube.com",
        "youtu.be",
        "www.youtu.be",
        "vimeo.com",
        "www.vimeo.com",
        "player.vimeo.com",
    };

    public static bool IsAllowed(string? videoUrl)
    {
        if (Uri.TryCreate((videoUrl ?? "").Trim(), UriKind.Absolute, out var uri) == false)
            return false;
        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;
        return AllowedHosts.Contains(uri.Host);
    }
}

