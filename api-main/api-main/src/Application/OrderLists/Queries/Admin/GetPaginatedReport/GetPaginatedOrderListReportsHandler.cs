using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.OrderLists.Queries.Admin.GetPaginated;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Queries.Admin.GetPaginatedReport;

public class GetPaginatedOrderListReportsHandler : IRequestHandler<GetPaginatedOrderListReportsQuery,
    Result<GetPaginatedOrderListReportsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetPaginatedOrderListReportsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetPaginatedOrderListReportsResponse>> Handle(GetPaginatedOrderListReportsQuery request,
        CancellationToken cancellationToken)
    {
        var orderListReports = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.Orders.Any())
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!) ||
                                                              x.Mandob!.FullName.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetPaginatedOrderListReportItem
            {
                Id = x.Id,
                Name = x.Name,
                InstallmentPaymentsCount = x.Orders
                    .SelectMany(o => o.InstallmentPayments)
                    .Count(ip => (request.Filter.StartDate == null || ip.Date >= request.Filter.StartDate) &&
                                 (request.Filter.EndDate == null || ip.Date <= request.Filter.EndDate)),
                TotalPaidInstallmentsAmount = x.Orders
                    .SelectMany(o => o.InstallmentPayments
                        .Where(ip => (request.Filter.StartDate == null || ip.Date >= request.Filter.StartDate) &&
                                     (request.Filter.EndDate == null || ip.Date <= request.Filter.EndDate))
                        .Select(ip => ip.Amount))
                    .Sum(),
                Mandob = new GetUserDto
                {
                    Id = x.MandobId,
                    FullName = x.Mandob!.FullName
                },
                CustomersCount = x.Orders.Select(o => o.CustomerId).Distinct().Count(),
                CreatedAt = x.CreatedAt!.Value
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return new GetPaginatedOrderListReportsResponse
        {
            Items = orderListReports,
            TotalInstallmentPaymentCount = await _dbContext.InstallmentPayments
                .Where(x => x.Order!.Step == OrderStep.Completed)
                .Where(x => x.Order!.BranchId == request.Filter.BranchId)
                .When(request.Filter.StartDate is not null, ip => ip.Date >= request.Filter.StartDate)
                .When(request.Filter.EndDate is not null, ip => ip.Date <= request.Filter.EndDate)
                .CountAsync(cancellationToken),
            TotalCollectedAmount = await _dbContext.InstallmentPayments
                .Where(x => x.Order!.Step == OrderStep.Completed)
                .Where(x => x.Order!.BranchId == request.Filter.BranchId)
                .When(request.Filter.StartDate is not null, ip => ip.Date >= request.Filter.StartDate)
                .When(request.Filter.EndDate is not null, ip => ip.Date <= request.Filter.EndDate)
                .SumAsync(x => x.Amount, cancellationToken)
        };
    }
}