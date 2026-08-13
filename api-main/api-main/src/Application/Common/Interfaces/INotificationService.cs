using Domain.Entities.OrderAggregate;
using Domain.Entities.SafeAggregate;

namespace Application.Common.Interfaces;

public interface INotificationService
{
    Task AddOrderCreatedNotificationAsync(Order order, CancellationToken cancellationToken);
    void AddTransactionCreatedNotification(Transaction transaction, int branchId);
}
