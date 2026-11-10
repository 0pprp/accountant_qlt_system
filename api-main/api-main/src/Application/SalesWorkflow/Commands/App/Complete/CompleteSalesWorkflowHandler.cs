using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Application.SalesWorkflow.Common;
using Application.SalesWorkflow.Queries.App.GetBins;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesWorkflow.Commands.App.Complete;

public class CompleteSalesWorkflowHandler : IRequestHandler<CompleteSalesWorkflowCommand, Result<SalesWorkflowItemDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _clock;

    public CompleteSalesWorkflowHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    {
        _dbContext = dbContext;
        _clock = clock;
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

        order.CompleteSalesWorkflow(_clock.UtcNowOffset);

        // نفس المعاملة: إن وُجد طلب مربوط بهذا Order أكمله — لا sold عام يتجاوز قواعد الإكمال
        var linkedRequest = await _dbContext.SalesRequests
            .FirstOrDefaultAsync(x => x.LinkedOrderId == order.Id && !x.IsDeleted, cancellationToken);
        if (linkedRequest is not null && linkedRequest.Status != SalesRequestStatus.Completed)
        {
            var at = _clock.UtcNowOffset;
            var prev = linkedRequest.Status;
            linkedRequest.CompleteFromSale(at);
            SalesRequestTimelineWriter.Append(
                _dbContext, linkedRequest.Id, SalesRequestTimelineEventType.Completed,
                request.UserId, at, prev, linkedRequest.Status,
                linkedRequest.TargetEmployeeId, linkedRequest.TargetEmployeeId, null, Guid.NewGuid());
            SalesRequestChangeOutboxWriter.Append(
                _dbContext, linkedRequest, SalesRequestChangeType.Completed, at);
        }

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
