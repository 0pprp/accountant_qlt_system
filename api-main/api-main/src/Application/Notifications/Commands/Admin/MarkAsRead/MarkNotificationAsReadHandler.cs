using Application.Common.Interfaces;
using Application.Notifications.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Commands.Admin.MarkAsRead;

public class MarkNotificationAsReadHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public MarkNotificationAsReadHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (notification is null)
            return NotificationErrors.NotificationNotFound;

        notification.MarkAsRead();

        await _activityLogService.AddAsync(
            notification.BranchId,
            ActivityType.NotificationMarkedAsRead,
            TargetEntityType.Notification,
            request.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
