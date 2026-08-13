using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Common.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAuthService _authService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;
    private readonly ISecurityService _securityService;

    public LogoutHandler(
        IAuthService authService,
        IApplicationDbContext dbContext,
        IActivityLogService activityLogService,
        ISecurityService securityService)
    {
        _authService = authService;
        _dbContext = dbContext;
        _activityLogService = activityLogService;
        _securityService = securityService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hashedRefreshToken = _securityService.GetSha256Hash(request.RefreshToken);

        var userInfo = await _dbContext.UserLogins.AsNoTracking()
            .Where(x => x.RefreshToken == hashedRefreshToken)
            .Select(x => new
            {
                UserId = x.UserId!.Value,
                x.User!.Username,
                BranchIds = x.User.UserBranches.Select(ub => ub.BranchId).ToList(),
                RoleDisplayNames = x.User.UserRoles.Select(ur => ur.Role!.DisplayName).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var result = await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        if (result.IsFailed)
            return result;

        if (userInfo is not null)
        {
            var actor = new ActivityLogActor(
                userInfo.UserId,
                userInfo.Username,
                string.Join(",", userInfo.RoleDisplayNames));

            foreach (var branchId in userInfo.BranchIds)
            {
                await _activityLogService.AddAsync(
                    branchId,
                    ActivityType.Logout,
                    TargetEntityType.User,
                    userInfo.UserId,
                    actor);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
