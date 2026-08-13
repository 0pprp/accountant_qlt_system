using Application.Common.Extensions;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductCategories.Queries.App.GetPaginated;

public class GetPaginatedProductCategoriesHandler : IRequestHandler<GetPaginatedProductCategoriesQuery,
    List<GetPaginatedProductCategoriesResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedProductCategoriesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GetPaginatedProductCategoriesResponse>> Handle(GetPaginatedProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var productCategories = await _dbContext.ProductCategories.AsNoTracking()
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetPaginatedProductCategoriesResponse
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt!.Value
            })
            .KeysetPaginateAsync(key: x => x.CreatedAt, pagination: request.Pagination, cancellationToken: cancellationToken);

        return productCategories;
    }
}