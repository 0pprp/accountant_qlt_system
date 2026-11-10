using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Domain.Entities.SalesEmployeeAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesEmployees.Commands.Admin.GrantAssignment;

public class GrantSalesEmployeeAssignmentHandler
    : IRequestHandler<GrantSalesEmployeeAssignmentCommand, Result<GrantSalesEmployeeAssignmentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GrantSalesEmployeeAssignmentHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<GrantSalesEmployeeAssignmentResponse>> Handle(
        GrantSalesEmployeeAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var branchErr = SalesEmployeeAccess.ValidateBranchOrAll(_currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr;
        if (!SalesEmployeeAccess.CanAccessBranch(_currentUser, request.BranchId))
            return SalesEmployeeErrors.BranchForbidden;

        var userOk = await _db.Users.AsNoTracking()
            .AnyAsync(
                u => u.Id == request.UserId && u.CreationStep == UserCreationStep.Completed,
                cancellationToken);
        if (!userOk)
            return SalesEmployeeErrors.NotFound;

        var now = _clock.UtcNowOffset;
        var existing = await SalesWorkforce.HasActiveAssignmentAsync(
            _db, request.UserId, request.BranchId, now, cancellationToken);
        if (existing)
            return new Error("يوجد تكليف ساري بالفعل على هذا الفرع.", "SalesEmployee.AssignmentExists");

        var row = new SalesEmployeeAssignment(
            request.UserId,
            request.BranchId,
            now,
            _currentUser.UserId,
            request.Note);
        _db.SalesEmployeeAssignments.Add(row);
        await _db.SaveChangesAsync(cancellationToken);

        return new GrantSalesEmployeeAssignmentResponse
        {
            AssignmentId = row.Id,
            UserId = row.UserId,
            BranchId = row.BranchId,
            StartsAtUtc = row.StartsAtUtc,
        };
    }
}
