using Application.Common.Content;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.TutorialVideos.Common;
using Domain.Entities.TutorialVideoAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.TutorialVideos.Queries;

public sealed class ListTutorialVideosQuery : IRequest<Result<TutorialVideoListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? AppKey { get; init; }
    public bool? IsEnabled { get; init; }
}

public sealed class ListTutorialVideosHandler
    : IRequestHandler<ListTutorialVideosQuery, Result<TutorialVideoListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListTutorialVideosHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<TutorialVideoListResponse>> Handle(ListTutorialVideosQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Tutorial.Read) == false)
            return TutorialErrors.Forbidden;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var q = _db.TutorialVideos.AsNoTracking().AsQueryable();

        if (string.IsNullOrWhiteSpace(request.AppKey) == false)
        {
            if (TutorialAppKeyLabels.TryParse(request.AppKey, out var key) == false)
                return TutorialErrors.InvalidAppKey;
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

        return new TutorialVideoListResponse(items.Select(TutorialMapper.ToAdmin).ToList(), total, page, pageSize);
    }
}

public sealed class ListTutorialAppsQuery : IRequest<Result<IReadOnlyList<TutorialAppOptionDto>>>;

public sealed class ListTutorialAppsHandler
    : IRequestHandler<ListTutorialAppsQuery, Result<IReadOnlyList<TutorialAppOptionDto>>>
{
    private readonly ICurrentUserService _current;

    public ListTutorialAppsHandler(ICurrentUserService current) => _current = current;

    public Task<Result<IReadOnlyList<TutorialAppOptionDto>>> Handle(ListTutorialAppsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Tutorial.Read) == false
            && _current.HasPermission(SeedPermissions.Tutorial.Create) == false)
            return Task.FromResult<Result<IReadOnlyList<TutorialAppOptionDto>>>(TutorialErrors.Forbidden);

        IReadOnlyList<TutorialAppOptionDto> apps =
        [
            new(TutorialAppKeyLabels.Wire(TutorialAppKey.Delegate), TutorialAppKeyLabels.Arabic(TutorialAppKey.Delegate)),
            new(TutorialAppKeyLabels.Wire(TutorialAppKey.BranchManager), TutorialAppKeyLabels.Arabic(TutorialAppKey.BranchManager)),
            new(TutorialAppKeyLabels.Wire(TutorialAppKey.Follower), TutorialAppKeyLabels.Arabic(TutorialAppKey.Follower)),
            new(TutorialAppKeyLabels.Wire(TutorialAppKey.SalesEmployee), TutorialAppKeyLabels.Arabic(TutorialAppKey.SalesEmployee)),
        ];
        return Task.FromResult(Result<IReadOnlyList<TutorialAppOptionDto>>.Success(apps));
    }
}

public sealed class ListPublicTutorialVideosQuery : IRequest<Result<IReadOnlyList<TutorialPublicDto>>>
{
    public required string AppKey { get; init; }
}

public sealed class ListPublicTutorialVideosHandler
    : IRequestHandler<ListPublicTutorialVideosQuery, Result<IReadOnlyList<TutorialPublicDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ListPublicTutorialVideosHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<TutorialPublicDto>>> Handle(
        ListPublicTutorialVideosQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Tutorial.AppRead) == false)
            return TutorialErrors.Forbidden;
        if (TutorialAppKeyLabels.TryParse(request.AppKey, out var key) == false)
            return TutorialErrors.InvalidAppKey;

        var allowed = await ContentAppAudience.ResolveTutorialAsync(_db, _current, _clock, ct);
        if (allowed is null || ContentAppAudience.AllowsTutorial(allowed.Value, key) == false)
            return TutorialErrors.AudienceDenied;

        var items = await _db.TutorialVideos.AsNoTracking()
            .Where(x => x.AppKey == key && x.IsEnabled && x.VideoUrl != "")
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        return items.Select(TutorialMapper.ToPublic).ToList();
    }
}
