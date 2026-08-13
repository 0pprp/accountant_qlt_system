using Domain.Entities.SafeAggregate.Enums;

namespace Application.Expenses.Queries.Admin.GetPaginated;

public record GetPaginatedExpensesResponse
{
    public int Id { get; set; }
    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public SafeType SafeType { get; set; }
    public int ExpenseItemsCount { get; set; }
    public required List<ExpenseItemDto> ExpenseItems { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record ExpenseItemDto
{
    public int Id { get; set; }
    public int? Quantity { get; set; }
    public double Amount { get; set; }
    public required string Name { get; set; }
}
