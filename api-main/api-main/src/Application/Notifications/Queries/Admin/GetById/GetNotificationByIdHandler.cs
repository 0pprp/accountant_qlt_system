using Application.Common.Interfaces;
using Application.Notifications.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Queries.Admin.GetById;

public class GetNotificationByIdHandler : IRequestHandler<GetNotificationByIdQuery, Result<GetNotificationByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationByIdHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetNotificationByIdResponse>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Select(x => new GetNotificationByIdResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ActionType = x.ActionType,
                ActorUserFullName = x.ActorUser!.FullName,
                CreatedAt = x.CreatedAt!.Value,
                HasRead = x.HasRead
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (notification is null)
            return NotificationErrors.NotificationNotFound;

        return notification;
    }
}
