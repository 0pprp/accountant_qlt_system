namespace Domain.Entities.AnnouncementAggregate.Enums;

public enum AnnouncementAppKey : byte
{
    Delegate = 1,
    Follower = 2
}

public enum AnnouncementContentType : byte
{
    Text = 1,
    Image = 2
}

public static class AnnouncementLabels
{
    public static string AppKeyArabic(AnnouncementAppKey k) => k switch
    {
        AnnouncementAppKey.Delegate => "المندوب",
        AnnouncementAppKey.Follower => "المتابع",
        _ => k.ToString()
    };

    public static string ContentTypeArabic(AnnouncementContentType t) => t switch
    {
        AnnouncementContentType.Text => "نص",
        AnnouncementContentType.Image => "صورة",
        _ => t.ToString()
    };

    public static bool TryParseAppKey(string? raw, out AnnouncementAppKey key)
    {
        key = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.Trim().ToLowerInvariant();
        key = s switch
        {
            "delegate" or "مندوب" or "المندوب" => AnnouncementAppKey.Delegate,
            "follower" or "متابع" or "المتابع" or "motaba" or "committee"
                or "مسؤول اللجنة" => AnnouncementAppKey.Follower,
            _ => default
        };
        return key != default;
    }
}
