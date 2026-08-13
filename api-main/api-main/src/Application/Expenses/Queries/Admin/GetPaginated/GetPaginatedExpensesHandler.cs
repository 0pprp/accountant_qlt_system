using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Expenses.Queries.Admin.GetPaginated;

public class GetPaginatedExpensesHandler : IRequestHandler<GetPaginatedExpensesQuery,
    Result<PaginatedList<GetPaginatedExpensesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedExpensesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedList<GetPaginatedExpensesResponse>>> Handle(GetPaginatedExpensesQuery request,
        CancellationToken cancellationToken)
    {
        var expenses = await _dbContext.Expenses.AsNoTracking()
            .When(request.Filter.BranchId is not null, x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.FactorNumber.ToString().Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new GetPaginatedExpensesResponse
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
                CreatedAt = x.CreatedAt!.Value,
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return expenses;
    }
}
