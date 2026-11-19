using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.Safes.Commands.Admin.Transfer;

public class TransferBetweenSafesHandler : IRequestHandler<TransferBetweenSafesCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ISafeLedger _safeLedger;
    private readonly IActivityLogService _activityLogService;

    public TransferBetweenSafesHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser,
        ISafeLedger safeLedger, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _safeLedger = safeLedger;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(TransferBetweenSafesCommand request, CancellationToken cancellationToken)
    {
        var source = await SafeAccess.EnsureSafeInScopeAsync(
            _dbContext, _currentUser, request.SourceSafeId, cancellationToken);
        if (source.IsFailed)
            return source.Error;

        var destination = await SafeAccess.EnsureSafeInScopeAsync(
            _dbContext, _currentUser, request.DestinationSafeId, cancellationToken);
        if (destination.IsFailed)
            return destination.Error;

        var movement = await _safeLedger.TransferAsync(
            request.SourceSafeId, request.DestinationSafeId, request.Amount, request.Note,
            request.OperationKey, cancellationToken);

        if (movement.IsFailed)
            return movement.Error;

        await _activityLogService.AddAsync(
            source.Data, ActivityType.SafeTransfer, TargetEntityType.Safe, request.SourceSafeId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
