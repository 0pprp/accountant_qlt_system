using Application.Common.Interfaces;
using Application.MobileAppReleases.Common;
using Application.SalesRequests.Common;
using Domain.Entities.MobileAppReleaseAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.MobileAppReleases.Commands;

public sealed class PublishMobileAppReleaseCommand : IRequest<Result<MobileAppReleaseDto>>
{
    public required string AppKey { get; init; }
    public required string LatestVersionName { get; init; }
    public int LatestVersionCode { get; init; }
    public int MinimumSupportedVersionCode { get; init; }
    public bool ForceUpdate { get; init; }
    public string ApkUrl { get; init; } = "";
    public string? Sha256 { get; init; }
    public string? ReleaseNotes { get; init; }
    public int? ExpectedVersion { get; init; }
}

public sealed class PublishMobileAppReleaseHandler
    : IRequestHandler<PublishMobileAppReleaseCommand, Result<MobileAppReleaseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public PublishMobileAppReleaseHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<MobileAppReleaseDto>> Handle(
        PublishMobileAppReleaseCommand request,
        CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.MobileAppRelease.Publish) == false)
            return MobileUpdateErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return MobileUpdateErrors.Forbidden;

        var key = MobileAppKeys.NormalizeKnown(request.AppKey);
        if (key is null || MobileAppKeys.All.Contains(key) == false)
            return MobileUpdateErrors.AppKey;

        if (request.ForceUpdate && string.IsNullOrWhiteSpace(request.ApkUrl))
            return MobileUpdateErrors.ApkUrlRequired;
        if (string.IsNullOrWhiteSpace(request.ApkUrl) == false
            && request.ApkUrl.Trim().StartsWith("http", StringComparison.OrdinalIgnoreCase) == false)
            return MobileUpdateErrors.ApkUrlInvalid;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct);
        var at = _clock.UtcNowOffset;

        var existing = await _db.MobileAppReleases.FirstOrDefaultAsync(x => x.AppKey == key, ct);
        try
        {
            if (existing is null)
            {
                existing = new MobileAppRelease(
                    key,
                    request.LatestVersionName,
                    request.LatestVersionCode,
                    request.MinimumSupportedVersionCode,
                    request.ForceUpdate,
                    request.ApkUrl ?? "",
                    request.Sha256,
                    request.ReleaseNotes,
                    actorId,
                    actorName,
                    at);
                _db.MobileAppReleases.Add(existing);
            }
            else
            {
                var expected = request.ExpectedVersion ?? existing.ConcurrencyStamp;
                existing.Publish(
                    request.LatestVersionName,
                    request.LatestVersionCode,
                    request.MinimumSupportedVersionCode,
                    request.ForceUpdate,
                    request.ApkUrl ?? "",
                    request.Sha256,
                    request.ReleaseNotes,
                    actorId,
                    actorName,
                    expected,
                    at);
            }
        }
        catch (ArgumentException)
        {
            return MobileUpdateErrors.Validation;
        }
        catch (InvalidOperationException)
        {
            return MobileUpdateErrors.Concurrency;
        }

        await _db.SaveChangesAsync(ct);
        return MobileAppReleaseMapper.ToDto(existing);
    }
}
