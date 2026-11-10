using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Queries.Admin.Timeline;

public class GetAdminSalesRequestTimelineHandler
    : IRequestHandler<GetAdminSalesRequestTimelineQuery, Result<List<AdminSalesRequestTimelineItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAdminSalesRequestTimelineHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AdminSalesRequestTimelineItem>>> Handle(
        GetAdminSalesRequestTimelineQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission(SeedPermissions.SalesRequest.TimelineRead))
            return SalesRequestErrors.TabForbidden;

        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var header = await _db.SalesRequests.AsNoTracking()
            .Where(x => x.Id == request.Id && branches.Contains(x.BranchId))
            .Select(x => new
            {
                x.Id,
                x.IsDeleted,
                x.Status,
                x.TargetEmployeeId,
                x.ExceptionHoldStatus,
                x.ExceptionConsumedAtUtc,
                x.SourceType
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
            return SalesRequestErrors.NotFound;

        var allowed = SalesRequestAdminQuery.CanReadRow(
            _currentUser,
            header.IsDeleted,
            header.Status,
            header.TargetEmployeeId,
            header.ExceptionHoldStatus,
            header.ExceptionConsumedAtUtc,
            header.SourceType);

        if (allowed == false)
            return SalesRequestErrors.TabForbidden;

        var rows = await _db.SalesRequestTimelineEntries.AsNoTracking()
            .Where(x => x.SalesRequestId == request.Id)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.EventType,
                x.ActorUserId,
                x.OccurredAtUtc,
                x.PreviousStatus,
                x.NewStatus,
                x.PreviousEmployeeId,
                x.NewEmployeeId,
                x.Reason
            })
            .ToListAsync(cancellationToken);

        var actorIds = rows.Select(x => x.ActorUserId).Distinct().ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(u => actorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return rows.Select(x => new AdminSalesRequestTimelineItem
        {
            Id = x.Id,
            EventType = x.EventType,
            EventLabel = TimelineEventLabel(x.EventType),
            ActorUserId = x.ActorUserId,
            ActorName = names.GetValueOrDefault(x.ActorUserId),
            OccurredAtUtc = x.OccurredAtUtc,
            PreviousStatus = x.PreviousStatus,
            PreviousStatusLabel = x.PreviousStatus is { } ps ? SalesRequestStatusLabels.For(ps) : null,
            NewStatus = x.NewStatus,
            NewStatusLabel = x.NewStatus is { } ns ? SalesRequestStatusLabels.For(ns) : null,
            PreviousEmployeeId = x.PreviousEmployeeId,
            NewEmployeeId = x.NewEmployeeId,
            Reason = x.Reason
        }).ToList();
    }

    private static string TimelineEventLabel(SalesRequestTimelineEventType t) => t switch
    {
        SalesRequestTimelineEventType.Assigned => "إسناد",
        SalesRequestTimelineEventType.Reassigned => "إعادة إسناد",
        SalesRequestTimelineEventType.Viewed => "اطلاع أول",
        SalesRequestTimelineEventType.ViewSeen => "اطلاع",
        SalesRequestTimelineEventType.Prepared => "تجهيز للبيع",
        SalesRequestTimelineEventType.Pend => "تعليق",
        SalesRequestTimelineEventType.Rejected => "رفض",
        SalesRequestTimelineEventType.Inspected => "كشف",
        SalesRequestTimelineEventType.DraftLinked => "ربط مسودة بيع",
        SalesRequestTimelineEventType.Completed => "إتمام البيع",
        SalesRequestTimelineEventType.ExceptionRequested => "طلب استثناء",
        SalesRequestTimelineEventType.Returned => "إعادة",
        SalesRequestTimelineEventType.ExceptionApproved => "موافقة على الاستثناء",
        SalesRequestTimelineEventType.ExceptionRejected => "رفض الاستثناء",
        SalesRequestTimelineEventType.ExceptionConsumed => "استهلاك الاستثناء المعتمد",
        SalesRequestTimelineEventType.SoftDeleted => "حذف ناعم",
        SalesRequestTimelineEventType.Created => "إرسال طلب جديد للتدقيق",
        SalesRequestTimelineEventType.Restored => "استعادة",
        _ => t.ToString()
    };
}
