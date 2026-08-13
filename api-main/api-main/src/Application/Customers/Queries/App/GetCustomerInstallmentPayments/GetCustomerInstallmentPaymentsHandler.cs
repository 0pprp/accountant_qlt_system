using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.App.GetCustomerInstallmentPayments;

public class GetCustomerInstallmentPaymentsHandler : IRequestHandler<GetCustomerInstallmentPaymentsQuery,
    Result<List<GetCustomerInstallmentPaymentsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCustomerInstallmentPaymentsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<GetCustomerInstallmentPaymentsResponse>>> Handle(GetCustomerInstallmentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var installmentPayments = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.CustomerId == request.CustomerId)
            .Where(x => x.Order!.OrderList!.MandobId == request.UserId || x.Order.OrderList.MotabaId == request.UserId)
            .Select(x => new GetCustomerInstallmentPaymentsResponse
            {
                Id = x.Id,
                CustomerFullName = x.Order!.Customer!.FullName,
                Amount = x.Amount,
                Date = x.Date,
                LastUpdatedAt = x.LastUpdatedAt!.Value,
                OrderId = x.OrderId
            })
            .OrderByDescending(x => x.LastUpdatedAt)
            .ToListAsync(cancellationToken);

        return installmentPayments;
    }
}