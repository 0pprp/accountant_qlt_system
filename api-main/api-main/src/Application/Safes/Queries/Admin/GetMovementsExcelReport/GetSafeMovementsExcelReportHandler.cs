using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Safes.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetMovementsExcelReport;

public class GetSafeMovementsExcelReportHandler
    : IRequestHandler<GetSafeMovementsExcelReportQuery, Result<GetSafeMovementsExcelReportResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetSafeMovementsExcelReportHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<GetSafeMovementsExcelReportResponse>> Handle(
        GetSafeMovementsExcelReportQuery request, CancellationToken cancellationToken)
    {
        var scope = await SafeAccess.EnsureSafeInScopeAsync(
            _dbContext, _currentUser, request.SafeId, cancellationToken);
        if (scope.IsFailed)
            return scope.Error;

        var fromUtc = request.Filter.StartDate is DateOnly start
            ? new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue).AddHours(-3), TimeSpan.Zero)
            : (DateTimeOffset?)null;

        var toUtc = request.Filter.EndDate is DateOnly end
            ? new DateTimeOffset(end.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-3), TimeSpan.Zero)
            : (DateTimeOffset?)null;

        // الترجمة العربية للـenum تُنفَّذ في الذاكرة لأنها غير قابلة للترجمة إلى SQL
        var raw = await _dbContext.SafeMovements.AsNoTracking()
            .Where(x => x.SafeId == request.SafeId)
            .When(request.Filter.Kind is not null, x => x.Kind == request.Filter.Kind)
            .When(fromUtc is not null, x => x.CreatedAt >= fromUtc)
            .When(toUtc is not null, x => x.CreatedAt < toUtc)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Kind,
                x.Amount,
                x.BalanceAfter,
                CounterpartySafeName = x.CounterpartySafe != null ? x.CounterpartySafe.Name : null,
                x.Note,
                PerformedByFullName = _dbContext.Users
                    .Where(u => u.Id == x.PerformedByUserId)
                    .Select(u => u.FullName)
                    .FirstOrDefault(),
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var rows = raw
            .Select(x => new GetSafeMovementExcelReportItem
            {
                Id = x.Id,
                Kind = x.Kind.ToArabicString(),
                Amount = x.Amount,
                BalanceAfter = x.BalanceAfter,
                CounterpartySafeName = x.CounterpartySafeName,
                Note = x.Note,
                PerformedByFullName = x.PerformedByFullName,
                CreatedAt = x.CreatedAt?.AddHours(3).ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        var columns = new List<ExcelColumn>
        {
            new(headerText: "المعرف", propertyName: "Id"),
            new(headerText: "نوع الحركة", propertyName: "Kind"),
            new(headerText: "المبلغ", propertyName: "Amount"),
            new(headerText: "الرصيد بعدها", propertyName: "BalanceAfter"),
            new(headerText: "الخزينة المقابلة", propertyName: "CounterpartySafeName"),
            new(headerText: "ملاحظات", propertyName: "Note"),
            new(headerText: "المنفّذ", propertyName: "PerformedByFullName"),
            new(headerText: "التاريخ", propertyName: "CreatedAt")
        };

        var data = ExcelReportGenerator.GenerateExcelReport("حركات الخزينة", rows, columns);

        return new GetSafeMovementsExcelReportResponse
        {
            Data = data,
            ContentType = "application/vnd.ms-excel",
            FileName = $"SafeMovements-{DateTime.UtcNow.AddHours(3):yyyy-MM-dd-HH-mm}.xlsx"
        };
    }
}
