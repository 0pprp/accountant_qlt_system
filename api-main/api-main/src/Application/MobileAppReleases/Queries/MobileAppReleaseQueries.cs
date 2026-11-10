using Application.Common.Interfaces;
using Application.MobileAppReleases.Common;
using Domain.Entities.MobileAppReleaseAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.MobileAppReleases.Queries;

public sealed class ListMobileAppReleasesQuery : IRequest<Result<IReadOnlyList<MobileAppReleaseDto>>>
{
}

public sealed class ListMobileAppReleasesHandler
    : IRequestHandler<ListMobileAppReleasesQuery, Result<IReadOnlyList<MobileAppReleaseDto>>>
{
    private readonly IApplicationDbContext _db;

    public ListMobileAppReleasesHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<MobileAppReleaseDto>>> Handle(
        ListMobileAppReleasesQuery request,
        CancellationToken ct)
    {
        var rows = await _db.MobileAppReleases.AsNoTracking()
            .OrderBy(x => x.AppKey)
            .ToListAsync(ct);
        return rows.Select(MobileAppReleaseMapper.ToDto).ToList();
    }
}

public sealed class EvaluateMobileUpdateQuery : IRequest<Result<MobileUpdateCheckDto>>
{
    public required string AppKey { get; init; }
    public int CurrentVersionCode { get; init; }
}

public sealed class EvaluateMobileUpdateHandler
    : IRequestHandler<EvaluateMobileUpdateQuery, Result<MobileUpdateCheckDto>>
{
    private readonly IApplicationDbContext _db;

    public EvaluateMobileUpdateHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<MobileUpdateCheckDto>> Handle(
        EvaluateMobileUpdateQuery request,
        CancellationToken ct)
    {
        var key = MobileAppKeys.NormalizeKnown(request.AppKey)
                  ?? (string.IsNullOrWhiteSpace(request.AppKey)
                      ? null
                      : request.AppKey.Trim().ToLowerInvariant());
        if (key is null)
            return MobileUpdateErrors.UnknownApp;

        var row = await _db.MobileAppReleases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AppKey == key, ct);
        if (row is null)
            return MobileUpdateErrors.UnknownApp;

        return MobileAppReleaseMapper.ToCheck(row, request.CurrentVersionCode);
    }
}
