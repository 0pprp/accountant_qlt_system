using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries.Admin.GetPaginated;

public class GetPaginatedProductsHandler : IRequestHandler<GetPaginatedProductsQuery, Result<GetPaginatedProductsResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedProductsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetPaginatedProductsResponse>> Handle(GetPaginatedProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products.AsNoTracking()
            .Where(x => x.Warehouse!.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!) ||
                                                              x.Category!.Name.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.CategoryId is not null, x => x.CategoryId == request.Filter.CategoryId);

        var products = await query
            .Select(x => new GetPaginatedProductsItem
            {
                Id = x.Id,
                Name = x.Name,
                RemainingCount = x.RemainingCount,
                BuyAmount = x.BuyAmount,
                SellAmount = x.SellAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                CategoryName = x.Category!.Name,
                WarehouseName = x.Warehouse!.Name,
                CreatedAt = x.CreatedAt!.Value,
                CreatorName = x.Creator!.FullName
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return new GetPaginatedProductsResponse
        {
            PaginatedProducts = products,
            TotalRemainingCount = await query.SumAsync(x => x.RemainingCount, cancellationToken),
            TotalBuyAmount = await query.SumAsync(x => x.BuyAmount * x.RemainingCount, cancellationToken),
            TotalSellAmount = await query.SumAsync(x => x.SellAmount * x.RemainingCount, cancellationToken)
        };
    }
}