using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Query.GetProductsReport;

public class GetProductsReportHandler : IRequestHandler<GetProductsReportQuery, Result<GetProductsReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetProductsReportHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetProductsReportResponse>> Handle(GetProductsReportQuery request, CancellationToken cancellationToken)
    {
        var accessDenied = Application.Reports.Common.ReportBranchAccess.DenyIfOutOfScope(
            _currentUserService, request.BranchId);
        if (accessDenied is not null)
            return accessDenied;

        var productsCount = await _dbContext.Products.AsNoTracking()
            .Where(x => x.Warehouse!.BranchId == request.BranchId)
            .CountAsync(cancellationToken);

        return new GetProductsReportResponse
        {
            ProductsCount = productsCount
        };
    }
}