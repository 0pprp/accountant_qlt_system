using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Queries.Admin.ExceptionInbox;

public enum ExceptionInboxStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public record GetExceptionInboxQuery : IRequest<Result<ExceptionInboxResponse>>
{
    public ExceptionInboxStatus Status { get; init; } = ExceptionInboxStatus.Pending;
    public int? BranchId { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record ExceptionInboxRowDto(
    int Id,
    string CustomerName,
    string? CustomerPhone,
    int BranchId,
    string? BranchName,
    int? ExistingCustomerId,
    string Status,
    string RequestStatus,
    DateTimeOffset? RequestedAtUtc,
    string? RequestedByName,
    string? RequestReason,
    DateTimeOffset? DecidedAtUtc,
    string? DecidedByName,
    string? DecisionNote,
    bool IsRead,
    DateTimeOffset? ReadAtUtc);

public sealed record ExceptionInboxCountsDto(int Pending, int Approved, int Rejected, int Cancelled);

public sealed record ExceptionInboxResponse(
    IReadOnlyList<ExceptionInboxRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    ExceptionInboxCountsDto Counts);

public class GetExceptionInboxHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetExceptionInboxQuery, Result<ExceptionInboxResponse>>
{
    public async Task<Result<ExceptionInboxResponse>> Handle(GetExceptionInboxQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.HasPermission(SeedPermissions.SalesRequest.TabExceptions) == false
            && currentUser.HasPermission(SeedPermissions.SalesRequest.ExceptionDecide) == false)
            return SalesRequestErrors.ActionForbidden;

        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        if (request.BranchId is int only)
            branches = branches.Contains(only) ? new List<int> { only } : new List<int>();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var scoped = db.SalesRequests.AsNoTracking()
            .Where(x => x.IsDeleted == false && branches.Contains(x.BranchId));
        if (string.IsNullOrWhiteSpace(request.Search) == false)
        {
            var term = request.Search.Trim();
            scoped = scoped.Where(x => x.CustomerName.Contains(term)
                                       || (x.CustomerPhone != null && x.CustomerPhone.Contains(term))
                                       || (x.Branch != null && x.Branch.Name.Contains(term)));
        }

        var counts = new ExceptionInboxCountsDto(
            await ByStatus(scoped, ExceptionInboxStatus.Pending).CountAsync(cancellationToken),
            await ByStatus(scoped, ExceptionInboxStatus.Approved).CountAsync(cancellationToken),
            await ByStatus(scoped, ExceptionInboxStatus.Rejected).CountAsync(cancellationToken),
            await ByStatus(scoped, ExceptionInboxStatus.Cancelled).CountAsync(cancellationToken));

        var query = ByStatus(scoped, request.Status);
        var total = request.Status switch
        {
            ExceptionInboxStatus.Pending => counts.Pending,
            ExceptionInboxStatus.Approved => counts.Approved,
            ExceptionInboxStatus.Rejected => counts.Rejected,
            _ => counts.Cancelled
        };

        var page0 = await query
            .Select(x => new
            {
                x.Id,
                x.CustomerName,
                x.CustomerPhone,
                x.BranchId,
                x.ExistingCustomerId,
                x.ExceptionHoldStatus,
                x.Status,
                x.ExceptionDecidedAtUtc,
                x.ExceptionReadAtUtc,
                RequestedAt = db.SalesRequestTimelineEntries
                    .Where(e => e.SalesRequestId == x.Id && e.EventType == SalesRequestTimelineEventType.ExceptionRequested)
                    .Max(e => (DateTimeOffset?)e.OccurredAtUtc)
            })
            .OrderByDescending(x => x.RequestedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ids = page0.Select(r => r.Id).ToList();
        var branchIds = page0.Select(r => r.BranchId).Distinct().ToList();
        var branchNames = await db.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);
        var events = await db.SalesRequestTimelineEntries.AsNoTracking()
            .Where(e => ids.Contains(e.SalesRequestId)
                        && (e.EventType == SalesRequestTimelineEventType.ExceptionRequested
                            || e.EventType == SalesRequestTimelineEventType.ExceptionApproved
                            || e.EventType == SalesRequestTimelineEventType.ExceptionRejected
                            || e.EventType == SalesRequestTimelineEventType.CancelledToUnassigned))
            .Select(e => new { e.Id, e.SalesRequestId, e.EventType, e.OccurredAtUtc, e.ActorUserId, e.Reason })
            .ToListAsync(cancellationToken);

        var rows = page0.Select(r =>
        {
            var mine = events.Where(e => e.SalesRequestId == r.Id)
                .OrderByDescending(e => e.OccurredAtUtc).ThenByDescending(e => e.Id).ToList();
            var requested = mine.FirstOrDefault(e => e.EventType == SalesRequestTimelineEventType.ExceptionRequested);
            var decided = mine.FirstOrDefault(e => e.EventType != SalesRequestTimelineEventType.ExceptionRequested);
            return new
            {
                r.Id,
                r.CustomerName,
                r.CustomerPhone,
                r.BranchId,
                BranchName = branchNames.GetValueOrDefault(r.BranchId),
                r.ExistingCustomerId,
                r.ExceptionHoldStatus,
                r.Status,
                r.ExceptionDecidedAtUtc,
                r.ExceptionReadAtUtc,
                Requested = requested is null ? null : new { requested.OccurredAtUtc, requested.ActorUserId, requested.Reason },
                Decided = decided is null ? null : new { decided.OccurredAtUtc, decided.ActorUserId, decided.Reason }
            };
        }).ToList();

        var userIds = rows
            .SelectMany(r => new[] { r.Requested?.ActorUserId, r.Decided?.ActorUserId })
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var names = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var items = rows.Select(r =>
        {
            var status = Classify(r.ExceptionHoldStatus);
            var decided = status == ExceptionInboxStatus.Pending ? null : r.Decided;
            return new ExceptionInboxRowDto(
                r.Id,
                r.CustomerName,
                r.CustomerPhone,
                r.BranchId,
                r.BranchName,
                r.ExistingCustomerId,
                status.ToString(),
                r.Status.ToString(),
                r.Requested?.OccurredAtUtc,
                r.Requested is { ActorUserId: > 0 } q ? names.GetValueOrDefault(q.ActorUserId) : null,
                r.Requested?.Reason,
                decided?.OccurredAtUtc ?? r.ExceptionDecidedAtUtc,
                decided is { ActorUserId: > 0 } d ? names.GetValueOrDefault(d.ActorUserId) : null,
                decided?.Reason,
                r.ExceptionReadAtUtc != null || status != ExceptionInboxStatus.Pending,
                r.ExceptionReadAtUtc);
        }).ToList();

        return new ExceptionInboxResponse(items, total, page, pageSize, counts);
    }

    private static ExceptionInboxStatus Classify(SalesRequestExceptionHoldStatus? hold) => hold switch
    {
        SalesRequestExceptionHoldStatus.Pending => ExceptionInboxStatus.Pending,
        SalesRequestExceptionHoldStatus.Approved => ExceptionInboxStatus.Approved,
        SalesRequestExceptionHoldStatus.Rejected => ExceptionInboxStatus.Rejected,
        _ => ExceptionInboxStatus.Cancelled
    };

    private IQueryable<SalesRequest> ByStatus(IQueryable<SalesRequest> scoped, ExceptionInboxStatus status) => status switch
    {
        ExceptionInboxStatus.Pending => scoped.Where(x => x.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Pending),
        ExceptionInboxStatus.Approved => scoped.Where(x => x.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Approved),
        ExceptionInboxStatus.Rejected => scoped.Where(x => x.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Rejected),
        _ => scoped.Where(x => x.ExceptionHoldStatus == null
                               && db.SalesRequestTimelineEntries.Any(c =>
                                   c.SalesRequestId == x.Id
                                   && c.EventType == SalesRequestTimelineEventType.CancelledToUnassigned
                                   && db.SalesRequestTimelineEntries.Any(r =>
                                       r.SalesRequestId == x.Id
                                       && r.EventType == SalesRequestTimelineEventType.ExceptionRequested
                                       && r.OccurredAtUtc < c.OccurredAtUtc)))
    };
}
