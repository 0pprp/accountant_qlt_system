using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.Safes.Common;
using Application.Users.Common;
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
    private readonly ISafeLedger _safeLedger;

    public ChangeOrderApprovalStatusHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService, ISafeLedger safeLedger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _safeLedger = safeLedger;
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

        // أي خروج مبكر يتراجع تلقائياً عند الـdispose قبل الـcommit
        var ownsTransaction = _dbContext.Database.CurrentTransaction is null;
        await using var dbTransaction = ownsTransaction
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;

        if (request.ApprovalStatus == OrderApprovalStatus.Approved)
        {
            var decreaseProductsInventoryResult = DecreaseProductsInventory(order.OrderItems.ToList());
            if (decreaseProductsInventoryResult.IsFailed)
                return decreaseProductsInventoryResult.Error!;

            var registerPurchaseResult = await RegisterForeignProductsPurchaseAsync(order, cancellationToken);
            if (registerPurchaseResult.IsFailed)
                return registerPurchaseResult;

            if (order.PrepaymentAmount > 0)
            {
                var firstInstallmentPayment = new InstallmentPayment(order.PrepaymentAmount, order.SaleDate!.Value, PaymentSafePosting.PrepaymentNote);

                var seller = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == order.SellerId, cancellationToken);
                if (seller is null)
                    return UserErrors.UserNotFound;

                seller.UndeliveredCashAmount += firstInstallmentPayment.Amount;

                order.InstallmentPayments.Add(firstInstallmentPayment);

                if (order.OrderListId is int orderListId)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    var posted = await new PaymentSafePosting(_dbContext, _safeLedger).PostAsync(
                        firstInstallmentPayment.Id, order.Id, orderListId, firstInstallmentPayment.Amount,
                        seller.Id, cancellationToken, PaymentSafePosting.PrepaymentNote);
                    if (posted.IsFailed)
                        return posted;
                }
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
        if (dbTransaction is not null)
            await dbTransaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> RegisterForeignProductsPurchaseAsync(Order order, CancellationToken cancellationToken)
    {
        var foreignProducts = order.OrderItems
            .Where(x => x.ProductType == ProductType.Foreign)
            .ToList();
        if (foreignProducts.Count == 0)
            return Result.Success();

        var foreignProductsBuyAmount = foreignProducts.Sum(x => x.BuyAmount);
        var lastFactorNumber = await _dbContext.Purchases
            .Select(x => (int?)x.FactorNumber)
            .MaxAsync(cancellationToken) ?? 0;

        var branchSafe = await _safeLedger.GetBranchSafeAsync(order.BranchId, cancellationToken);
        if (branchSafe.IsFailed)
            return branchSafe.Error;

        var transaction = new Transaction(
            foreignProductsBuyAmount,
            TransactionType.Purchase,
            TransactionStatus.Approved,
            TransactionDirection.Out,
            branchSafe.Data!.Id);

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

        var debit = await _safeLedger.RecordAsync(
            new SafeLedgerEntry
            {
                SafeId = branchSafe.Data!.Id,
                Kind = SafeMovementKind.PurchaseOut,
                Amount = foreignProductsBuyAmount,
                Note = $"شراء منتجات خارجية للمبيع {order.Id}",
                OrderId = order.Id
            },
            cancellationToken);

        if (debit.IsFailed)
            return debit.Error;

        return Result.Success();
    }

    private Result DecreaseProductsInventory(List<OrderItem> orderItems)
    {
        foreach (var orderItem in orderItems.Where(x => x.ProductId is not null))
        {
            if (orderItem.Product is null)
                return OrderErrors.SomeSelectedProductsNotFound;

            if (orderItem.Quantity > orderItem.Product.RemainingCount)
                return OrderErrors.ThereIsNotEnoughQuantityOfThisProduct;

            orderItem.Product.RemainingCount -= orderItem.Quantity;
        }

        return Result.Success();
    }
}