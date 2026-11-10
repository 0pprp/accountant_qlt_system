using Domain.Common;

namespace Domain.Entities.MobileAppReleaseAggregate;

/// <summary>
/// إصدار منشور لتطبيق ميداني — يحدد الحد الأدنى والتحديث الإجباري ورابط APK.
/// </summary>
public class MobileAppRelease : Entity
{
    private MobileAppRelease()
    {
    }

    public MobileAppRelease(
        string appKey,
        string latestVersionName,
        int latestVersionCode,
        int minimumSupportedVersionCode,
        bool forceUpdate,
        string apkUrl,
        string? sha256,
        string? releaseNotes,
        int? updatedByUserId,
        string? updatedByName,
        DateTimeOffset updatedAtUtc)
    {
        AppKey = NormalizeAppKey(appKey);
        Apply(
            latestVersionName,
            latestVersionCode,
            minimumSupportedVersionCode,
            forceUpdate,
            apkUrl,
            sha256,
            releaseNotes,
            updatedByUserId,
            updatedByName,
            updatedAtUtc,
            bumpStamp: false);
    }

    public string AppKey { get; private set; } = "";
    public string LatestVersionName { get; private set; } = "";
    public int LatestVersionCode { get; private set; }
    public int MinimumSupportedVersionCode { get; private set; }
    public bool ForceUpdate { get; private set; }
    public string ApkUrl { get; private set; } = "";
    public string? Sha256 { get; private set; }
    public string? ReleaseNotes { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public int? UpdatedByUserId { get; private set; }
    public string? UpdatedByName { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public void Publish(
        string latestVersionName,
        int latestVersionCode,
        int minimumSupportedVersionCode,
        bool forceUpdate,
        string apkUrl,
        string? sha256,
        string? releaseNotes,
        int? updatedByUserId,
        string? updatedByName,
        int expectedVersion,
        DateTimeOffset updatedAtUtc)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");

        Apply(
            latestVersionName,
            latestVersionCode,
            minimumSupportedVersionCode,
            forceUpdate,
            apkUrl,
            sha256,
            releaseNotes,
            updatedByUserId,
            updatedByName,
            updatedAtUtc,
            bumpStamp: true);
    }

    private void Apply(
        string latestVersionName,
        int latestVersionCode,
        int minimumSupportedVersionCode,
        bool forceUpdate,
        string apkUrl,
        string? sha256,
        string? releaseNotes,
        int? updatedByUserId,
        string? updatedByName,
        DateTimeOffset updatedAtUtc,
        bool bumpStamp)
    {
        if (string.IsNullOrWhiteSpace(latestVersionName))
            throw new ArgumentException("اسم الإصدار مطلوب.", nameof(latestVersionName));
        if (latestVersionCode < 1)
            throw new ArgumentOutOfRangeException(nameof(latestVersionCode));
        if (minimumSupportedVersionCode < 1)
            throw new ArgumentOutOfRangeException(nameof(minimumSupportedVersionCode));
        if (minimumSupportedVersionCode > latestVersionCode)
            throw new ArgumentException("الحد الأدنى لا يتجاوز أحدث إصدار.");

        LatestVersionName = latestVersionName.Trim();
        LatestVersionCode = latestVersionCode;
        MinimumSupportedVersionCode = minimumSupportedVersionCode;
        ForceUpdate = forceUpdate;
        ApkUrl = (apkUrl ?? "").Trim();
        Sha256 = NormalizeOptional(sha256)?.ToLowerInvariant();
        ReleaseNotes = NormalizeOptional(releaseNotes);
        UpdatedByUserId = updatedByUserId;
        UpdatedByName = NormalizeOptional(updatedByName);
        UpdatedAtUtc = updatedAtUtc;
        if (bumpStamp)
            ConcurrencyStamp++;
    }

    public static string NormalizeAppKey(string appKey)
    {
        var key = (appKey ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("مفتاح التطبيق مطلوب.", nameof(appKey));
        return key;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}

public enum MobileUpdateKind : byte
{
    None = 0,
    Optional = 1,
    Mandatory = 2
}

public static class MobileUpdateRules
{
    public static MobileUpdateKind Evaluate(
        int currentVersionCode,
        int latestVersionCode,
        int minimumSupportedVersionCode,
        bool forceUpdate)
    {
        if (currentVersionCode >= latestVersionCode)
            return MobileUpdateKind.None;
        if (currentVersionCode < minimumSupportedVersionCode)
            return MobileUpdateKind.Mandatory;
        if (forceUpdate)
            return MobileUpdateKind.Mandatory;
        return MobileUpdateKind.Optional;
    }
}

public static class MobileAppKeys
{
    public const string Delegate = "delegate";
    public const string SalesEmployee = "sales-employee";
    public const string Committee = "committee";
    public const string Follower = "follower";

    public static readonly string[] All =
    [
        Delegate,
        SalesEmployee,
        Committee,
        Follower
    ];

    public static string LabelAr(string appKey) => NormalizeKnown(appKey) switch
    {
        Delegate => "المندوب",
        SalesEmployee => "موظف المبيعات",
        Committee => "مسؤول اللجنة",
        Follower => "المتابع / التطبيق الميداني",
        _ => appKey
    };

    public static string? NormalizeKnown(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim().ToLowerInvariant();
        return s switch
        {
            "delegate" or "مندوب" or "المندوب" => Delegate,
            "sales-employee" or "salesemployee" or "sales_employee" or "موظف المبيعات" => SalesEmployee,
            "committee" or "committee-officer" or "لجنة" or "مسؤول اللجنة" => Committee,
            "follower" or "motaba" or "app-dev" or "متابع" or "المتابع" => Follower,
            _ => s
        };
    }
}
