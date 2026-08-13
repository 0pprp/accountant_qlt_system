using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductCategories.Queries.Admin.GetPaginated;

public class GetPaginatedProductCategoriesHandler : IRequestHandler<GetPaginatedProductCategoriesQuery, 
    Result<GetPaginatedProductCategoriesResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedProductCategoriesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetPaginatedProductCategoriesResponse>> Handle(GetPaginatedProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var productCategories = await _dbContext.ProductCategories.AsNoTracking()
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetPaginatedProductCategoriesItem
            {
                Id = x.Id,
                Name = x.Name,
                ProductsCount = x.Products.Count(p => p.Warehouse!.BranchId == request.Filter.BranchId),
                CreatedAt = x.CreatedAt!.Value
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return new GetPaginatedProductCategoriesResponse
        {
            Items = productCategories,
            TotalProductsCount = await _dbContext.Products
                .Where(x => x.Warehouse!.BranchId == request.Filter.BranchId)
                .CountAsync(cancellationToken)
        };
    }
}