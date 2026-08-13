using Application.Attachments.Common;
using Application.Common.Interfaces;
using Application.Expenses.Common;
using Application.Expenses.Queries.Admin.GetPaginated;
using Microsoft.EntityFrameworkCore;

namespace Application.Expenses.Queries.Admin.GetById;

public class GetExpenseByIdHandler : IRequestHandler<GetExpenseByIdQuery, Result<GetExpenseByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetExpenseByIdResponse>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _dbContext.Expenses.AsNoTracking()
            .Select(x => new GetExpenseByIdResponse
            {
                Id = x.Id,
                FactorNumber = x.FactorNumber,
                TotalAmount = x.TotalAmount,
                SafeType = x.SafeType,
                ExpenseItemsCount = x.ExpenseItems.Count,
                ExpenseItems = x.ExpenseItems
                    .Select(ei => new ExpenseItemDto
                    {
                        Id = ei.Id,
                        Quantity = ei.Quantity,
                        Amount = ei.Amount,
                        Name = ei.Name
                    })
                    .ToList(),
                Attachments = x.Attachments
                    .Select(a => new GetAttachmentDto
                    {
                        Id = a.Id,
                        OriginalFileName = a.OriginalFileName,
                        RelativePath = a.RelativePath,
                        Type = a.Type,
                        FileSizeInByte = a.FileSizeInByte
                    })
                    .ToList(),
                CreatedAt = x.CreatedAt!.Value,
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (expense is null)
            return ExpenseErrors.ExpenseNotFound;

        return expense;
    }
}
