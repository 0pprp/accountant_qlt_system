using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Notifications.Common;
using Domain.Entities.NotificationAggregate;
using Domain.Entities.NotificationAggregate.Enums;
using Domain.Entities.OrderAggregate;
using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public NotificationService(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task AddOrderCreatedNotificationAsync(Order order, CancellationToken cancellationToken)
    {
        var customerFullName = await _dbContext.Customers.AsNoTracking()
            .Where(x => x.Id == order.CustomerId)
            .Select(x => x.FullName)
            .FirstAsync(cancellationToken);

        var notification = new Notification(
            NotificationTexts.OrderCreated.Title,
            NotificationTexts.OrderCreated.Description(customerFullName, order.SellAmount),
            NotificationActionType.OrderCreated,
            order.BranchId,
            _currentUserService.UserId!.Value);

        _dbContext.Notifications.Add(notification);
    }

    public void AddTransactionCreatedNotification(Transaction transaction, int branchId)
    {
        var notification = new Notification(
            NotificationTexts.TransactionCreated.Title,
            NotificationTexts.TransactionCreated.Description(
                transaction.Type.ToArabicString(),
                transaction.Direction.ToArabicString(),
                transaction.Amount),
            NotificationActionType.TransactionCreated,
            branchId,
            _currentUserService.UserId!.Value);

        _dbContext.Notifications.Add(notification);
    }
}
