using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Products.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Purchases.Queries.Admin.GetPaginated;

public class GetPaginatedPurchasesHandler : IRequestHandler<GetPaginatedPurchasesQuery,
    Result<PaginatedList<GetPaginatedPurchasesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedPurchasesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedList<GetPaginatedPurchasesResponse>>> Handle(GetPaginatedPurchasesQuery request,
        CancellationToken cancellationToken)
    {
        var purchases = await _dbContext.Purchases.AsNoTracking()
            .When(request.Filter.BranchId is not null, x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.FactorNumber.ToString().Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new GetPaginatedPurchasesResponse
            {
                Id = x.Id,
                FactorNumber = x.FactorNumber,
                TotalAmount = x.TotalAmount,
                SafeType = x.SafeType,
                PurchaseItemsCount = x.PurchaseItems.Count,
                PurchaseItems = x.PurchaseItems
                    .Select(pi => new PurchaseItemDto
                    {
                        Id = pi.Id,
                        Quantity = pi.Quantity,
                        Amount = pi.Amount,
                        Product = pi.ProductId != null
                            ? new ProductDto
                            {
                                Id = pi.ProductId.Value,
                                Name = pi.Product!.Name
                            }
                            : null,
                        ForeignProductName = pi.ForeignProductName
                    })
                    .ToList(),
                CreatedAt = x.CreatedAt!.Value
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return purchases;
    }
}