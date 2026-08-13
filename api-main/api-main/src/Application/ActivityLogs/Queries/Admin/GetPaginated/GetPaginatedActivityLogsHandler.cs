using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.ActivityLogs.Queries.Admin.GetPaginated;

public class GetPaginatedActivityLogsHandler : IRequestHandler<GetPaginatedActivityLogsQuery,
    Result<PaginatedList<GetPaginatedActivityLogsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPaginatedActivityLogsHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<GetPaginatedActivityLogsResponse>>> Handle(GetPaginatedActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        var activityLogs = await _dbContext.ActivityLogs.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.ActivityType is not null, x => x.ActivityType == request.Filter.ActivityType)
            .When(request.Filter.TargetEntityType is not null, x => x.TargetEntityType == request.Filter.TargetEntityType)
            .When(request.Filter.UserId is not null, x => x.UserId == request.Filter.UserId)
            .When(request.Filter.SearchTerm is not null,
                x => x.Description.Contains(request.Filter.SearchTerm!) || x.UserName.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null,
                x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null,
                x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new GetPaginatedActivityLogsResponse
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                UserName = x.UserName,
                UserRoles = x.UserRoles,
                TargetEntityType = x.TargetEntityType,
                TargetEntityId = x.TargetEntityId,
                CreatedAt = x.CreatedAt!.Value
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return activityLogs;
    }
}
