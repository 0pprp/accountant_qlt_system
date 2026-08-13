using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Queries.Admin.GetOrderListPaymentInstallments;

public class GetOrderListInstallmentPaymentsHandler : IRequestHandler<GetOrderListInstallmentPaymentsQuery,
    Result<PaginatedList<GetOrderListInstallmentPaymentsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetOrderListInstallmentPaymentsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PaginatedList<GetOrderListInstallmentPaymentsResponse>>> Handle(GetOrderListInstallmentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var installmentPayments = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.OrderList!.BranchId == request.Filter.BranchId)
            .Where(x => x.Order!.OrderListId == request.OrderListId)
            .When(request.Filter.StartDate is not null, x => x.Date >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => x.Date <= request.Filter.EndDate)
            .When(request.Filter.SearchTerm is not null, x => x.Order!.Customer!.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Order.Seller!.FullName.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetOrderListInstallmentPaymentsResponse
            {
                Id = x.Id,
                CustomerFullName = x.Order!.Customer!.FullName,
                Date = x.Date,
                Amount = x.Amount,
                Description = x.Description,
                DailyInstallmentAmount = x.Order.DailyInstallmentAmount,
                CreatedAt = x.CreatedAt!.Value
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return installmentPayments;
    }
}