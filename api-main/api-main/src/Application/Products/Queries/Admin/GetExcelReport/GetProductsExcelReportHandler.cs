using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries.Admin.GetExcelReport;

public class GetProductsExcelReportHandler : IRequestHandler<GetProductsExcelReportQuery, Result<GetProductsExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductsExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetProductsExcelReportResponse>> Handle(GetProductsExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products.AsNoTracking()
            .Where(x => x.Warehouse!.BranchId == request.BranchId)
            .Select(x => new GetProductExcelReportItem
            {
                Id = x.Id,
                Name = x.Name,
                RemainingCount = x.RemainingCount,
                BuyAmount = x.BuyAmount,
                SellAmount = x.SellAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                Description = x.Description,
                CreatedAt = x.CreatedAt!.Value,
                CreatorName = x.Creator!.FullName,
                WarehouseName = x.Warehouse!.Name,
                ProductCategoryName = x.Category!.Name
            })
            .ToListAsync(cancellationToken);

        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "العنصر", propertyName: "Name"),
            new(headerText: "عدد العناصر", propertyName: "RemainingCount"),
            new(headerText: "سعر الشراء", propertyName: "BuyAmount"),
            new(headerText: "سعر البيع", propertyName: "SellAmount"),
            new(headerText: "القسط", propertyName: "DailyInstallmentAmount"),
            new(headerText: "ملاحظات", propertyName: "Description"),
            new(headerText: "تاريخ الأضافة", propertyName: "CreatedAt"),
            new(headerText: "المستخدم", propertyName: "CreatorName"),
            new(headerText: "الخزينة", propertyName: "WarehouseName"),
            new(headerText: "الفئة", propertyName: "ProductCategoryName")
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("المنتجات", products, columns);

        return new GetProductsExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Products-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}