using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Queries.Admin.GetExcelReport;

public class GetOrderListsExcelReportHandler : IRequestHandler<GetOrderListsExcelReportQuery, Result<GetOrderListsExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrderListsExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetOrderListsExcelReportResponse>> Handle(GetOrderListsExcelReportQuery request, CancellationToken cancellationToken)
    {
        var orderLists = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetOrderListsExcelReportItem
            {
                Id = x.Id,
                Name = x.Name,
                MandobName = x.Mandob!.FullName,
                MotabaName = x.Motaba!.FullName,
                BranchName = x.Branch!.Name,
                CustomersCount = x.Orders.Select(o => o.CustomerId).Distinct().Count(),
                CreatedAt = x.CreatedAt!.Value
            })
            .ToListAsync(cancellationToken);
        
        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "القائمة", propertyName: "Name"),
            new(headerText: "الفرع", propertyName: "BranchName"),
            new(headerText: "المتابع", propertyName: "MotabaName"),
            new(headerText: "المندوب", propertyName: "MandobName"),
            new(headerText: "عدد الزبائن", propertyName: "CustomersCount"),
            new(headerText: "تاريخ الأنشاء", propertyName: "CreatedAt"),
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("القائمات", orderLists, columns);

        return new GetOrderListsExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"OrderLists-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}