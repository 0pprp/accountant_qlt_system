using Application.Common.Interfaces;
using Application.SalesWorkflow.Common;
using Application.SalesWorkflow.Queries.App.GetBins;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesWorkflow.Queries.App.GetByStatus;

public class GetSalesWorkflowByStatusHandler : IRequestHandler<GetSalesWorkflowByStatusQuery, Result<List<SalesWorkflowItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSalesWorkflowByStatusHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<SalesWorkflowItemDto>>> Handle(GetSalesWorkflowByStatusQuery request, CancellationToken cancellationToken)
    {
        if (SalesWorkflowStatusMapper.TryParse(request.Status, out var status) == false)
            return SalesWorkflowErrors.InvalidStatus;

        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.SellerId == request.UserId)
            .Select(x => new
            {
                x.Id,
                x.SellAmount,
                x.DailyInstallmentAmount,
                CreatedAt = x.CreatedAt!.Value,
                x.WorkflowStatus,
                x.ApprovalStatus,
                x.Step,
                CustomerId = x.CustomerId,
                CustomerFullName = x.Customer!.FullName,
                CustomerPhoneNumber = x.Customer.PhoneNumber
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var items = orders
            .Select(x => new SalesWorkflowItemDto
            {
                Id = x.Id,
                Status = SalesWorkflowStatusMapper.ToApiStatus(x.WorkflowStatus, x.ApprovalStatus, x.Step),
                SellAmount = x.SellAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                CreatedAt = x.CreatedAt,
                Customer = new SalesWorkflowCustomerDto
                {
                    Id = x.CustomerId,
                    FullName = x.CustomerFullName,
                    PhoneNumber = x.CustomerPhoneNumber
                }
            })
            .Where(x => x.Status == status)
            .ToList();

        return items;
    }
}
