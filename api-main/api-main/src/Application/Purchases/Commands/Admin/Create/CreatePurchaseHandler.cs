using Application.Common.Constants;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Purchases.Common;
using Application.Safes.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.AttachmentAggregate.Enums;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Purchases.Commands.Admin.Create;

public class CreatePurchaseHandler : IRequestHandler<CreatePurchaseCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileManager _fileManager;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public CreatePurchaseHandler(IApplicationDbContext dbContext, IFileManager fileManager, INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var isFactorNumberAlreadyExist = await _dbContext.Purchases
            .AnyAsync(x => x.FactorNumber == request.FactorNumber, cancellationToken);
        if (isFactorNumberAlreadyExist)
            return PurchaseErrors.PurchaseWithThisFactorNumberAlreadyExist;

        var safe = await GetSafeAsync(request.SafeType, request.BranchId);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        var totalAmount = request.PurchaseItems.Sum(x => x.Amount);
        if (safe.RemainingCashAmount < totalAmount)
            return SafeErrors.ThereIsNotEnoughMoneyInTheSafe;

        safe.RemainingCashAmount -= totalAmount;

        var transaction = new Transaction(totalAmount, TransactionType.Purchase, TransactionStatus.Approved, TransactionDirection.Out,
            safe.Id);

        var purchase = new Purchase(request.FactorNumber, totalAmount, request.SafeType, request.BranchId, transaction,
            request.PurchaseItems
                .Select(x => new PurchaseItem(x.Quantity, x.Amount, x.ProductId))
                .ToList());
        _dbContext.Purchases.Add(purchase);

        if (request.SafeType == SafeType.Branch && request.BranchId is not null)
            _notificationService.AddTransactionCreatedNotification(transaction, request.BranchId.Value);

        var updateInventoryResult = await UpdateProductInventoryAsync(request.PurchaseItems, cancellationToken);
        if (updateInventoryResult.IsFailed)
            return updateInventoryResult;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            request.BranchId ?? safe.BranchId!.Value,
            ActivityType.PurchaseCreated,
            TargetEntityType.Purchase,
            purchase.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SaveAttachmentsAsync(request.Attachments, purchase.Id);

        return Result.Success();
    }

    private async Task<Safe?> GetSafeAsync(SafeType safeType, int? branchId)
    {
        return await _dbContext.Safes
            .When(safeType == SafeType.Branch, x => x.BranchId == branchId)
            .FirstOrDefaultAsync();
    }

    private async Task<Result> UpdateProductInventoryAsync(List<PurchaseItemDto> purchaseItems, CancellationToken cancellationToken)
    {
        var productIds = purchaseItems.Select(x => x.ProductId).ToList();
        var products = await _dbContext.Products
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            return PurchaseErrors.SomeProductsNotFound;

        foreach (var product in products)
        {
            var quantityToIncrease = purchaseItems.First(x => x.ProductId == product.Id).Quantity;
            product.RemainingCount += quantityToIncrease;
        }

        return Result.Success();
    }

    private async Task SaveAttachmentsAsync(List<IFormFile>? files, int purchaseId)
    {
        if (files is null)
            return;

        foreach (var file in files)
        {
            var fileDto = await _fileManager.SaveFileAsync(file, Path.Combine(AttachmentPath.Root,
                AttachmentPath.PurchasesSubfolder(purchaseId)));

            var attachment = new Attachment(file.FileName, fileDto.Name, fileDto.RelativePath, file.ContentType, fileDto.Extension,
                file.Length, AttachmentType.Factor, AttachmentUtility.GetFileType(file.ContentType))
            {
                PurchaseId = purchaseId
            };

            _dbContext.Attachments.Add(attachment);
        }

        await _dbContext.SaveChangesAsync();
    }
}