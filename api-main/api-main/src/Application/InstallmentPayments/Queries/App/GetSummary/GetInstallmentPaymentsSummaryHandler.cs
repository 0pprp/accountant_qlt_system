using Application.Common.Extensions;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Queries.App.GetSummary;

public class GetInstallmentPaymentsSummaryHandler : IRequestHandler<GetInstallmentPaymentsSummaryQuery,
    Result<GetInstallmentPaymentsSummaryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetInstallmentPaymentsSummaryHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetInstallmentPaymentsSummaryResponse>> Handle(GetInstallmentPaymentsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.Filter.StartDate ?? _dateTimeProvider.Today;
        var to = request.Filter.EndDate ?? _dateTimeProvider.Today;

        var installmentPaymentsSummary = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.OrderList!.MandobId == request.UserId || x.Order.OrderList.MotabaId == request.UserId)
            .Where(x => x.Date >= from)
            .Where(x => x.Date <= to)
            .When(request.Filter.FullName is not null, x => x.Order!.Customer!.FullName.Contains(request.Filter.FullName!))
            .When(request.Filter.OrderListId is not null, x => x.Order!.OrderListId == request.Filter.OrderListId)
            .GroupBy(x => 1)
            .Select(x => new GetInstallmentPaymentsSummaryResponse
            {
                TotalCount = x.Count(),
                TotalAmount = x.Sum(p => p.Amount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return installmentPaymentsSummary ?? new GetInstallmentPaymentsSummaryResponse
        {
            TotalCount = 0,
            TotalAmount = 0
        };
    }
}