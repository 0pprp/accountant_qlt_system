using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.OrderAggregate;

namespace Application.Tests;

internal sealed class NotificationServiceStub : INotificationService
{
    public Task AddOrderCreatedNotificationAsync(Order order, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public void AddTransactionCreatedNotification(Domain.Entities.SafeAggregate.Transaction transaction, int branchId)
    {
    }
}

internal sealed class ActivityLogServiceStub : IActivityLogService
{
    public Task AddAsync(
        int branchId,
        ActivityType activityType,
        TargetEntityType targetEntityType,
        int? targetEntityId,
        ActivityLogActor? actorOverride = null)
        => Task.CompletedTask;
}
