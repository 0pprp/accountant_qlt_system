using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Queries.Admin.GetExcelReport;

public class GetInstallmentPaymentsExcelReportHandler : IRequestHandler<GetInstallmentPaymentsExcelReportQuery,
    Result<GetInstallmentPaymentsExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetInstallmentPaymentsExcelReportHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetInstallmentPaymentsExcelReportResponse>> Handle(GetInstallmentPaymentsExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.StartDate ?? _dateTimeProvider.Today;
        var to = request.EndDate ?? _dateTimeProvider.Today;
        
        var installmentPayments = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.OrderList!.BranchId == request.BranchId)
            .Where(x => x.Date >= from && x.Date <= to)
            .When(request.SearchTerm is not null, x => x.Order!.Customer!.FullName.Contains(request.SearchTerm!) ||
                                                              x.Order.Seller!.FullName.Contains(request.SearchTerm!))
            .Select(x => new GetInstallmentPaymentsExcelReportItem
            {
                Id = x.Id,
                CustomerFullName = x.Order!.Customer!.FullName,
                Date = x.Date.ToString("yyyy-MM-dd"),
                SellerFullName = x.Order.Seller!.FullName,
                Amount = x.Amount,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "الأسم الثلاثي", propertyName: "CustomerFullName"),
            new(headerText: "المبيع", propertyName: "ProductName"),
            new(headerText: "التاريخ", propertyName: "Date"),
            new(headerText: "المندوب", propertyName: "SellerFullName"),
            new(headerText: "المبلغ", propertyName: "Amount"),
            new(headerText: "ملاحظات", propertyName: "Description")
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("تسدیدات", installmentPayments, columns);

        return new GetInstallmentPaymentsExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"InstallmentPayments-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}