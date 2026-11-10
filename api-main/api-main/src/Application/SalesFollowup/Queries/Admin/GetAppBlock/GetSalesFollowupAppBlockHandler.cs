using Application.Common.Interfaces;
using Application.SalesFollowup.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesFollowup.Queries.Admin.GetAppBlock;

public class GetSalesFollowupAppBlockHandler
    : IRequestHandler<GetSalesFollowupAppBlockQuery, Result<SalesFollowupAppBlockDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetSalesFollowupAppBlockHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesFollowupAppBlockDto>> Handle(
        GetSalesFollowupAppBlockQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var guard = await SalesFollowupEmployeeGuard.EnsureAssignedInBranchAsync(
            _db, request.EmployeeId, request.BranchId, _clock.UtcNowOffset, cancellationToken);
        if (guard is not null)
            return guard;

        var row = await _db.SalesEmployeeAppBlocks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeUserId == request.EmployeeId, cancellationToken);

        return new SalesFollowupAppBlockDto
        {
            EmployeeId = request.EmployeeId,
            IsBlocked = row?.IsBlocked ?? false,
            UpdatedAtUtc = row?.UpdatedAtUtc,
            UpdatedBy = row?.UpdatedByName,
        };
    }
}
