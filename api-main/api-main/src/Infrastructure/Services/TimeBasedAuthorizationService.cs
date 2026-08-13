using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TimeBasedAuthorizationService : ITimeBasedAuthorizationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TimeBasedAuthorizationService(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> CanPerformActionWithinTimeLimit<T>(
        int entityId,
        string restrictedRole,
        TimeSpan timeLimit,
        CancellationToken cancellationToken = default) where T : AuditableEntity
    {
        if (_currentUserService.Role != restrictedRole)
            return true;

        var entity = await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);

        if (entity is null)
            return false;

        return CanPerformActionWithinTimeLimit(entity, restrictedRole, timeLimit);
    }

    public bool CanPerformActionWithinTimeLimit<T>(
        T entity,
        string restrictedRole,
        TimeSpan timeLimit) where T : AuditableEntity
    {
        if (_currentUserService.Role != restrictedRole)
            return true;

        var hoursSinceCreation = _dateTimeProvider.UtcNow - entity.CreatedAt!.Value;
        return hoursSinceCreation <= timeLimit;
    }
}