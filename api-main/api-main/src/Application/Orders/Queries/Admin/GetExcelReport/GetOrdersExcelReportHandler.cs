using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Orders.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.Admin.GetExcelReport;

public class GetOrdersExcelReportHandler : IRequestHandler<GetOrdersExcelReportQuery, Result<GetOrdersExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrdersExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetOrdersExcelReportResponse>> Handle(GetOrdersExcelReportQuery request, CancellationToken cancellationToken)
    {
        var columnsToInclude = OrderColumns.All;

        if (request.SelectedColumns.Count > 0)
        {
            columnsToInclude = OrderColumns.All
                .Where(x => request.SelectedColumns.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        var fieldSelectors = columnsToInclude.ToDictionary(x => x.Key, x => x.Selector);

        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.Customer!.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Seller!.FullName.Contains(request.Filter.SearchTerm!))
            .ProjectToDynamicAsync(fieldSelectors, cancellationToken);

        var columns = columnsToInclude.Select(x => new ExcelColumn(headerText: x.DisplayName, propertyName: x.Key)).ToList();

        var byteArray = ExcelReportGenerator.GenerateExcelReport("المبیعات", orders, columns);

        return new GetOrdersExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Orders-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}