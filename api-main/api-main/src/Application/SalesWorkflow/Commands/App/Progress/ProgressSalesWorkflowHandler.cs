using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.SalesWorkflow.Common;
using Application.SalesWorkflow.Queries.App.GetBins;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesWorkflow.Commands.App.Progress;

public class ProgressSalesWorkflowHandler : IRequestHandler<ProgressSalesWorkflowCommand, Result<SalesWorkflowItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ProgressSalesWorkflowHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SalesWorkflowItemDto>> Handle(ProgressSalesWorkflowCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.SellerId != request.UserId)
            return OrderErrors.OrderDoesNotBelongToYou;

        if (order.ApprovalStatus == OrderApprovalStatus.Rejected ||
            order.WorkflowStatus is SalesWorkflowStatus.Rejected or SalesWorkflowStatus.Sold ||
            order.Step == OrderStep.Completed)
            return SalesWorkflowErrors.CannotProgress;

        order.ProgressSalesWorkflow();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SalesWorkflowItemDto
        {
            Id = order.Id,
            Status = SalesWorkflowStatusMapper.ToApiStatus(order),
            SellAmount = order.SellAmount,
            DailyInstallmentAmount = order.DailyInstallmentAmount,
            CreatedAt = order.CreatedAt!.Value,
            Customer = new SalesWorkflowCustomerDto
            {
                Id = order.CustomerId,
                FullName = order.Customer!.FullName,
                PhoneNumber = order.Customer.PhoneNumber
            }
        };
    }
}
