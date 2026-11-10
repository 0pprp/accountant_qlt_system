using Application.AppNotes.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.AppNoteAggregate;
using Domain.Entities.AppNoteAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.AppNotes.Commands.Create;

public sealed class CreateAppNoteCommand : IRequest<Result<AppNoteDetailDto>>
{
    public required string Body { get; init; }
    public string? Subject { get; init; }
    /// <summary>
    /// اختياري للاختبارات فقط. المسار الحي يتجاهله ويحلّ المصدر من الجلسة+التكليف.
    /// </summary>
    public AppNoteSourceApp? SourceAppOverride { get; init; }
}

public sealed class CreateAppNoteHandler : IRequestHandler<CreateAppNoteCommand, Result<AppNoteDetailDto>>
{
    public const int MinBody = 10;
    public const int MaxBody = 2000;
    public const int MaxSubject = 256;
    public const string DefaultSubject = "ملاحظة إلى المدير";

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public CreateAppNoteHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<AppNoteDetailDto>> Handle(CreateAppNoteCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.AppNote.Create) == false)
            return AppNoteErrors.Forbidden;
        if (_current.UserId is not int senderId)
            return AppNoteErrors.NoActor;

        var source = request.SourceAppOverride
            ?? await AppNoteSourceResolver.ResolveAsync(_db, _current, _clock, ct);
        if (source is null)
            return AppNoteErrors.Forbidden;

        var body = (request.Body ?? "").Trim().Replace('<', ' ').Replace('>', ' ');
        if (body.Length < MinBody)
            return AppNoteErrors.BodyShort;
        if (body.Length > MaxBody)
            return AppNoteErrors.BodyLong;

        var subject = string.IsNullOrWhiteSpace(request.Subject) ? DefaultSubject : request.Subject.Trim();
        if (subject.Length > MaxSubject)
            subject = subject[..MaxSubject];

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == senderId)
            .Select(u => new { u.FullName })
            .FirstOrDefaultAsync(ct);
        if (user is null)
            return AppNoteErrors.NoActor;

        int? branchId = _current.BranchIds?.FirstOrDefault();
        if (branchId is <= 0)
            branchId = null;
        string? branchName = null;
        if (branchId is int bid)
        {
            branchName = await _db.Branches.AsNoTracking()
                .Where(b => b.Id == bid).Select(b => b.Name).FirstOrDefaultAsync(ct);
        }

        var role = string.IsNullOrWhiteSpace(_current.Role) ? source.Value.ToString() : _current.Role!;
        var note = new AppNote(
            source.Value,
            "manager_note",
            senderId,
            user.FullName ?? $"#{senderId}",
            role,
            branchId,
            branchName,
            subject,
            body,
            _clock.UtcNowOffset);

        _db.AppNotes.Add(note);
        await _db.SaveChangesAsync(ct);
        return AppNoteMapper.ToDetail(note);
    }
}
