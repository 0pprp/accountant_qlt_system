using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Committee.Commands.App.AddCustomerNote;

public record AddCommitteeCustomerNoteCommand(
    int UserId,
    int CustomerId,
    int ListId,
    string NoteText) : IRequest<Result<FollowerNoteRowDto>>;

public class AddCommitteeCustomerNoteHandler
    : IRequestHandler<AddCommitteeCustomerNoteCommand, Result<FollowerNoteRowDto>>
{
    private readonly FollowerNotesDataService _notes;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public AddCommitteeCustomerNoteHandler(
        FollowerNotesDataService notes,
        IApplicationDbContext db,
        ICurrentUserService current)
    {
        _notes = notes;
        _db = db;
        _current = current;
    }

    public async Task<Result<FollowerNoteRowDto>> Handle(
        AddCommitteeCustomerNoteCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.FollowerWork.NotesWrite) == false)
            return new Error("ليس لديك صلاحية كتابة ملاحظات الزبائن.", "FollowerNotes.WriteDenied");

        var name = await _db.Users.AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        return await _notes.AddNoteForMotabaAsync(
            request.UserId,
            name,
            request.CustomerId,
            request.ListId,
            request.NoteText,
            cancellationToken);
    }
}
