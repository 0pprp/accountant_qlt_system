using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.AttachmentAggregate.Enums;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands.App.CompleteAttachments;

public class CompleteOrderAttachmentsHandler : IRequestHandler<CompleteOrderAttachmentsCommand, Result>
{
    private static readonly HashSet<AttachmentType> RequiredAttachmentTypes =
    [
        AttachmentType.PurchaseReceipt,
        AttachmentType.TrustReceipt,
        AttachmentType.SaleContract
    ];

    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public CompleteOrderAttachmentsHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CompleteOrderAttachmentsCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.SellerId != request.UserId)
            return OrderErrors.OrderDoesNotBelongToYou;

        if (order.Step != OrderStep.Attachments)
            return OrderErrors.AttachmentsStepAlreadyCompleted;

        if (order.Attachments.Count != RequiredAttachmentTypes.Count)
            return OrderErrors.RequiredOrderAttachmentsAreMissing;

        if (order.Attachments.Select(x => x.Type).ToHashSet().SetEquals(RequiredAttachmentTypes) == false)
            return OrderErrors.RequiredOrderAttachmentsAreMissing;

        order.CompleteAttachmentsStep();

        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.OrderAttachmentsCompleted,
            TargetEntityType.Order,
            request.OrderId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
