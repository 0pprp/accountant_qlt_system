namespace Application.Expenses.Commands.Admin.Update;

public record UpdateExpenseCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required List<ExpenseItemDto> ExpenseItems { get; set; }
}

public record UpdateExpenseDto
{
    public required List<ExpenseItemDto> ExpenseItems { get; set; }
}

public record ExpenseItemDto
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public int? Quantity { get; set; }
    public required double Amount { get; set; }
}
