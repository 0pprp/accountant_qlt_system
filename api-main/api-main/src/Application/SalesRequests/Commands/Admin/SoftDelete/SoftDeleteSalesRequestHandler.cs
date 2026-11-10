using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Commands.Admin.SoftDelete;

public class SoftDeleteSalesRequestHandler : IRequestHandler<SoftDeleteSalesRequestCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SoftDeleteSalesRequestHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(SoftDeleteSalesRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.SalesRequest.SoftDelete) == false)
            return SalesRequestErrors.ActionForbidden;

        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return SalesRequestErrors.ActionForbidden;

        if (string.IsNullOrWhiteSpace(request.Reason))
            return SalesRequestErrors.NoteRequired;

        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);

        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        var check = SalesRequestExtraViewRules.CheckSoftDelete(entity);
        switch (check)
        {
            case SoftDeleteCheck.AlreadyDeleted:
                return SalesRequestErrors.AlreadyDeleted;
            case SoftDeleteCheck.Completed:
                return SalesRequestErrors.DeleteCompletedForbidden;
            case SoftDeleteCheck.LinkedOrder:
                return SalesRequestErrors.DeleteLinkedOrderForbidden;
        }

        var at = _clock.UtcNowOffset;
        var reason = request.Reason.Trim();
        var prevEmployee = entity.TargetEmployeeId;

        entity.SoftDelete(reason, actorId, at);

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.SoftDeleted, actorId, at,
            entity.Status, entity.Status, prevEmployee, entity.TargetEmployeeId, reason, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.Invalidation, at, prevEmployee);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct),
            cancellationToken);
    }
}
