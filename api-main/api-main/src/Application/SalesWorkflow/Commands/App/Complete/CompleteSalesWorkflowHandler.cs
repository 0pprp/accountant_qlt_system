using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.SalesWorkflow.Common;
using Application.SalesWorkflow.Queries.App.GetBins;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesWorkflow.Commands.App.Complete;

public class CompleteSalesWorkflowHandler : IRequestHandler<CompleteSalesWorkflowCommand, Result<SalesWorkflowItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public CompleteSalesWorkflowHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SalesWorkflowItemDto>> Handle(CompleteSalesWorkflowCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.SellerId != request.UserId)
            return OrderErrors.OrderDoesNotBelongToYou;

        if (order.ApprovalStatus == OrderApprovalStatus.Rejected || order.WorkflowStatus == SalesWorkflowStatus.Rejected)
            return SalesWorkflowErrors.CannotComplete;

        order.CompleteSalesWorkflow();
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
