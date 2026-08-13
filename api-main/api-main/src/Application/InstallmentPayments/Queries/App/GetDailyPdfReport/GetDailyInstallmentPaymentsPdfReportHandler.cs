using Application.Common.Interfaces;
using Application.Common.Utilities;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Queries.App.GetDailyPdfReport;

public class GetDailyInstallmentPaymentsPdfReportHandler : IRequestHandler<GetDailyInstallmentPaymentsPdfReportQuery,
    Result<GetDailyInstallmentPaymentsPdfReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly PdfReportGenerator _pdfReportGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDailyInstallmentPaymentsPdfReportHandler(IApplicationDbContext dbContext, PdfReportGenerator pdfReportGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _pdfReportGenerator = pdfReportGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetDailyInstallmentPaymentsPdfReportResponse>> Handle(GetDailyInstallmentPaymentsPdfReportQuery request,
        CancellationToken cancellationToken)
    {
        var ordersWithPayments = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .Where(x => x.ExecutionStatus == OrderExecutionStatus.InProgress)
            .Select(x => new
            {
                CustomerFullName = x.Customer!.FullName,
                x.DailyInstallmentAmount,
                TodayPayment = x.InstallmentPayments.FirstOrDefault(ip => ip.Date == _dateTimeProvider.Today)
            })
            .ToListAsync(cancellationToken);

        List<GetDailyInstallmentPaymentsReportItem> installmentPayments;

        if (request.ShowPaidInstallments)
        {
            installmentPayments = ordersWithPayments
                .Where(x => x.TodayPayment != null)
                .Select(x => new GetDailyInstallmentPaymentsReportItem
                {
                    Id = x.TodayPayment!.Id,
                    CustomerFullName = x.CustomerFullName,
                    Amount = x.TodayPayment.Amount,
                    Date = x.TodayPayment.Date.ToString("yyyy-MM-dd"),
                    Status = "مسدد",
                    Description = x.TodayPayment.Description
                })
                .ToList();
        }
        else
        {
            installmentPayments = ordersWithPayments
                .Where(x => x.TodayPayment == null)
                .Select(x => new GetDailyInstallmentPaymentsReportItem
                {
                    Id = null,
                    CustomerFullName = x.CustomerFullName,
                    Amount = x.DailyInstallmentAmount,
                    Date = _dateTimeProvider.Today.ToString("yyyy-MM-dd"),
                    Status = "غير مسدد"
                })
                .ToList();
        }

        var columns = new List<PdfTableColumn>
        {
            new("المعرّف", "Id"),
            new("اسم العمیل", "CustomerFullName"),
            new("المبلغ", "Amount"),
            new("التاريخ", "Date"),
            new("الحالة", "Status"),
            new("ملاحظة", "Description")
        };

        var byteArray = _pdfReportGenerator.GeneratePdfReport("تسدیدات", installmentPayments, columns, request.PdfSettings);

        return new GetDailyInstallmentPaymentsPdfReportResponse
        {
            Data = byteArray,
            ContentType = "application/pdf",
            FileName = $"InstallmentPayments-{DateTime.Now:yyyy/MM/dd HH:mm:ss}.pdf"
        };
    }
}