using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.TutorialVideos.Common;
using Domain.Entities.TutorialVideoAggregate;
using Domain.Entities.TutorialVideoAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.TutorialVideos.Commands;

public sealed class CreateTutorialVideoCommand : IRequest<Result<TutorialVideoDto>>
{
    public required string AppKey { get; init; }
    public required string QuestionAr { get; init; }
    public required string VideoUrl { get; init; }
    public bool IsEnabled { get; init; } = true;
}

public sealed class UpdateTutorialVideoCommand : IRequest<Result<TutorialVideoDto>>
{
    public int Id { get; init; }
    public required string QuestionAr { get; init; }
    public required string VideoUrl { get; init; }
    public bool IsEnabled { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class DeleteTutorialVideoCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class CreateTutorialVideoHandler
    : IRequestHandler<CreateTutorialVideoCommand, Result<TutorialVideoDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public CreateTutorialVideoHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<TutorialVideoDto>> Handle(CreateTutorialVideoCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Tutorial.Create) == false)
            return TutorialErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return TutorialErrors.Forbidden;
        if (TutorialAppKeyLabels.TryParse(request.AppKey, out var appKey) == false)
            return TutorialErrors.InvalidAppKey;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var nextSort = (await _db.TutorialVideos.AsNoTracking()
            .Where(x => x.AppKey == appKey).Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? 0) + 1;

        TutorialVideo entity;
        try
        {
            entity = new TutorialVideo(
                appKey, request.QuestionAr, request.VideoUrl, request.IsEnabled, nextSort,
                actorId, actorName, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException)
        {
            return TutorialErrors.Validation;
        }

        _db.TutorialVideos.Add(entity);
        await _db.SaveChangesAsync(ct);
        return TutorialMapper.ToAdmin(entity);
    }
}

public sealed class UpdateTutorialVideoHandler
    : IRequestHandler<UpdateTutorialVideoCommand, Result<TutorialVideoDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public UpdateTutorialVideoHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<TutorialVideoDto>> Handle(UpdateTutorialVideoCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Tutorial.Update) == false)
            return TutorialErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return TutorialErrors.Forbidden;

        var entity = await _db.TutorialVideos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return TutorialErrors.NotFound;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        try
        {
            entity.Update(request.QuestionAr, request.VideoUrl, request.IsEnabled,
                actorId, actorName, request.ExpectedVersion, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return TutorialErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return TutorialErrors.Validation;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TutorialErrors.Concurrency;
        }

        return TutorialMapper.ToAdmin(entity);
    }
}

public sealed class DeleteTutorialVideoHandler : IRequestHandler<DeleteTutorialVideoCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public DeleteTutorialVideoHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result> Handle(DeleteTutorialVideoCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Tutorial.Delete) == false)
            return TutorialErrors.Forbidden;

        var entity = await _db.TutorialVideos.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return TutorialErrors.NotFound;
        if (entity.MatchesExpectedVersion(request.ExpectedVersion) == false)
            return TutorialErrors.Concurrency;

        _db.TutorialVideos.Remove(entity);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TutorialErrors.Concurrency;
        }

        return Result.Success();
    }
}
