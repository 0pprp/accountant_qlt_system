using Application.Common.Extensions;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries.App.GetPaginated;

public class GetPaginatedProductsHandler : IRequestHandler<GetPaginatedProductsQuery, Result<List<GetPaginatedProductsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPaginatedProductsHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GetPaginatedProductsResponse>>> Handle(GetPaginatedProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.Warehouse!.BranchId))
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!) ||
                                                              x.Category!.Name.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.CategoryId is not null, x => x.CategoryId == request.Filter.CategoryId)
            .Select(x => new GetPaginatedProductsResponse
            {
                Id = x.Id,
                Name = x.Name,
                BuyAmount = x.BuyAmount,
                SellAmount = x.SellAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                Category = new ProductCategoryDto
                {
                    Id = x.CategoryId,
                    Name = x.Category!.Name
                },
                CreatedAt = x.CreatedAt!.Value,
            })
            .KeysetPaginateAsync(key: x => x.CreatedAt, pagination: request.Pagination, cancellationToken: cancellationToken);

        return products;
    }
}