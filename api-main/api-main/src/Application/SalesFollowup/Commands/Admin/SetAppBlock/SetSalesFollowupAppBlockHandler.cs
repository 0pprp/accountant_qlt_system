using Application.Common.Interfaces;
using Application.SalesFollowup.Common;
using Domain.Entities.SalesFollowupAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesFollowup.Commands.Admin.SetAppBlock;

public class SetSalesFollowupAppBlockHandler
    : IRequestHandler<SetSalesFollowupAppBlockCommand, Result<SalesFollowupAppBlockDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SetSalesFollowupAppBlockHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesFollowupAppBlockDto>> Handle(
        SetSalesFollowupAppBlockCommand request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var now = _clock.UtcNowOffset;
        var guard = await SalesFollowupEmployeeGuard.EnsureAssignedInBranchAsync(
            _db, request.EmployeeId, request.BranchId, now, cancellationToken);
        if (guard is not null)
            return guard;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == _currentUser.UserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var row = await _db.SalesEmployeeAppBlocks
            .FirstOrDefaultAsync(x => x.EmployeeUserId == request.EmployeeId, cancellationToken);

        if (row is null)
        {
            row = new SalesEmployeeAppBlock(
                request.EmployeeId,
                request.IsBlocked,
                now,
                _currentUser.UserId,
                actorName);
            _db.SalesEmployeeAppBlocks.Add(row);
        }
        else if (row.IsBlocked == request.IsBlocked)
        {
            // لا تكرار أثر عند حفظ نفس الحالة
            return new SalesFollowupAppBlockDto
            {
                EmployeeId = request.EmployeeId,
                IsBlocked = row.IsBlocked,
                UpdatedAtUtc = row.UpdatedAtUtc,
                UpdatedBy = row.UpdatedByName,
            };
        }
        else
        {
            row.SetBlocked(request.IsBlocked, now, _currentUser.UserId, actorName);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new SalesFollowupAppBlockDto
        {
            EmployeeId = request.EmployeeId,
            IsBlocked = row.IsBlocked,
            UpdatedAtUtc = row.UpdatedAtUtc,
            UpdatedBy = row.UpdatedByName,
        };
    }
}
