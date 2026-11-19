using Application.Common.Interfaces;
using Application.Safes.Common;
using Application.Purchases.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ProductAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Purchases.Commands.Admin.Update;

public class UpdatePurchaseHandler : IRequestHandler<UpdatePurchaseCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly ISafeLedger _safeLedger;

    public UpdatePurchaseHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService, ISafeLedger safeLedger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _safeLedger = safeLedger;
    }

    public async Task<Result> Handle(UpdatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .Include(x => x.PurchaseItems)
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (purchase is null)
            return PurchaseErrors.PurchaseNotFound;

        var productIds = GetAllProductIds(purchase, request);
        var products = await LoadProductsAsync(productIds, cancellationToken);
        if (products.Count != productIds.Count)
            return PurchaseErrors.SomeProductsNotFound;

        var inventoryResult = ProcessInventoryChanges(purchase, request, products);
        if (inventoryResult.IsFailed)
            return inventoryResult;

        UpdatePurchaseItems(purchase, request);

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var safeResult = await HandleSafeTransactionsAsync(purchase, cancellationToken);
        if (safeResult.IsFailed)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return safeResult;
        }

        var safe = await _dbContext.Safes.FirstAsync(x => x.Id == purchase.Transactions.First().SafeId, cancellationToken);

        await _activityLogService.AddAsync(
            purchase.BranchId ?? safe.BranchId,
            ActivityType.PurchaseUpdated,
            TargetEntityType.Purchase,
            purchase.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private static HashSet<int> GetAllProductIds(Purchase purchase, UpdatePurchaseCommand request)
    {
        var productIds = new HashSet<int>();

        foreach (var item in purchase.PurchaseItems)
        {
            if (item.ProductId is not null)
                productIds.Add(item.ProductId.Value);
        }

        foreach (var item in request.PurchaseItems)
        {
            if (item.ProductId is not null)
                productIds.Add(item.ProductId.Value);
        }

        return productIds;
    }

    private async Task<Dictionary<int, Product>> LoadProductsAsync(HashSet<int> productIds, CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        return products;
    }

    private static Result ProcessInventoryChanges(Purchase purchase, UpdatePurchaseCommand request, Dictionary<int, Product> products)
    {
        var requestItems = request.PurchaseItems
            .Where(x => x.Id.HasValue)
            .ToDictionary(x => x.Id!.Value);

        foreach (var existingItem in purchase.PurchaseItems)
        {
            if (existingItem.ProductId is null)
                continue;

            var product = products[existingItem.ProductId.Value];

            if (requestItems.TryGetValue(existingItem.Id, out var updatedItem) == false)
            {
                if (product.RemainingCount < existingItem.Quantity)
                    return PurchaseErrors.YouAreNotAllowedToDoThisAction;

                product.RemainingCount -= existingItem.Quantity;
            }
            else
            {
                var quantityDifference = updatedItem.Quantity - existingItem.Quantity;

                if (quantityDifference < 0)
                {
                    if (product.RemainingCount < Math.Abs(quantityDifference))
                        return PurchaseErrors.YouAreNotAllowedToDoThisAction;

                    product.RemainingCount -= Math.Abs(quantityDifference);
                }
                else if (quantityDifference > 0)
                {
                    product.RemainingCount += quantityDifference;
                }
            }
        }

        foreach (var newItem in request.PurchaseItems.Where(x => x.Id.HasValue == false))
        {
            if (newItem.ProductId is null)
                return PurchaseErrors.SomeProductsNotFound;

            var product = products[newItem.ProductId.Value];
            product.RemainingCount += newItem.Quantity;
        }

        return Result.Success();
    }

    private static void UpdatePurchaseItems(Purchase purchase, UpdatePurchaseCommand request)
    {
        var requestItems = request.PurchaseItems
            .Where(x => x.Id.HasValue)
            .ToDictionary(x => x.Id!.Value);

        foreach (var existingItem in purchase.PurchaseItems)
        {
            if (requestItems.TryGetValue(existingItem.Id, out var updatedItem) == false) continue;

            existingItem.Quantity = updatedItem.Quantity;
            existingItem.Amount = updatedItem.Amount;

            if (existingItem.ProductId is null)
                existingItem.ForeignProductName = updatedItem.ForeignProductName;
        }

        var itemsToRemove = purchase.PurchaseItems
            .Where(x => requestItems.ContainsKey(x.Id) == false)
            .ToList();
        foreach (var itemToRemove in itemsToRemove)
        {
            purchase.PurchaseItems.Remove(itemToRemove);
        }

        foreach (var newItem in request.PurchaseItems.Where(x => x.Id.HasValue == false))
        {
            purchase.PurchaseItems.Add(new PurchaseItem(
                newItem.Quantity,
                newItem.Amount,
                newItem.ProductId!.Value));
        }
    }

    private async Task<Result> HandleSafeTransactionsAsync(Purchase purchase, CancellationToken cancellationToken)
    {
        var safeId = purchase.Transactions.First().SafeId;
        var safe = await _dbContext.Safes.FirstAsync(x => x.Id == safeId, cancellationToken);

        var newTotalAmount = purchase.PurchaseItems.Sum(x => x.Amount);
        var oldTotalAmount = purchase.TotalAmount;

        if (newTotalAmount == oldTotalAmount)
            return Result.Success();

        var isAdditional = newTotalAmount > oldTotalAmount;
        var delta = Math.Abs(newTotalAmount - oldTotalAmount);

        var transaction = new Transaction(
            delta,
            TransactionType.Purchase,
            TransactionStatus.Approved,
            isAdditional ? TransactionDirection.Out : TransactionDirection.In,
            safeId);

        purchase.Transactions.Add(transaction);
        _notificationService.AddTransactionCreatedNotification(transaction, safe.BranchId);

        var movement = await _safeLedger.RecordAsync(
            new SafeLedgerEntry
            {
                SafeId = safeId,
                Kind = isAdditional ? SafeMovementKind.PurchaseOut : SafeMovementKind.Refund,
                Amount = delta,
                Note = $"تعديل مشترى بفاتورة {purchase.FactorNumber}",
                PurchaseId = purchase.Id
            },
            cancellationToken);

        if (movement.IsFailed)
            return movement.Error;

        purchase.TotalAmount = newTotalAmount;
        return Result.Success();
    }
}