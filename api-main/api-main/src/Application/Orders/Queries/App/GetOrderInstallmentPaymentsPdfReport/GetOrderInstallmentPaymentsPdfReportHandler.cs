using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Orders.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.App.GetOrderInstallmentPaymentsPdfReport;

public class GetOrderInstallmentPaymentsPdfReportHandler : IRequestHandler<GetOrderInstallmentPaymentsPdfReportQuery,
    Result<GetOrderInstallmentPaymentsPdfReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly PdfReportGenerator _pdfReportGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetOrderInstallmentPaymentsPdfReportHandler(IApplicationDbContext dbContext, PdfReportGenerator pdfReportGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _pdfReportGenerator = pdfReportGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetOrderInstallmentPaymentsPdfReportResponse>> Handle(GetOrderInstallmentPaymentsPdfReportQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.Id == request.OrderId)
            .Where(x => x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .Select(x => new
            {
                x.CreatedAt,
                x.SaleDate,
                x.ExecutionStatus,
                InstallmentPayments = x.InstallmentPayments.Select(ip => new
                {
                    ip.Id,
                    ip.Date,
                    ip.Amount,
                    ip.Description
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return OrderErrors.OrderNotFound;

        var startDate = order.SaleDate!.Value;
        var endDate = order.ExecutionStatus == OrderExecutionStatus.Completed
            ? order.InstallmentPayments.Max(ip => ip.Date)
            : _dateTimeProvider.Today;

        var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
        var installmentPayments = Enumerable.Range(0, totalDays)
            .Select(dayOffset =>
            {
                var date = startDate.AddDays(dayOffset);
                var payment = order.InstallmentPayments.FirstOrDefault(ip => ip.Date == date);
                return new GetOrderInstallmentPaymentsPdfReportItem
                {
                    Id = payment?.Id,
                    Date = date.ToString("yyyy-MM-dd"),
                    Amount = payment?.Amount,
                    Status = payment is not null ? "مسدد" : "غير مسدد",
                    Description = payment?.Description
                };
            })
            .OrderByDescending(ip => ip.Date)
            .ToList();

        var columns = new List<PdfTableColumn>
        {
            new("المعرّف", "Id"),
            new("التاریخ", "Date"),
            new("المبلغ", "Amount"),
            new("الحالة", "Status"),
            new("ملاحظة", "Description")
        };

        var byteArray = _pdfReportGenerator.GeneratePdfReport("تسدیدات", installmentPayments, columns, request.PdfSettings);

        return new GetOrderInstallmentPaymentsPdfReportResponse
        {
            Data = byteArray,
            ContentType = "application/pdf",
            FileName = $"Order-{request.OrderId}-InstallmentPayments-{DateTime.Now:yyyy/MM/dd HH:mm:ss}.pdf"
        };
    }
}