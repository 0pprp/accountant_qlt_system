using Application.Common.Interfaces;
using Application.SalesWorkflow.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesWorkflow.Queries.App.GetBins;

public class GetSalesWorkflowBinsHandler : IRequestHandler<GetSalesWorkflowBinsQuery, Result<GetSalesWorkflowBinsResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSalesWorkflowBinsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetSalesWorkflowBinsResponse>> Handle(GetSalesWorkflowBinsQuery request, CancellationToken cancellationToken)
    {
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

        var items = orders.Select(x => new SalesWorkflowItemDto
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
        }).ToList();

        var bins = SalesWorkflowStatusMapper.All
            .Select(status => new SalesWorkflowBinDto
            {
                Status = status,
                Items = items.Where(i => i.Status == status).ToList(),
                Count = items.Count(i => i.Status == status)
            })
            .ToList();

        return new GetSalesWorkflowBinsResponse { Bins = bins };
    }
}
