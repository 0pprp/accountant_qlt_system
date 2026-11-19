using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Commands.Admin.Delete;

public class DeleteSafeHandler : IRequestHandler<DeleteSafeCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IActivityLogService _activityLogService;

    public DeleteSafeHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(DeleteSafeCommand request, CancellationToken cancellationToken)
    {
        var safe = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        var scoped = SafeAccess.EnsureBranchInScope(_currentUser, safe.BranchId);
        if (scoped.IsFailed)
            return scoped.Error;

        if (safe.IsSystem)
            return SafeErrors.SystemSafeCannotBeDeleted;

        if (safe.IsEmpty == false)
            return SafeErrors.SafeWithBalanceCannotBeDeleted;

        var hasMovements = await _dbContext.SafeMovements
            .AnyAsync(x => x.SafeId == request.Id || x.CounterpartySafeId == request.Id, cancellationToken);
        if (hasMovements)
            return SafeErrors.SafeWithMovementsCannotBeDeleted;

        var branchId = safe.BranchId;
        _dbContext.Safes.Remove(safe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            branchId, ActivityType.SafeDeleted, TargetEntityType.Safe, request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
