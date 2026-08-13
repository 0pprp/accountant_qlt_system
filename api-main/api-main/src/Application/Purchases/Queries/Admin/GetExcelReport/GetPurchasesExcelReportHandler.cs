using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Purchases.Queries.Admin.GetExcelReport;

public class GetPurchasesExcelReportHandler : IRequestHandler<GetPurchasesExcelReportQuery, Result<GetPurchasesExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPurchasesExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetPurchasesExcelReportResponse>> Handle(GetPurchasesExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var purchases = await _dbContext.Purchases.AsNoTracking()
            .When(request.SearchTerm is not null, x => x.FactorNumber.ToString().Contains(request.SearchTerm!))
            .When(request.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.StartDate)
            .When(request.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.EndDate)
            .Select(x => new GetPurchaseExcelReportItem
            {
                Id = x.Id,
                FactorNumber = x.FactorNumber,
                TotalAmount = x.TotalAmount,
                SafeName = x.Branch!.Safe!.Name,
                PurchaseItemsCount = x.PurchaseItems.Count,
                CreatedAt = x.CreatedAt!.Value
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "فاتوره", propertyName: "FactorNumber"),
            new(headerText: "المبلغ الكلي", propertyName: "TotalAmount"),
            new(headerText: "قاصه", propertyName: "SafeName"),
            new(headerText: "عدد العناصر", propertyName: "PurchaseItemsCount"),
            new(headerText: "تم إنشاؤه في", propertyName: "CreatedAt")
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("المشتريات", purchases, columns);

        return new GetPurchasesExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Purchases-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}