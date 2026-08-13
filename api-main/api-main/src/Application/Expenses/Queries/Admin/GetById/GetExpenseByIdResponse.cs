using Application.Attachments.Common;
using Application.Expenses.Queries.Admin.GetPaginated;
using Domain.Entities.SafeAggregate.Enums;

namespace Application.Expenses.Queries.Admin.GetById;

public record GetExpenseByIdResponse
{
    public int Id { get; set; }
    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public SafeType SafeType { get; set; }
    public int ExpenseItemsCount { get; set; }
    public required List<ExpenseItemDto> ExpenseItems { get; set; }
    public required List<GetAttachmentDto> Attachments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
