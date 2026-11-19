using Application.Common.Constants;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Expenses.Common;
using Application.Safes.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.AttachmentAggregate.Enums;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Expenses.Commands.Admin.Create;

public class CreateExpenseHandler : IRequestHandler<CreateExpenseCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileManager _fileManager;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly ISafeLedger _safeLedger;

    public CreateExpenseHandler(IApplicationDbContext dbContext, IFileManager fileManager, INotificationService notificationService,
        IActivityLogService activityLogService, ISafeLedger safeLedger)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _safeLedger = safeLedger;
    }

    public async Task<Result> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var isFactorNumberAlreadyExist = await _dbContext.Expenses
            .AnyAsync(x => x.FactorNumber == request.FactorNumber, cancellationToken);
        if (isFactorNumberAlreadyExist)
            return ExpenseErrors.ExpenseWithThisFactorNumberAlreadyExist;

        if (request.BranchId is null)
            return SafeErrors.BranchIsRequired;

        var branchId = request.BranchId.Value;
        var branchSafe = await _safeLedger.GetBranchSafeAsync(branchId, cancellationToken);
        if (branchSafe.IsFailed)
            return branchSafe.Error;

        var totalAmount = request.ExpenseItems.Sum(x => x.Amount);

        var transaction = new Transaction(totalAmount, TransactionType.Expense, TransactionStatus.Approved, TransactionDirection.Out,
            branchSafe.Data!.Id);

        var expense = new Expense(request.FactorNumber, totalAmount, request.SafeType, branchId, transaction, request.ExpenseItems
            .Select(x => new ExpenseItem(x.Name, x.Quantity, x.Amount))
            .ToList());

        _dbContext.Expenses.Add(expense);

        _notificationService.AddTransactionCreatedNotification(transaction, branchId);

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var debit = await _safeLedger.RecordAsync(
            new SafeLedgerEntry
            {
                SafeId = branchSafe.Data!.Id,
                Kind = SafeMovementKind.ExpenseOut,
                Amount = totalAmount,
                Note = $"مصروف بفاتورة {request.FactorNumber}",
                ExpenseId = expense.Id
            },
            cancellationToken);

        if (debit.IsFailed)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return debit.Error;
        }

        await _activityLogService.AddAsync(
            branchId,
            ActivityType.ExpenseCreated,
            TargetEntityType.Expense,
            expense.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await dbTransaction.CommitAsync(cancellationToken);

        await SaveAttachmentsAsync(request.Attachments, expense.Id);

        return Result.Success();
    }

    private async Task SaveAttachmentsAsync(List<IFormFile>? files, int expenseId)
    {
        if (files is null)
            return;

        foreach (var file in files)
        {
            var fileDto = await _fileManager.SaveFileAsync(file, Path.Combine(AttachmentPath.Root,
                AttachmentPath.ExpensesSubfolder(expenseId)));

            var attachment = new Attachment(file.FileName, fileDto.Name, fileDto.RelativePath, file.ContentType, fileDto.Extension,
                file.Length, AttachmentType.Factor, AttachmentUtility.GetFileType(file.ContentType))
            {
                ExpenseId = expenseId
            };

            _dbContext.Attachments.Add(attachment);
        }

        await _dbContext.SaveChangesAsync();
    }
}