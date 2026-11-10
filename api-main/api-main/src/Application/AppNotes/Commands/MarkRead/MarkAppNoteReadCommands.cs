using Application.AppNotes.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.AppNoteAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.AppNotes.Commands.MarkRead;

public sealed class MarkAppNoteReadCommand : IRequest<Result<AppNoteDetailDto>>
{
    public int Id { get; init; }
}

public sealed class MarkAllAppNotesReadCommand : IRequest<Result<int>>;

public sealed class MarkAppNoteReadHandler : IRequestHandler<MarkAppNoteReadCommand, Result<AppNoteDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkAppNoteReadHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<AppNoteDetailDto>> Handle(MarkAppNoteReadCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.AppNote.Read) == false)
            return AppNoteErrors.Forbidden;

        var note = await _db.AppNotes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (note is null)
            return AppNoteErrors.NotFound;

        note.MarkRead(_clock.UtcNowOffset);
        await _db.SaveChangesAsync(ct);
        return AppNoteMapper.ToDetail(note);
    }
}

public sealed class MarkAllAppNotesReadHandler : IRequestHandler<MarkAllAppNotesReadCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkAllAppNotesReadHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(MarkAllAppNotesReadCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.AppNote.Read) == false)
            return AppNoteErrors.Forbidden;

        var unread = await _db.AppNotes
            .Where(x => x.Status == AppNoteStatus.Unread)
            .ToListAsync(ct);
        var at = _clock.UtcNowOffset;
        var changed = 0;
        foreach (var note in unread)
        {
            if (note.MarkRead(at))
                changed++;
        }

        if (changed > 0)
            await _db.SaveChangesAsync(ct);
        return changed;
    }
}
