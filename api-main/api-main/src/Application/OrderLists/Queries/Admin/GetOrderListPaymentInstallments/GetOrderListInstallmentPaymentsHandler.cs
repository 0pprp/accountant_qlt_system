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
            .When(request.OrderListId > 0, x => x.Order!.OrderListId == request.OrderListId)
            .When(request.Filter.StartDate is not null, x => x.Date >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => x.Date <= request.Filter.EndDate)
            .When(request.Filter.SearchTerm is not null, x => x.Order!.Customer!.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              _dbContext.Users.Any(u => u.Id == x.CreatedBy
                                                                                        && (x.ClientPaymentId != null || u.Id == x.Order.OrderList!.MandobId || u.Id == x.Order.OrderList.MotabaId)
                                                                                        && u.FullName.Contains(request.Filter.SearchTerm!)) ||
                                                              (x.Order.OrderList!.Mandob != null && x.Order.OrderList.Mandob.FullName.Contains(request.Filter.SearchTerm!)))
            .Select(x => new GetOrderListInstallmentPaymentsResponse
            {
                Id = x.Id,
                CustomerId = x.Order!.CustomerId,
                CustomerFullName = x.Order!.Customer!.FullName,
                SellerFullName = (x.ClientPaymentId != null || x.CreatedBy == x.Order.OrderList!.MandobId || x.CreatedBy == x.Order.OrderList.MotabaId
                        ? _dbContext.Users.Where(u => u.Id == x.CreatedBy).Select(u => u.FullName).FirstOrDefault()
                        : null)
                    ?? (x.Order.OrderList!.Mandob != null ? x.Order.OrderList.Mandob.FullName : null),
                OrderListName = x.Order.OrderList!.Name,
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