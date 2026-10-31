using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.SalesWorkflow.Common;
using Application.SalesWorkflow.Queries.App.GetBins;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesWorkflow.Commands.App.Transfer;

public class TransferSalesWorkflowHandler : IRequestHandler<TransferSalesWorkflowCommand, Result<SalesWorkflowItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public TransferSalesWorkflowHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SalesWorkflowItemDto>> Handle(TransferSalesWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (SalesWorkflowStatusMapper.TryParse(request.TargetStatus, out var normalized) == false)
            return SalesWorkflowErrors.InvalidStatus;

        var order = await _dbContext.Orders
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.SellerId != request.UserId)
            return OrderErrors.OrderDoesNotBelongToYou;

        var requiresReason = normalized is SalesWorkflowStatusMapper.Rejected or SalesWorkflowStatusMapper.OnHold;
        if (requiresReason && string.IsNullOrWhiteSpace(request.Reason))
            return SalesWorkflowErrors.ReasonRequired;

        var target = normalized switch
        {
            SalesWorkflowStatusMapper.Pending => SalesWorkflowStatus.Pending,
            SalesWorkflowStatusMapper.Ready => SalesWorkflowStatus.Ready,
            SalesWorkflowStatusMapper.Disclosure => SalesWorkflowStatus.Disclosure,
            SalesWorkflowStatusMapper.OnHold => SalesWorkflowStatus.OnHold,
            SalesWorkflowStatusMapper.Rejected => SalesWorkflowStatus.Rejected,
            SalesWorkflowStatusMapper.Sold => SalesWorkflowStatus.Sold,
            _ => SalesWorkflowStatus.Pending
        };

        try
        {
            order.TransferSalesWorkflow(target, request.Reason);
        }
        catch (ArgumentException)
        {
            return SalesWorkflowErrors.ReasonRequired;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SalesWorkflowItemDto
        {
            Id = order.Id,
            Status = SalesWorkflowStatusMapper.ToApiStatus(order),
            SellAmount = order.SellAmount,
            DailyInstallmentAmount = order.DailyInstallmentAmount,
            CreatedAt = order.CreatedAt!.Value,
            Reason = order.WorkflowChangeReason,
            Customer = new SalesWorkflowCustomerDto
            {
                Id = order.CustomerId,
                FullName = order.Customer!.FullName,
                PhoneNumber = order.Customer.PhoneNumber
            }
        };
    }
}
