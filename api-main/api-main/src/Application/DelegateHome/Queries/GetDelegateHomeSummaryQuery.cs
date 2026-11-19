using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.ComplaintAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Domain.Entities.WaiverAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.DelegateHome.Queries;

public sealed record DelegateHomeSummaryDto(
    int? UnreadComplaints,
    bool UnreadComplaintsAvailable,
    int? PendingExceptions,
    bool PendingExceptionsAvailable,
    int? UnreadWrongSales,
    bool UnreadWrongSalesAvailable,
    int? ApprovedExceptions,
    int? RejectedExceptions,
    int? CancelledExceptions,
    int? TotalExceptions,
    bool ExceptionsSummaryAvailable,
    int? PendingWaivers = null,
    int? AwaitingAccountantWaivers = null,
    int? ApprovedWaivers = null,
    int? RejectedWaivers = null,
    int? TotalWaivers = null,
    bool WaiversAvailable = false,
    int? OpenTasks = null,
    bool OpenTasksAvailable = false);

public sealed class GetDelegateHomeSummaryQuery : IRequest<Result<DelegateHomeSummaryDto>>
{
    public int? BranchId { get; init; }
}

public sealed class GetDelegateHomeSummaryHandler
    : IRequestHandler<GetDelegateHomeSummaryQuery, Result<DelegateHomeSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetDelegateHomeSummaryHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<DelegateHomeSummaryDto>> Handle(
        GetDelegateHomeSummaryQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.DelegateHome.Read) == false)
            return new Error("لا صلاحية لرئيسية المفوض.", "DelegateHome.Forbidden");

        var scope = _current.BranchIds ?? [];
        if (request.BranchId is int only)
            scope = scope.Contains(only) ? [only] : [];
        // الفشل ليس صفرًا — عند غياب النطاق نعيد null مع Available=false
        if (scope.Count == 0)
        {
            return new DelegateHomeSummaryDto(
                null, false, null, false, null, false,
                null, null, null, null, false);
        }

        int? unreadComplaints = null;
        var complaintsOk = _current.HasPermission(SeedPermissions.Complaint.Read);
        if (complaintsOk)
        {
            unreadComplaints = await Application.Complaints.Common.ComplaintScope
                .Apply(_db.ComplaintMessages.AsNoTracking(), _current.BranchIds ?? [], request.BranchId)
                .CountAsync(c => c.Status == ComplaintStatus.Unread, ct);
        }

        int? unreadWrong = null;
        var wrongOk = _current.HasPermission(SeedPermissions.WrongSale.Read);
        if (wrongOk)
        {
            unreadWrong = await _db.WrongSaleNotices.AsNoTracking()
                .CountAsync(n => scope.Contains(n.BranchId) && n.DelegateReadAtUtc == null, ct);
        }

        int? pending = null, approved = null, rejected = null, cancelled = null, total = null;
        var exOk = _current.HasPermission(SeedPermissions.SalesRequest.TabExceptions)
                   || _current.HasPermission(SeedPermissions.SalesRequest.ExceptionDecide);
        if (exOk)
        {
            var q = _db.SalesRequests.AsNoTracking()
                .Where(r => r.IsDeleted == false && scope.Contains(r.BranchId)
                            && r.ExceptionHoldStatus != null);
            pending = await q.CountAsync(r => r.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Pending, ct);
            approved = await q.CountAsync(r => r.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Approved, ct);
            rejected = await q.CountAsync(r => r.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Rejected, ct);
            cancelled = await _db.SalesRequests.AsNoTracking()
                .Where(r => r.IsDeleted == false && scope.Contains(r.BranchId) && r.ExceptionHoldStatus == null
                            && _db.SalesRequestTimelineEntries.Any(c =>
                                c.SalesRequestId == r.Id
                                && c.EventType == SalesRequestTimelineEventType.CancelledToUnassigned
                                && _db.SalesRequestTimelineEntries.Any(e =>
                                    e.SalesRequestId == r.Id
                                    && e.EventType == SalesRequestTimelineEventType.ExceptionRequested
                                    && e.OccurredAtUtc < c.OccurredAtUtc)))
                .CountAsync(ct);
            total = await q.CountAsync(ct) + cancelled;
        }

        int? wPending = null, wAwaiting = null, wApproved = null, wRejected = null, wTotal = null;
        var waiversOk = _current.HasPermission(SeedPermissions.Waiver.Read);
        if (waiversOk)
        {
            var counts = await _db.CustomerWaiverRequests.AsNoTracking()
                .Where(w => scope.Contains(w.BranchId))
                .GroupBy(w => w.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);
            int Of(WaiverRequestStatus s) => counts.Where(c => c.Status == s).Sum(c => c.Count);
            wPending = Of(WaiverRequestStatus.Pending);
            wAwaiting = Of(WaiverRequestStatus.AwaitingAccountant);
            wApproved = Of(WaiverRequestStatus.Approved);
            wRejected = Of(WaiverRequestStatus.Rejected);
            wTotal = counts.Sum(c => c.Count);
        }

        int? openTasks = null;
        var tasksOk = _current.HasPermission(SeedPermissions.ManagementTask.Read) && _current.UserId is not null;
        if (tasksOk)
        {
            var me = _current.UserId!.Value;
            openTasks = await _db.ManagementTasks.AsNoTracking()
                .CountAsync(t => t.CurrentHolderUserId == me
                                 && scope.Contains(t.BranchId)
                                 && (t.Status == ManagementTaskStatus.Sent
                                     || t.Status == ManagementTaskStatus.Accepted
                                     || t.Status == ManagementTaskStatus.AwaitingDelegatedConfirm), ct);
        }

        return new DelegateHomeSummaryDto(
            unreadComplaints, complaintsOk,
            pending, exOk,
            unreadWrong, wrongOk,
            approved, rejected, cancelled, total, exOk,
            wPending, wAwaiting, wApproved, wRejected, wTotal, waiversOk,
            openTasks, tasksOk);
    }
}
