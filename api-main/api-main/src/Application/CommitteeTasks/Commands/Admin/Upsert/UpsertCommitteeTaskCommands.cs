using Application.Common.Interfaces;
using Application.CommitteeTasks.Common;
using Application.SalesRequests.Common;
using Domain.Entities.CommitteeTaskAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.CommitteeTasks.Commands.Admin.Upsert;

public sealed class CreateCommitteeTaskCommand : IRequest<Result<CommitteeTaskRowDto>>
{
    public required string Body { get; init; }
    public bool IsEnabled { get; init; } = true;
}

public sealed class UpdateCommitteeTaskCommand : IRequest<Result<CommitteeTaskRowDto>>
{
    public int Id { get; init; }
    public required string Body { get; init; }
    public bool IsEnabled { get; init; } = true;
    public int ExpectedVersion { get; init; }
}

public sealed class DeleteCommitteeTaskCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

file static class CommitteeTaskBody
{
    public const int Max = 8000;

    public static Result<string> Validate(string? body)
    {
        var text = (body ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text))
            return CommitteeTaskErrors.BodyRequired;
        if (text.Length > Max)
            return CommitteeTaskErrors.BodyTooLong;
        return text;
    }
}

public sealed class CreateCommitteeTaskHandler
    : IRequestHandler<CreateCommitteeTaskCommand, Result<CommitteeTaskRowDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public CreateCommitteeTaskHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<CommitteeTaskRowDto>> Handle(CreateCommitteeTaskCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CommitteeTask.Write) == false)
            return CommitteeTaskErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return CommitteeTaskErrors.Forbidden;

        var bodyResult = CommitteeTaskBody.Validate(request.Body);
        if (bodyResult.IsFailed)
            return bodyResult.Error!;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var maxSort = await _db.CommitteeTaskItems.AsNoTracking()
            .Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? -1;
        var entity = new CommitteeTaskItem(
            bodyResult.Data!,
            request.IsEnabled,
            maxSort + 1,
            actorId,
            actorName,
            _clock.UtcNowOffset);
        _db.CommitteeTaskItems.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public static CommitteeTaskRowDto Map(CommitteeTaskItem e) => new(
        e.Id, e.Body, e.IsEnabled, e.SortOrder, e.CreatedAtUtc, e.UpdatedAtUtc, e.UpdatedByName, e.ConcurrencyStamp);
}

public sealed class UpdateCommitteeTaskHandler
    : IRequestHandler<UpdateCommitteeTaskCommand, Result<CommitteeTaskRowDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public UpdateCommitteeTaskHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<CommitteeTaskRowDto>> Handle(UpdateCommitteeTaskCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CommitteeTask.Write) == false)
            return CommitteeTaskErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return CommitteeTaskErrors.Forbidden;

        var bodyResult = CommitteeTaskBody.Validate(request.Body);
        if (bodyResult.IsFailed)
            return bodyResult.Error!;

        var entity = await _db.CommitteeTaskItems.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return CommitteeTaskErrors.NotFound;
        if (entity.MatchesExpectedVersion(request.ExpectedVersion) == false)
            return CommitteeTaskErrors.Concurrency;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        entity.Update(bodyResult.Data!, request.IsEnabled, actorId, actorName, _clock.UtcNowOffset);
        await _db.SaveChangesAsync(ct);
        return CreateCommitteeTaskHandler.Map(entity);
    }
}

public sealed class DeleteCommitteeTaskHandler : IRequestHandler<DeleteCommitteeTaskCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public DeleteCommitteeTaskHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result> Handle(DeleteCommitteeTaskCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CommitteeTask.Write) == false)
            return CommitteeTaskErrors.Forbidden;

        var entity = await _db.CommitteeTaskItems.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return CommitteeTaskErrors.NotFound;
        if (entity.MatchesExpectedVersion(request.ExpectedVersion) == false)
            return CommitteeTaskErrors.Concurrency;

        _db.CommitteeTaskItems.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
