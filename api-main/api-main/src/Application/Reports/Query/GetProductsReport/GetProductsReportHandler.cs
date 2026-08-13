using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Query.GetProductsReport;

public class GetProductsReportHandler : IRequestHandler<GetProductsReportQuery, Result<GetProductsReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductsReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetProductsReportResponse>> Handle(GetProductsReportQuery request, CancellationToken cancellationToken)
    {
        var productsCount = await _dbContext.Products.AsNoTracking()
            .Where(x => x.Warehouse!.BranchId == request.BranchId)
            .CountAsync(cancellationToken);

        return new GetProductsReportResponse
        {
            ProductsCount = productsCount
        };
    }
}