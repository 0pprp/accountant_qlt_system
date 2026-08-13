using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetTransactionsExcelReport;

public class GetSafeTransactionsExcelReportHandler : IRequestHandler<GetSafeTransactionsExcelReportQuery,
    Result<GetSafeTransactionsExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSafeTransactionsExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetSafeTransactionsExcelReportResponse>> Handle(GetSafeTransactionsExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _dbContext.Transactions.AsNoTracking()
            .Where(x => x.SafeId == request.SafeId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new GetSafeTransactionsExcelReportItem 
            {
                Id = x.Id,
                Source = x.SafeTransferTransaction != null
                    ? x.SafeTransferTransaction.SourceSafe!.Name
                    : x.SellerCashDeliveryTransaction!.Seller!.FullName,
                Destination = x.SafeTransferTransaction != null
                    ? x.SafeTransferTransaction.DestinationSafe!.Name
                    : x.Safe!.Name,
                Amount = x.Amount,
                Type = x.Type.ToArabicString(),
                Status = x.Status.ToArabicString(),
                StatusDescription = x.StatusDescription,
                Direction = x.Direction.ToArabicString(),
                CreatedAt = x.CreatedAt!.Value.ToString("yyyy-MM-dd")
            })
            .ToListAsync(cancellationToken);

        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "من", propertyName: "Source"),
            new(headerText: "الى", propertyName: "Destination"),
            new(headerText: "المبلغ", propertyName: "Amount"),
            new(headerText: "نوع", propertyName: "Type"),
            new(headerText: "الحالة", propertyName: "Status"),
            new(headerText: "ملاحظات", propertyName: "StatusDescription"),
            new(headerText: "اتجاه المعاملة", propertyName: "Direction"),
            new(headerText: "التاريخ", propertyName: "CreatedAt"),
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("العملاء", transactions, columns);

        return new GetSafeTransactionsExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Transaction-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}