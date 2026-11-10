using Domain.Entities.MobileAppReleaseAggregate;

namespace Application.MobileAppReleases.Common;

public sealed record MobileAppReleaseDto(
    int Id,
    string AppKey,
    string AppKeyLabel,
    string LatestVersionName,
    int LatestVersionCode,
    int MinimumSupportedVersionCode,
    bool ForceUpdate,
    string ApkUrl,
    string? Sha256,
    string? ReleaseNotes,
    DateTimeOffset UpdatedAtUtc,
    string? UpdatedByName,
    int ExpectedVersion);

public sealed record MobileUpdateCheckDto(
    string AppKey,
    string LatestVersionName,
    int LatestVersionCode,
    int MinimumSupportedVersionCode,
    bool ForceUpdate,
    string? ApkUrl,
    string? Sha256,
    string? ReleaseNotes,
    DateTimeOffset UpdatedAtUtc,
    string UpdateKind,
    bool UpdateAvailable,
    bool Mandatory);

/// <summary>توافق مسار checkVersion القديم.</summary>
public sealed record LegacyAppVersionResponse(bool ShouldUpdate, bool IsForce, string Url);

public static class MobileAppReleaseMapper
{
    public static MobileAppReleaseDto ToDto(MobileAppRelease e) => new(
        e.Id,
        e.AppKey,
        MobileAppKeys.LabelAr(e.AppKey),
        e.LatestVersionName,
        e.LatestVersionCode,
        e.MinimumSupportedVersionCode,
        e.ForceUpdate,
        e.ApkUrl,
        e.Sha256,
        e.ReleaseNotes,
        e.UpdatedAtUtc,
        e.UpdatedByName,
        e.ConcurrencyStamp);

    public static MobileUpdateCheckDto ToCheck(MobileAppRelease e, int currentVersionCode)
    {
        var kind = MobileUpdateRules.Evaluate(
            currentVersionCode,
            e.LatestVersionCode,
            e.MinimumSupportedVersionCode,
            e.ForceUpdate);
        return new MobileUpdateCheckDto(
            e.AppKey,
            e.LatestVersionName,
            e.LatestVersionCode,
            e.MinimumSupportedVersionCode,
            e.ForceUpdate,
            string.IsNullOrWhiteSpace(e.ApkUrl) ? null : e.ApkUrl,
            e.Sha256,
            e.ReleaseNotes,
            e.UpdatedAtUtc,
            kind.ToString(),
            kind != MobileUpdateKind.None,
            kind == MobileUpdateKind.Mandatory);
    }

    public static LegacyAppVersionResponse ToLegacy(MobileUpdateCheckDto check) => new(
        check.UpdateAvailable,
        check.Mandatory,
        check.ApkUrl ?? "");
}
