using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesEmployees.Commands.Admin.EndAssignment;

public class EndSalesEmployeeAssignmentHandler
    : IRequestHandler<EndSalesEmployeeAssignmentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public EndSalesEmployeeAssignmentHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(
        EndSalesEmployeeAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var row = await _db.SalesEmployeeAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);
        if (row is null)
            return SalesEmployeeErrors.NotFound;
        if (!SalesEmployeeAccess.CanAccessBranch(_currentUser, row.BranchId))
            return SalesEmployeeErrors.BranchForbidden;

        row.End(_clock.UtcNowOffset);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
