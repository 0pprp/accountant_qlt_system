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

namespace Application.Orders.Commands.Admin.ChangeApprovalStatus;

public class ChangeOrderApprovalStatusHandler : IRequestHandler<ChangeOrderApprovalStatusCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public ChangeOrderApprovalStatusHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(ChangeOrderApprovalStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Product)
            .Include(x => x.InstallmentPayments)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.Step != OrderStep.Completed)
            return OrderErrors.OrderInformationHasNotCompletedYet;

        if (order.ApprovalStatus != OrderApprovalStatus.Pending)
            return OrderErrors.ApprovalStatusOfOrderHasBeenSetBefore;

        if (request.ApprovalStatus == OrderApprovalStatus.Approved)
        {
            var decreaseProductsInventoryResult = DecreaseProductsInventory(order.OrderItems.ToList());
            if (decreaseProductsInventoryResult.IsFailed)
                return decreaseProductsInventoryResult.Error!;

            await RegisterForeignProductsPurchaseAsync(order, cancellationToken);

            if (order.PrepaymentAmount > 0)
            {
                var firstInstallmentPayment = new InstallmentPayment(order.PrepaymentAmount, order.SaleDate!.Value);

                var seller = await _dbContext.Users.FirstAsync(x => x.Id == order.SellerId, cancellationToken);
                seller.UndeliveredCashAmount += firstInstallmentPayment.Amount;

                order.InstallmentPayments.Add(firstInstallmentPayment);
            }

            order.Approve();
        }
        else
        {
            order.Reject();
        }

        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.OrderApprovalStatusChanged,
            TargetEntityType.Order,
            request.OrderId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task RegisterForeignProductsPurchaseAsync(Order order, CancellationToken cancellationToken)
    {
        var foreignProducts = order.OrderItems
            .Where(x => x.ProductType == ProductType.Foreign)
            .ToList();
        if (foreignProducts.Count == 0)
            return;

        var foreignProductsBuyAmount = foreignProducts.Sum(x => x.BuyAmount);
        var lastFactorNumber = await _dbContext.Purchases.MaxAsync(x => x.FactorNumber, cancellationToken);

        var branchSafe = await _dbContext.Safes.FirstAsync(x => x.BranchId == order.BranchId, cancellationToken);
        branchSafe.RemainingCashAmount -= foreignProductsBuyAmount;

        var transaction = new Transaction(
            foreignProductsBuyAmount,
            TransactionType.Purchase,
            TransactionStatus.Approved,
            TransactionDirection.Out,
            branchSafe.Id);

        var purchase = new Purchase(
            lastFactorNumber + 1,
            foreignProductsBuyAmount,
            SafeType.Branch,
            order.BranchId,
            transaction,
            foreignProducts
                .Select(x => new PurchaseItem(x.Quantity, x.BuyAmount, x.ProductName))
                .ToList())
        {
            OrderId = order.Id
        };

        _dbContext.Purchases.Add(purchase);

        _notificationService.AddTransactionCreatedNotification(transaction, order.BranchId);
    }

    private Result DecreaseProductsInventory(List<OrderItem> orderItems)
    {
        foreach (var orderItem in orderItems.Where(x => x.ProductId is not null))
        {
            if (orderItem.Quantity > orderItem.Product!.RemainingCount)
                return OrderErrors.ThereIsNotEnoughQuantityOfThisProduct;

            orderItem.Product.RemainingCount -= orderItem.Quantity;
        }

        return Result.Success();
    }
}