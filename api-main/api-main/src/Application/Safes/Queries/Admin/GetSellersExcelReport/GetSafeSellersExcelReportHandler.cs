using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetSellersExcelReport;

public class GetSafeSellersExcelReportHandler : IRequestHandler<GetSafeSellersExcelReportQuery, Result<GetSafeSellersExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSafeSellersExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetSafeSellersExcelReportResponse>> Handle(GetSafeSellersExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var sellers = await _dbContext.Users.AsNoTracking()
            .Where(x => x.OrderListAsMandob!.Branch!.Safe!.Id == request.SafeId)
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetSafeSellersExcelReportItem
            {
                Id = x.Id,
                FullName = x.FullName,
                DeliveredCashAmount = x.SellerCashDeliveryTransactions.Sum(s => s.Transaction!.Amount),
                UndeliveredCashAmount = x.UndeliveredCashAmount,
                LastCashDeliveryDate = x.SellerCashDeliveryTransactions.Max(s => (DateTimeOffset?)s.Transaction!.CreatedAt!.Value).ToString(),
                LastCashDeliveryDescription = x.SellerCashDeliveryTransactions
                    .OrderByDescending(s => s.Transaction!.CreatedAt)
                    .Select(s => s.Description)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
        
        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "الأسم الثلاثي", propertyName: "FullName"),
            new(headerText: "التسديدات", propertyName: "DeliveredCashAmount"),
            new(headerText: "الواردات", propertyName: "UndeliveredCashAmount"),
            new(headerText: "اخر تسديد", propertyName: "LastCashDeliveryDate"),
            new(headerText: "ملاحظات", propertyName: "LastCashDeliveryDescription")
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("البائعين", sellers, columns);

        return new GetSafeSellersExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Sellers-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}