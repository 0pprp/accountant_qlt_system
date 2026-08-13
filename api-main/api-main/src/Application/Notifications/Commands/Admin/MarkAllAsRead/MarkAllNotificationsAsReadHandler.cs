using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Commands.Admin.MarkAllAsRead;

public class MarkAllNotificationsAsReadHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<MarkAllNotificationsAsReadResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public MarkAllNotificationsAsReadHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<MarkAllNotificationsAsReadResponse>> Handle(MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var updatedCount = await _dbContext.Notifications
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Where(x => x.BranchId == request.BranchId)
            .Where(x => x.HasRead == false)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.HasRead, true), cancellationToken);

        await _activityLogService.AddAsync(
            request.BranchId,
            ActivityType.AllNotificationsMarkedAsRead,
            TargetEntityType.Notification,
            null);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MarkAllNotificationsAsReadResponse { UpdatedCount = updatedCount };
    }
}
