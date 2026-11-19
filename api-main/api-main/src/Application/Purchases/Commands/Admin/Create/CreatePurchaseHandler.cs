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
    private readonly ISafeLedger _safeLedger;

    public CreatePurchaseHandler(IApplicationDbContext dbContext, IFileManager fileManager, INotificationService notificationService,
        IActivityLogService activityLogService, ISafeLedger safeLedger)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _safeLedger = safeLedger;
    }

    public async Task<Result> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var isFactorNumberAlreadyExist = await _dbContext.Purchases
            .AnyAsync(x => x.FactorNumber == request.FactorNumber, cancellationToken);
        if (isFactorNumberAlreadyExist)
            return PurchaseErrors.PurchaseWithThisFactorNumberAlreadyExist;

        if (request.BranchId is null)
            return SafeErrors.BranchIsRequired;

        var branchId = request.BranchId.Value;
        var branchSafe = await _safeLedger.GetBranchSafeAsync(branchId, cancellationToken);
        if (branchSafe.IsFailed)
            return branchSafe.Error;

        var totalAmount = request.PurchaseItems.Sum(x => x.Amount);

        var transaction = new Transaction(totalAmount, TransactionType.Purchase, TransactionStatus.Approved, TransactionDirection.Out,
            branchSafe.Data!.Id);

        var purchase = new Purchase(request.FactorNumber, totalAmount, request.SafeType, branchId, transaction,
            request.PurchaseItems
                .Select(x => new PurchaseItem(x.Quantity, x.Amount, x.ProductId))
                .ToList());
        _dbContext.Purchases.Add(purchase);

        _notificationService.AddTransactionCreatedNotification(transaction, branchId);

        var updateInventoryResult = await UpdateProductInventoryAsync(request.PurchaseItems, cancellationToken);
        if (updateInventoryResult.IsFailed)
            return updateInventoryResult;

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var debit = await _safeLedger.RecordAsync(
            new SafeLedgerEntry
            {
                SafeId = branchSafe.Data!.Id,
                Kind = SafeMovementKind.PurchaseOut,
                Amount = totalAmount,
                Note = $"مشترى بفاتورة {request.FactorNumber}",
                PurchaseId = purchase.Id
            },
            cancellationToken);

        if (debit.IsFailed)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return debit.Error;
        }

        await _activityLogService.AddAsync(
            branchId,
            ActivityType.PurchaseCreated,
            TargetEntityType.Purchase,
            purchase.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await dbTransaction.CommitAsync(cancellationToken);

        await SaveAttachmentsAsync(request.Attachments, purchase.Id);

        return Result.Success();
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