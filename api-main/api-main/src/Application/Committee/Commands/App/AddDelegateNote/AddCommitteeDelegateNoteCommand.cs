using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Committee.Commands.App.AddDelegateNote;

public record AddCommitteeDelegateNoteCommand(
    int UserId,
    int ListId,
    string NoteText) : IRequest<Result<FollowerDelegateNoteRowDto>>;

public class AddCommitteeDelegateNoteHandler
    : IRequestHandler<AddCommitteeDelegateNoteCommand, Result<FollowerDelegateNoteRowDto>>
{
    private readonly FollowerDelegateNotesDataService _notes;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public AddCommitteeDelegateNoteHandler(
        FollowerDelegateNotesDataService notes,
        IApplicationDbContext db,
        ICurrentUserService current)
    {
        _notes = notes;
        _db = db;
        _current = current;
    }

    public async Task<Result<FollowerDelegateNoteRowDto>> Handle(
        AddCommitteeDelegateNoteCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.FollowerWork.DelegateNotesWrite) == false)
            return FollowerWorkErrors.DelegateNotesWriteDenied;

        // الكاتب من الجلسة — لا نقبل انتحال CreatedBy/Role من العميل
        var name = await _db.Users.AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        return await _notes.AddForMotabaAsync(
            request.UserId,
            name,
            request.ListId,
            request.NoteText,
            cancellationToken);
    }
}
