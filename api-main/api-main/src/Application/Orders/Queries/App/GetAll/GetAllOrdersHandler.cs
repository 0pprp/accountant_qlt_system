using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.App.GetAll;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, Result<List<GetAllOrdersResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetAllOrdersHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<List<GetAllOrdersResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.SellerId == request.UserId || x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .Where(x => x.ApprovalStatus != OrderApprovalStatus.Rejected)
            .When(request.Filter.FullName is not null, x => x.Customer!.FullName.Contains(request.Filter.FullName!))
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .When(request.Filter.OrderListId is not null, x => x.OrderListId == request.Filter.OrderListId)
            .When(request.Filter.ExecutionStatuses is not null,x => request.Filter.ExecutionStatuses!.Contains(x.ExecutionStatus))
            .Select(x => new GetAllOrdersResponse
            {
                Id = x.Id,
                SellAmount = x.SellAmount,
                PaidAmount = x.InstallmentPayments.Sum(ip => ip.Amount),
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                CreatedAt = x.CreatedAt!.Value,
                CreationAddress = x.CreationAddress,
                Step = x.Step,
                ApprovalStatus = x.ApprovalStatus,
                OrderListId = x.OrderListId,
                Customer = new CustomerDto
                {
                    Id = x.CustomerId,
                    FullName = x.Customer!.FullName,
                    PhoneNumber = x.Customer.PhoneNumber
                },
                InstallmentPaymentId = x.InstallmentPayments
                    .Where(ip => ip.Date == _dateTimeProvider.Today)
                    .Select(ip => (int?)ip.Id)
                    .FirstOrDefault(),
                OrderItems = x.OrderItems
                    .Select(oi => new GetMinimalOrderItemDto
                    {
                        Id = oi.Id,
                        ProductName = oi.ProductName ?? oi.Product!.Name,
                        Quantity = oi.Quantity,
                        SellAmount = oi.SellAmount
                    })
                    .ToList()
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders;
    }
}