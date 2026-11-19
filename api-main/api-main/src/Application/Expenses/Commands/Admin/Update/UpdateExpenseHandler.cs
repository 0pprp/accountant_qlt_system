using Application.Common.Interfaces;
using Application.Expenses.Common;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Expenses.Commands.Admin.Update;

public class UpdateExpenseHandler : IRequestHandler<UpdateExpenseCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly ISafeLedger _safeLedger;

    public UpdateExpenseHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService, ISafeLedger safeLedger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _safeLedger = safeLedger;
    }

    public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _dbContext.Expenses
            .Include(x => x.ExpenseItems)
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (expense is null)
            return ExpenseErrors.ExpenseNotFound;

        UpdateExpenseItems(expense, request);

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var safeResult = await HandleSafeTransactionsAsync(expense, cancellationToken);
        if (safeResult.IsFailed)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return safeResult;
        }

        var safe = await _dbContext.Safes.FirstAsync(x => x.Id == expense.Transactions.First().SafeId, cancellationToken);

        await _activityLogService.AddAsync(
            expense.BranchId ?? safe.BranchId,
            ActivityType.ExpenseUpdated,
            TargetEntityType.Expense,
            expense.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
    
    private static void UpdateExpenseItems(Expense expense, UpdateExpenseCommand request)
    {
        var requestItems = request.ExpenseItems
            .Where(x => x.Id.HasValue)
            .ToDictionary(x => x.Id!.Value);

        foreach (var existingItem in expense.ExpenseItems)
        {
            if (requestItems.TryGetValue(existingItem.Id, out var updatedItem) == false) continue;

            existingItem.Quantity = updatedItem.Quantity;
            existingItem.Amount = updatedItem.Amount;
        }

        var itemsToRemove = expense.ExpenseItems
            .Where(x => requestItems.ContainsKey(x.Id) == false)
            .ToList();
        foreach (var itemToRemove in itemsToRemove)
        {
            expense.ExpenseItems.Remove(itemToRemove);
        }

        foreach (var newItem in request.ExpenseItems.Where(x => x.Id.HasValue == false))
        {
            expense.ExpenseItems.Add(new ExpenseItem(
                newItem.Name,
                newItem.Quantity,
                newItem.Amount));
        }
    }

    private async Task<Result> HandleSafeTransactionsAsync(Expense expense, CancellationToken cancellationToken)
    {
        var safeId = expense.Transactions.First().SafeId;
        var safe = await _dbContext.Safes.FirstAsync(x => x.Id == safeId, cancellationToken);

        var newTotalAmount = expense.ExpenseItems.Sum(x => x.Amount);
        var oldTotalAmount = expense.TotalAmount;

        if (newTotalAmount == oldTotalAmount)
            return Result.Success();

        var isAdditional = newTotalAmount > oldTotalAmount;
        var delta = Math.Abs(newTotalAmount - oldTotalAmount);

        var transaction = new Transaction(
            delta,
            TransactionType.Expense,
            TransactionStatus.Approved,
            isAdditional ? TransactionDirection.Out : TransactionDirection.In,
            safeId);

        expense.Transactions.Add(transaction);
        _notificationService.AddTransactionCreatedNotification(transaction, safe.BranchId);

        var movement = await _safeLedger.RecordAsync(
            new SafeLedgerEntry
            {
                SafeId = safeId,
                Kind = isAdditional ? SafeMovementKind.ExpenseOut : SafeMovementKind.Refund,
                Amount = delta,
                Note = $"تعديل مصروف بفاتورة {expense.FactorNumber}",
                ExpenseId = expense.Id
            },
            cancellationToken);

        if (movement.IsFailed)
            return movement.Error;

        expense.TotalAmount = newTotalAmount;
        return Result.Success();
    }
}
