using Domain.Entities.SafeAggregate.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Expenses.Commands.Admin.Create;

public record CreateExpenseCommand : IRequest<Result>
{
    public required int FactorNumber { get; set; }
    public required SafeType SafeType { get; set; }
    public int? BranchId { get; set; }
    public required List<ExpenseItemDto> ExpenseItems { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public record ExpenseItemDto
{
    public required string Name { get; set; }
    public int? Quantity { get; set; }
    public required double Amount { get; set; }
}
