using System.Security.Claims;
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

internal sealed class DateTimeProviderStub : IDateTimeProvider
{
    public DateTime UtcNow => new(2026, 8, 19, 8, 10, 0, DateTimeKind.Utc);
    public DateOnly Today => new(2026, 8, 19);
}

internal sealed class CurrentUserServiceStub : ICurrentUserService
{
    public int? UserId { get; set; } = 1;
    public ClaimsPrincipal? User { get; set; }
    public string? Role { get; set; } = "BranchAccountant";
    public List<int>? BranchIds { get; set; } = [];
}
