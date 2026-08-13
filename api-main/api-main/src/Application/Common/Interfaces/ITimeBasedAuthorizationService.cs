using Domain.Common;

namespace Application.Common.Interfaces;

public interface ITimeBasedAuthorizationService
{
    Task<bool> CanPerformActionWithinTimeLimit<T>(
        int entityId, 
        string restrictedRole, 
        TimeSpan timeLimit, 
        CancellationToken cancellationToken = default) where T : AuditableEntity;

    bool CanPerformActionWithinTimeLimit<T>(
        T entity,
        string restrictedRole,
        TimeSpan timeLimit) where T : AuditableEntity;
}