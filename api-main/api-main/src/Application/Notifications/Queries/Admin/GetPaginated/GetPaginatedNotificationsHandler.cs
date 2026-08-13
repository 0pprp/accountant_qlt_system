using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Queries.Admin.GetPaginated;

public class GetPaginatedNotificationsHandler : IRequestHandler<GetPaginatedNotificationsQuery,
    Result<PaginatedList<GetPaginatedNotificationsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPaginatedNotificationsHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<GetPaginatedNotificationsResponse>>> Handle(GetPaginatedNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.Notifications.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.HasRead is not null, x => x.HasRead == request.Filter.HasRead)
            .When(request.Filter.ActionType is not null, x => x.ActionType == request.Filter.ActionType)
            .When(request.Filter.SearchTerm is not null,
                x => x.Title.Contains(request.Filter.SearchTerm!) || x.Description.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null,
                x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null,
                x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new GetPaginatedNotificationsResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ActionType = x.ActionType,
                ActorUserFullName = x.ActorUser!.FullName,
                CreatedAt = x.CreatedAt!.Value,
                HasRead = x.HasRead
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return notifications;
    }
}
