using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Customers.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.Admin.GetExcelReport;

public class GetCustomersExcelReportHandler : IRequestHandler<GetCustomersExcelReportQuery, Result<GetCustomersExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCustomersExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetCustomersExcelReportResponse>> Handle(GetCustomersExcelReportQuery request, CancellationToken cancellationToken)
    {
        var asOf = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        var rows = await _dbContext.Customers.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Business.Name.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                BusinessName = x.Business.Name,
                BusinessAddress = x.Business.Address,
                OrderListName = x.Orders.Any() ? x.Orders.First().OrderList!.Name : null,
                OrdersCount = x.Orders.Count,
                LastInstallmentPaymentDate = x.Orders.SelectMany(o => o.InstallmentPayments).Any()
                    ? x.Orders.SelectMany(o => o.InstallmentPayments).Max(ip => ip.Date)
                    : (DateOnly?)null,
                SellAmount = x.Orders.Sum(o => o.SellAmount),
                PaidAmount = x.Orders.SelectMany(o => o.InstallmentPayments).Sum(ip => ip.Amount),
                SaleDate = x.Orders.Where(o => o.SaleDate != null).Select(o => o.SaleDate).Min(),
                EarliestOrderCreated = x.Orders.Where(o => o.CreatedAt != null).Select(o => (DateTimeOffset?)o.CreatedAt).Min(),
            })
            .ToListAsync(cancellationToken);

        var customers = rows.Select(x =>
        {
            var saleDate = x.SaleDate
                           ?? (x.EarliestOrderCreated is { } created
                               ? DateOnly.FromDateTime(created.DateTime)
                               : (DateOnly?)null);
            var remaining = x.SellAmount - x.PaidAmount;
            return new GetCustomersExcelReportItem
            {
                Id = x.Id,
                FullName = x.FullName,
                BusinessName = x.BusinessName,
                BusinessAddress = x.BusinessAddress,
                OrderListName = x.OrderListName,
                OrdersCount = x.OrdersCount,
                LastInstallmentPaymentDate = x.LastInstallmentPaymentDate?.ToString(),
                Rating = CustomerRatingCalculator.EvaluateLabel(
                    x.SellAmount,
                    x.PaidAmount,
                    remaining,
                    saleDate,
                    x.LastInstallmentPaymentDate,
                    asOf),
            };
        }).ToList();
        
        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "الأسم الثلاثي", propertyName: "FullName"),
            new(headerText: "العميل", propertyName: "BusinessName"),
            new(headerText: "عنوان العميل", propertyName: "BusinessAddress"),
            new(headerText: "القائمة", propertyName: "OrderListName"),
            new(headerText: "عدد المبيعات", propertyName: "OrdersCount"),
            new(headerText: "اخر تسديد", propertyName: "LastInstallmentPaymentDate"),
            new(headerText: "التقييم", propertyName: "Rating"),
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("العملاء", customers, columns);

        return new GetCustomersExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Customers-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}
