using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Expenses.Queries.Admin.GetExcelReport;

public class GetExpensesExcelReportHandler : IRequestHandler<GetExpensesExcelReportQuery, Result<GetExpensesExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpensesExcelReportHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetExpensesExcelReportResponse>> Handle(GetExpensesExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var expenses = await _dbContext.Expenses.AsNoTracking()
            .When(request.SearchTerm is not null, x => x.FactorNumber.ToString().Contains(request.SearchTerm!))
            .When(request.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.StartDate)
            .When(request.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.EndDate)
            .Select(x => new GetExpenseExcelReportItem
            {
                Id = x.Id,
                FactorNumber = x.FactorNumber,
                TotalAmount = x.TotalAmount,
                SafeName = x.Branch!.Safe!.Name,
                ExpenseItemsCount = x.ExpenseItems.Count,
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
            new(headerText: "عدد العناصر", propertyName: "ExpenseItemsCount"),
            new(headerText: "تم إنشاؤه في", propertyName: "CreatedAt")
        };

        var byteArray = ExcelReportGenerator.GenerateExcelReport("الصرفيات", expenses, columns);

        return new GetExpensesExcelReportResponse
        {
            Data = byteArray,
            ContentType = "application/vnd.ms-excel",
            FileName = $"Expenses-{DateTime.Now:yyyy-MM-dd HH:mm:ss}.xlsx"
        };
    }
}
