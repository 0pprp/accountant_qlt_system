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

namespace Application.Orders.Commands.Admin.SetSellerInfo;

public class SetOrderSellerInfoHandler : IRequestHandler<SetOrderSellerInfoCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public SetOrderSellerInfoHandler(
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

    public async Task<Result> Handle(SetOrderSellerInfoCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.Step == OrderStep.Completed)
            return OrderErrors.OrderInfoHasBeenCompletedBefore;

        var orderList = await _dbContext.OrderLists.FirstOrDefaultAsync(
            x => x.Id == request.OrderListId && (x.MandobId == request.SellerId || x.MotabaId == request.SellerId), cancellationToken);
        if (orderList is null)
            return OrderErrors.TheSellerDoesNotBelongToThisOrderList;

        if (order.BranchId != orderList.BranchId)
            return OrderErrors.OrderListDoesNotBelongToOrderBranch;

        if (_currentUserService.Role == Application.Common.SeedData.Roles.MainAccountant.Name)
        {
            var threeDaysAgo = DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(-3);
            if (request.SaleDate < threeDaysAgo)
            {
                return OrderErrors.YouCanNotAddOrderForMoreThanThreeDaysAgo;
            }
        }

        await RegisterForeignProductsPurchaseAsync(order, cancellationToken);

        InstallmentPayment? firstInstallmentPayment = null;
        if (order.PrepaymentAmount > 0)
        {
            firstInstallmentPayment = new InstallmentPayment(order.PrepaymentAmount, request.SaleDate);

            var seller = await _dbContext.Users.FirstAsync(x => x.Id == request.SellerId, cancellationToken);
            seller.UndeliveredCashAmount += firstInstallmentPayment.Amount;
        }

        order.SetSeller(
            request.SellerId,
            request.OrderListId,
            null,
            request.CreationAddress,
            request.SaleDate,
            request.SaleTime,
            firstInstallmentPayment);

        order.Approve();

        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.OrderSellerInfoSet,
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
}