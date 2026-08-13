using Application.Common.Interfaces;
using Application.Expenses.Common;
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

    public UpdateExpenseHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
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

        await HandleSafeTransactionsAsync(expense, cancellationToken);

        var safe = await _dbContext.Safes.FirstAsync(x => x.Id == expense.Transactions.First().SafeId, cancellationToken);

        await _activityLogService.AddAsync(
            expense.BranchId ?? safe.BranchId!.Value,
            ActivityType.ExpenseUpdated,
            TargetEntityType.Expense,
            expense.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);
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

    private async Task HandleSafeTransactionsAsync(Expense expense, CancellationToken cancellationToken)
    {
        var safeId = expense.Transactions.First().SafeId;
        var safe = await _dbContext.Safes.FirstAsync(x => x.Id == safeId, cancellationToken);

        var newTotalAmount = expense.ExpenseItems.Sum(x => x.Amount);
        var oldTotalAmount = expense.TotalAmount;

        if (newTotalAmount == oldTotalAmount)
            return;

        if (newTotalAmount > oldTotalAmount)
        {
            var additionalAmount = newTotalAmount - oldTotalAmount;

            var transaction = new Transaction(
                additionalAmount,
                TransactionType.Expense,
                TransactionStatus.Approved,
                TransactionDirection.Out,
                safeId);

            expense.Transactions.Add(transaction);
            safe.RemainingCashAmount -= additionalAmount;

            if (safe.BranchId is not null)
                _notificationService.AddTransactionCreatedNotification(transaction, safe.BranchId.Value);
        }
        else
        {
            var refundAmount = oldTotalAmount - newTotalAmount;

            var transaction = new Transaction(
                refundAmount,
                TransactionType.Expense,
                TransactionStatus.Approved,
                TransactionDirection.In,
                safeId);

            expense.Transactions.Add(transaction);
            safe.RemainingCashAmount += refundAmount;

            if (safe.BranchId is not null)
                _notificationService.AddTransactionCreatedNotification(transaction, safe.BranchId.Value);
        }

        expense.TotalAmount = newTotalAmount;
    }
}
