using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands.Admin.Update;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public UpdateOrderHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role == Application.Common.SeedData.Roles.MainAccountant.Name)
        {
            var twoDaysAgo = DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(-2);
            if (request.SaleDate is not null && request.SaleDate < twoDaysAgo)
                return OrderErrors.YouCanNotUpdateOrderToMoreThanTwoDaysAgo;
        }

        var order = await _dbContext.Orders
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Product)
            .Include(x => x.InstallmentPayments)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        var completedOrderFieldsValidation = ValidateRequiredFieldsForCompletedOrder(request, order.Step);
        if (completedOrderFieldsValidation.IsFailed)
            return completedOrderFieldsValidation.Error!;

        if (order.InstallmentPayments.Count > 1)
            return OrderErrors.SomeInstallmentsOfThisOrderHaveBeenCollected;

        ApplyIncomingChangesToExistingOrderItems(order, request);
        AppendNewOrderItems(order, request);

        var amounts = ComputeAggregatedItemsAmounts(order);

        if (order.ExecutionStatus == OrderExecutionStatus.InProgress)
        {
            await SynchronizeForeignProductPurchaseAsync(order, cancellationToken);
            
            ApplySaleDateToFirstInstallment(order, request.SaleDate!.Value);
            
            await SynchronizeFirstInstallmentWithPrepaymentAsync(order, request, amounts.Prepayment, cancellationToken);
        }

        order.Update(
            amounts.Buy,
            amounts.Sell,
            amounts.Prepayment,
            amounts.Daily,
            null,
            request.CreationAddress,
            request.SaleDate,
            request.SaleTime,
            request.SellerId,
            request.OrderListId);

        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.OrderUpdated,
            TargetEntityType.Order,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Result ValidateRequiredFieldsForCompletedOrder(UpdateOrderCommand request, OrderStep orderStep)
    {
        if (orderStep != OrderStep.Completed)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(request.CreationAddress))
            return OrderErrors.CreationAddressIsRequired;

        if (request.SaleDate is null)
            return OrderErrors.SaleDateIsRequired;

        if (request.SaleTime is null)
            return OrderErrors.SaleTimeIsRequired;

        if (request.OrderListId is null)
            return OrderErrors.OrderListIdIsRequired;

        return Result.Success();
    }

    private static void ApplyIncomingChangesToExistingOrderItems(Order order, UpdateOrderCommand request)
    {
        foreach (var existing in order.OrderItems)
        {
            var incoming = request.OrderItems.FirstOrDefault(x => x.Id == existing.Id);
            if (incoming is null)
            {
                DetachItemFromOrder(order, existing);
                RestoreProductStock(existing);
            }
            else
            {
                ApplyWarehouseInventoryDeltaForItemUpdate(existing, incoming);
                CopyIncomingValuesOntoItem(existing, incoming);
            }
        }
    }

    private static void AppendNewOrderItems(Order order, UpdateOrderCommand request)
    {
        var newLines = request.OrderItems
            .Where(x => x.Id is null)
            .Select(x => new OrderItem(
                x.ProductType,
                x.Quantity,
                x.BuyAmount,
                x.SellAmount,
                x.PrepaymentAmount,
                x.DailyInstallmentAmount,
                x.ProductName,
                x.ProductId))
            .ToList();

        foreach (var line in newLines)
            order.OrderItems.Add(line);
    }

    private async Task SynchronizeForeignProductPurchaseAsync(Order order, CancellationToken cancellationToken)
    {
        var foreignItems = order.OrderItems
            .Where(x => x.ProductType == ProductType.Foreign)
            .ToList();

        var newTotalAmount = foreignItems.Sum(x => x.BuyAmount);

        var existingPurchase = await _dbContext.Purchases
            .Include(x => x.PurchaseItems)
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);

        if (existingPurchase is null && newTotalAmount == 0)
            return;

        var branchSafe = await _dbContext.Safes.FirstAsync(x => x.BranchId == order.BranchId, cancellationToken);

        if (existingPurchase is null)
        {
            var lastFactorNumber = await _dbContext.Purchases.MaxAsync(x => x.FactorNumber, cancellationToken);

            var transaction = new Transaction(
                newTotalAmount,
                TransactionType.Purchase,
                TransactionStatus.Approved,
                TransactionDirection.Out,
                branchSafe.Id);

            var purchase = new Purchase(
                lastFactorNumber + 1,
                newTotalAmount,
                SafeType.Branch,
                order.BranchId,
                transaction,
                foreignItems
                    .Select(x => new PurchaseItem(x.Quantity, x.BuyAmount, x.ProductName))
                    .ToList())
            {
                OrderId = order.Id
            };

            _dbContext.Purchases.Add(purchase);
            branchSafe.RemainingCashAmount -= newTotalAmount;

            _notificationService.AddTransactionCreatedNotification(transaction, order.BranchId);
            return;
        }

        if (newTotalAmount == 0)
        {
            branchSafe.RemainingCashAmount += existingPurchase.TotalAmount;
            _dbContext.Purchases.Remove(existingPurchase);
            return;
        }

        var oldTotalAmount = existingPurchase.TotalAmount;
        var updatedPurchaseItems = foreignItems
            .Select(x => new PurchaseItem(x.Quantity, x.BuyAmount, x.ProductName))
            .ToList();

        existingPurchase.UpdateForOrder(newTotalAmount, updatedPurchaseItems);

        if (newTotalAmount == oldTotalAmount)
            return;

        var safeId = existingPurchase.Transactions.First().SafeId;

        if (newTotalAmount > oldTotalAmount)
        {
            var additionalAmount = newTotalAmount - oldTotalAmount;

            var transaction = new Transaction(
                additionalAmount,
                TransactionType.Purchase,
                TransactionStatus.Approved,
                TransactionDirection.Out,
                safeId);

            existingPurchase.Transactions.Add(transaction);
            branchSafe.RemainingCashAmount -= additionalAmount;

            _notificationService.AddTransactionCreatedNotification(transaction, order.BranchId);
        }
        else
        {
            var refundAmount = oldTotalAmount - newTotalAmount;

            var transaction = new Transaction(
                refundAmount,
                TransactionType.Purchase,
                TransactionStatus.Approved,
                TransactionDirection.In,
                safeId);

            existingPurchase.Transactions.Add(transaction);
            branchSafe.RemainingCashAmount += refundAmount;

            _notificationService.AddTransactionCreatedNotification(transaction, order.BranchId);
        }
    }

    private static (double Buy, double Sell, double Prepayment, double Daily) ComputeAggregatedItemsAmounts(Order order)
    {
        var buy = order.OrderItems.Sum(x => x.BuyAmount);
        var sell = order.OrderItems.Sum(x => x.SellAmount);
        var prepayment = order.OrderItems.Sum(x => x.PrepaymentAmount);
        var daily = order.OrderItems.Sum(x => x.DailyInstallmentAmount);
        return (buy, sell, prepayment, daily);
    }

    private static void ApplySaleDateToFirstInstallment(Order order, DateOnly saleDate)
    {
        var firstInstallment = order.InstallmentPayments.FirstOrDefault();
        if (firstInstallment is not null)
            firstInstallment.Date = saleDate;
    }

    private async Task SynchronizeFirstInstallmentWithPrepaymentAsync(
        Order order,
        UpdateOrderCommand request,
        double newPrepayment,
        CancellationToken cancellationToken)
    {
        if (order.PrepaymentAmount == newPrepayment)
            return;

        var firstInstallment = order.InstallmentPayments.FirstOrDefault();
        var previousInstallmentAmount = firstInstallment?.Amount ?? 0;

        var seller = await _dbContext.Users.FirstAsync(x => x.Id == request.SellerId, cancellationToken);

        if (firstInstallment is null)
        {
            firstInstallment = new InstallmentPayment(newPrepayment, request.SaleDate!.Value);
            order.InstallmentPayments.Add(firstInstallment);

            seller.UndeliveredCashAmount += firstInstallment.Amount;
        }
        else
        {
            seller.UndeliveredCashAmount -= previousInstallmentAmount;
            seller.UndeliveredCashAmount += newPrepayment;

            if (newPrepayment == 0)
            {
                order.InstallmentPayments.Remove(firstInstallment);
                return;
            }

            firstInstallment.Amount = newPrepayment;
        }
    }

    private static void DetachItemFromOrder(Order order, OrderItem item)
    {
        order.OrderItems.Remove(item);
    }

    private static void RestoreProductStock(OrderItem item)
    {
        if (item.ProductType != ProductType.Warehouse)
            return;

        item.Product!.RemainingCount += item.Quantity;
    }

    private static void ApplyWarehouseInventoryDeltaForItemUpdate(OrderItem item, OrderItemDto incoming)
    {
        if (item.ProductType != ProductType.Warehouse)
            return;

        if (incoming.Quantity > item.Quantity)
        {
            var additionalSold = incoming.Quantity - item.Quantity;
            item.Product!.RemainingCount -= additionalSold;
        }
        else if (incoming.Quantity < item.Quantity)
        {
            var returnedToStock = item.Quantity - incoming.Quantity;
            item.Product!.RemainingCount += returnedToStock;
        }
    }

    private static void CopyIncomingValuesOntoItem(OrderItem item, OrderItemDto incoming)
    {
        item.ProductType = incoming.ProductType;
        item.Quantity = incoming.Quantity;
        item.BuyAmount = incoming.BuyAmount;
        item.SellAmount = incoming.SellAmount;
        item.PrepaymentAmount = incoming.PrepaymentAmount;
        item.DailyInstallmentAmount = incoming.DailyInstallmentAmount;
        item.ProductName = incoming.ProductName;
        item.ProductId = incoming.ProductId;
    }
}