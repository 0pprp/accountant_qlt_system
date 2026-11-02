using Application.Common.Interfaces;
using Application.Committee.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetMe;

public class GetCommitteeMeHandler : IRequestHandler<GetCommitteeMeQuery, Result<GetCommitteeMeResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetCommitteeMeHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<GetCommitteeMeResponse>> Handle(GetCommitteeMeQuery request, CancellationToken cancellationToken)
    {
        if (!CommitteeAccess.IsCommitteeOfficerRole(request.Role))
            return CommitteeErrors.NotCommitteeOfficer;

        var user = await _db.Users.AsNoTracking()
            .Where(x => x.Id == request.UserId)
            .Select(x => new { x.Id, x.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return CommitteeErrors.NotCommitteeOfficer;

        return new GetCommitteeMeResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = request.Role ?? "Motaba",
            ProductRoleLabel = "مسؤول اللجنة",
            IsCommitteeOfficer = true,
            BranchIds = _current.BranchIds?.ToList() ?? []
        };
    }
}
