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

    public CreateExpenseHandler(IApplicationDbContext dbContext, IFileManager fileManager, INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var isFactorNumberAlreadyExist = await _dbContext.Expenses
            .AnyAsync(x => x.FactorNumber == request.FactorNumber, cancellationToken);
        if (isFactorNumberAlreadyExist)
            return ExpenseErrors.ExpenseWithThisFactorNumberAlreadyExist;

        var safe = await GetSafeAsync(request.SafeType, request.BranchId);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        var totalAmount = request.ExpenseItems.Sum(x => x.Amount);
        if (safe.RemainingCashAmount < totalAmount)
            return SafeErrors.ThereIsNotEnoughMoneyInTheSafe;

        safe.RemainingCashAmount -= totalAmount;

        var transaction = new Transaction(totalAmount, TransactionType.Expense, TransactionStatus.Approved, TransactionDirection.Out,
            safe.Id);

        var expense = new Expense(request.FactorNumber, totalAmount, request.SafeType, request.BranchId, transaction, request.ExpenseItems
            .Select(x => new ExpenseItem(x.Name, x.Quantity, x.Amount))
            .ToList());

        _dbContext.Expenses.Add(expense);

        if (request.SafeType == SafeType.Branch && request.BranchId is not null)
            _notificationService.AddTransactionCreatedNotification(transaction, request.BranchId.Value);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            request.BranchId ?? safe.BranchId!.Value,
            ActivityType.ExpenseCreated,
            TargetEntityType.Expense,
            expense.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SaveAttachmentsAsync(request.Attachments, expense.Id);

        return Result.Success();
    }

    private async Task<Safe?> GetSafeAsync(SafeType safeType, int? branchId)
    {
        return await _dbContext.Safes
            .When(safeType == SafeType.Branch, x => x.BranchId == branchId)
            .FirstOrDefaultAsync();
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