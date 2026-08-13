using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Queries.Admin.GetUnreadCount;

public class GetUnreadNotificationsCountHandler : IRequestHandler<GetUnreadNotificationsCountQuery, Result<GetUnreadNotificationsCountResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationsCountHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetUnreadNotificationsCountResponse>> Handle(GetUnreadNotificationsCountQuery request,
        CancellationToken cancellationToken)
    {
        var unreadCount = await _dbContext.Notifications.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Where(x => x.BranchId == request.BranchId)
            .Where(x => x.HasRead == false)
            .CountAsync(cancellationToken);

        return new GetUnreadNotificationsCountResponse { UnreadCount = unreadCount };
    }
}
