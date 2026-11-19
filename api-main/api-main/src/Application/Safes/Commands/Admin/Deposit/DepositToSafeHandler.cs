using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.Safes.Commands.Admin.Deposit;

public class DepositToSafeHandler : IRequestHandler<DepositToSafeCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ISafeLedger _safeLedger;
    private readonly IActivityLogService _activityLogService;

    public DepositToSafeHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser,
        ISafeLedger safeLedger, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _safeLedger = safeLedger;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(DepositToSafeCommand request, CancellationToken cancellationToken)
    {
        var scope = await SafeAccess.EnsureSafeInScopeAsync(_dbContext, _currentUser, request.SafeId, cancellationToken);
        if (scope.IsFailed)
            return scope.Error;

        var movement = await _safeLedger.DepositAsync(
            request.SafeId, request.Amount, request.Note, request.OperationKey, cancellationToken);

        if (movement.IsFailed)
            return movement.Error;

        await _activityLogService.AddAsync(
            scope.Data, ActivityType.SafeDeposit, TargetEntityType.Safe, request.SafeId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
