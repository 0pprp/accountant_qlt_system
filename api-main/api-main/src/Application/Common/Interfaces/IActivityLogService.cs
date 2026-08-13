using Application.Common.Models;
using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.Common.Interfaces;

public interface IActivityLogService
{
    Task AddAsync(
        int branchId,
        ActivityType activityType,
        TargetEntityType targetEntityType,
        int? targetEntityId,
        ActivityLogActor? actorOverride = null);
}
