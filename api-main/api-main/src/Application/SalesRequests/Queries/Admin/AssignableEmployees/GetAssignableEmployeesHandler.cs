using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.AssignableEmployees;

public class GetAssignableEmployeesHandler
    : IRequestHandler<GetAssignableEmployeesQuery, Result<List<AssignableEmployeeItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetAssignableEmployeesHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<List<AssignableEmployeeItem>>> Handle(
        GetAssignableEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        if (!SalesRequestAccess.CanAccessBranch(_currentUser, request.BranchId))
            return SalesRequestErrors.BranchForbidden;

        var now = _clock.UtcNowOffset;
        var query = SalesRequestEmployeeEligibility.QueryEligibleAssignees(
            _db, request.BranchId, now);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                (x.FullName != null && x.FullName.Contains(term))
                || (x.Username != null && x.Username.Contains(term)));
        }

        return await query
            .OrderBy(x => x.FullName)
            .Take(50)
            .Select(x => new AssignableEmployeeItem
            {
                Id = x.Id,
                FullName = x.FullName ?? x.Username ?? $"#{x.Id}",
                Username = x.Username
            })
            .ToListAsync(cancellationToken);
    }
}
