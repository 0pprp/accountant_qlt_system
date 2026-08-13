using Application.ActivityLogs.Common;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.ActivityLogs.Queries.Admin.GetById;

public class GetActivityLogByIdHandler : IRequestHandler<GetActivityLogByIdQuery, Result<GetActivityLogByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetActivityLogByIdHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetActivityLogByIdResponse>> Handle(GetActivityLogByIdQuery request, CancellationToken cancellationToken)
    {
        var activityLog = await _dbContext.ActivityLogs.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Select(x => new GetActivityLogByIdResponse
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                UserName = x.UserName,
                UserRoles = x.UserRoles,
                TargetEntityType = x.TargetEntityType,
                TargetEntityId = x.TargetEntityId,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                DeviceType = x.DeviceType,
                Browser = x.Browser,
                OperatingSystem = x.OperatingSystem,
                BranchId = x.BranchId,
                UserId = x.UserId,
                CreatedAt = x.CreatedAt!.Value
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (activityLog is null)
            return ActivityLogErrors.ActivityLogNotFound;

        return activityLog;
    }
}
