using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SafeEntity = Domain.Entities.SafeAggregate.Safe;

namespace Application.Safes.Commands.Admin.Create;

public class CreateSafeHandler : IRequestHandler<CreateSafeCommand, Result<CreateSafeResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IActivityLogService _activityLogService;

    public CreateSafeHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _activityLogService = activityLogService;
    }

    public async Task<Result<CreateSafeResponse>> Handle(CreateSafeCommand request, CancellationToken cancellationToken)
    {
        var scoped = SafeAccess.EnsureBranchInScope(_currentUser, request.BranchId);
        if (scoped.IsFailed)
            return scoped.Error;

        if (request.OrderListId is null && request.UserId is null)
            return SafeErrors.SafeBindingIsRequired;

        if (request.OrderListId is not null && request.UserId is not null)
            return SafeErrors.SafeBindingMustBeSingle;

        SafeEntity safe;

        if (request.OrderListId is int orderListId)
        {
            var list = await _dbContext.OrderLists.AsNoTracking()
                .Where(x => x.Id == orderListId && x.BranchId == request.BranchId)
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (list is null)
                return SafeErrors.OrderListNotFoundInBranch;

            var exists = await _dbContext.Safes
                .AnyAsync(x => x.OrderListId == orderListId, cancellationToken);
            if (exists)
                return SafeErrors.SafeForThisBindingAlreadyExists;

            safe = SafeEntity.ForOrderList(list.Name, request.BranchId, orderListId);
        }
        else
        {
            var userId = request.UserId!.Value;

            var user = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == userId && x.UserBranches.Any(b => b.BranchId == request.BranchId))
                .Select(x => new { x.Id, x.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                return SafeErrors.UserNotFoundInBranch;

            var exists = await _dbContext.Safes
                .AnyAsync(x => x.BranchId == request.BranchId && x.UserId == userId, cancellationToken);
            if (exists)
                return SafeErrors.SafeForThisBindingAlreadyExists;

            safe = SafeEntity.ForEmployee(user.FullName, request.BranchId, userId);
        }

        _dbContext.Safes.Add(safe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            request.BranchId, ActivityType.SafeCreated, TargetEntityType.Safe, safe.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateSafeResponse(safe.Id);
    }
}
