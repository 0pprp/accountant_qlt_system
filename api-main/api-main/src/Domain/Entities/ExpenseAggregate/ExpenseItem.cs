using Domain.Common;

namespace Domain.Entities.ExpenseAggregate;

public class ExpenseItem : AuditableEntity
{
    public ExpenseItem(string name, int? quantity, double amount, int expenseId)
    {
        Name = name;
        Quantity = quantity;
        Amount = amount;
        ExpenseId = expenseId;
    }
    
    public ExpenseItem(string name, int? quantity, double amount)
    {
        Name = name;
        Quantity = quantity;
        Amount = amount;
    }

    public string Name { get; set; }
    public int? Quantity { get; set; }
    public double Amount { get; set; }
    public int ExpenseId { get; set; }

    public Expense? Expense { get; set; }
}