using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Utilities;
using Domain.Entities.ActivityLogAggregate;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityLogService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task AddAsync(
        int branchId,
        ActivityType activityType,
        TargetEntityType targetEntityType,
        int? targetEntityId,
        ActivityLogActor? actorOverride = null)
    {
        var userId = actorOverride?.UserId ?? _currentUserService.UserId!.Value;
        var (userName, userRoles) = await ResolveActorAsync(userId, actorOverride);

        var requestInfo = _httpContextAccessor.HttpContext?.Request is { } request
            ? HttpRequestInfoExtractor.Extract(request)
            : new HttpRequestInfoExtractor.HttpRequestInfo();

        var activityLog = new ActivityLog(
            activityType,
            activityType.GetDisplayName(),
            userName,
            userRoles,
            targetEntityType,
            targetEntityId,
            requestInfo.IpAddress,
            requestInfo.UserAgent,
            requestInfo.DeviceType,
            requestInfo.Browser,
            requestInfo.OperatingSystem,
            branchId,
            userId);

        _dbContext.ActivityLogs.Add(activityLog);
    }

    private async Task<(string UserName, string UserRoles)> ResolveActorAsync(int userId, ActivityLogActor? actorOverride)
    {
        if (actorOverride is not null)
            return (actorOverride.UserName, actorOverride.UserRoles);

        var actor = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Username,
                RoleDisplayNames = x.UserRoles.Select(ur => ur.Role!.DisplayName).ToList()
            })
            .FirstAsync();

        return (actor.Username, string.Join(",", actor.RoleDisplayNames));
    }
}
