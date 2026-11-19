using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.Admin.List;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.SalesRequestAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Queries.Admin.GetById;

public class GetAdminSalesRequestByIdHandler
    : IRequestHandler<GetAdminSalesRequestByIdQuery, Result<AdminSalesRequestListItem>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAdminSalesRequestByIdHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<AdminSalesRequestListItem>> Handle(
        GetAdminSalesRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var row = await _db.SalesRequests.AsNoTracking()
            .Where(x => x.Id == request.Id && branches.Contains(x.BranchId))
            .Select(AdminSalesRequestRowMapper.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return SalesRequestErrors.NotFound;

        var allowed = SalesRequestAdminQuery.CanReadRow(
            _currentUser,
            row.IsDeleted,
            row.Status,
            row.TargetEmployeeId,
            row.ExceptionHoldStatus,
            row.ExceptionConsumedAtUtc,
            row.SourceType);

        if (allowed == false && await IsExceptionReaderAsync(row.Id, row.ExceptionHoldStatus, cancellationToken) == false)
            return SalesRequestErrors.TabForbidden;

        return AdminSalesRequestRowMapper.ToItem(row);
    }

    private async Task<bool> IsExceptionReaderAsync(int requestId, SalesRequestExceptionHoldStatus? hold, CancellationToken ct)
    {
        if (_currentUser.HasPermission(SeedPermissions.SalesRequest.TabExceptions) == false
            && _currentUser.HasPermission(SeedPermissions.SalesRequest.ExceptionDecide) == false)
            return false;
        if (hold is not null)
            return true;
        return await _db.SalesRequestTimelineEntries.AsNoTracking()
            .AnyAsync(e => e.SalesRequestId == requestId && e.EventType == SalesRequestTimelineEventType.ExceptionRequested, ct);
    }
}
