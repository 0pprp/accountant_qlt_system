using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.Admin.GetOrders;

public class GetCustomerOrdersHandler : IRequestHandler<GetCustomerOrdersQuery, Result<List<GetCustomerOrdersResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerOrdersHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GetCustomerOrdersResponse>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.CustomerId == request.CustomerId)
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Where(x => x.Step == OrderStep.Completed)
            .Where(x => x.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(x => new GetCustomerOrdersResponse
            {
                Id = x.Id,
                SellAmount = x.SellAmount,
                SellerFullName = x.Seller!.FullName,
                OrderListName = x.OrderList!.Name,
                OrderItems = x.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductName = oi.ProductId != null ? oi.Product!.Name : oi.ProductName!,
                        ProductType = oi.ProductType,
                        Quantity = oi.Quantity,
                        BuyAmount = oi.BuyAmount,
                        SellAmount = oi.SellAmount
                    })
                    .ToList(),
                CreatedAt = x.CreatedAt!.Value
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders;
    }
}