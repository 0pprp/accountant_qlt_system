using Application.Common.Interfaces;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Common;

/// <summary>ترشيح نطاق إداري على السيرفر قبل الـpagination.</summary>
public static class SalesRequestAdminQuery
{
    public static IQueryable<SalesRequest> BaseInBranches(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branchIds)
        => db.SalesRequests.AsNoTracking()
            .Where(x => !x.IsDeleted && branchIds.Contains(x.BranchId));

    /// <summary>
    /// يطبّق نطاق التبويب المصرّح. «الكل» = اتحاد التبويبات المسموح بها فقط.
    /// </summary>
    public static Result<TabScope> ApplyTab(
        IApplicationDbContext db,
        ICurrentUserService user,
        IReadOnlyCollection<int> branches,
        string? tab)
    {
        var key = SalesRequestTabMap.Normalize(tab);

        if (SalesRequestTabMap.IsExtraNotImplemented(key))
            return SalesRequestErrors.TabNotImplemented;

        if (SalesRequestTabMap.IsExtraImplemented(key))
        {
            if (!SalesRequestTabPermissions.CanReadTab(user, key))
                return SalesRequestErrors.TabForbidden;

            return new TabScope(ExtraViewQuery(db, branches, key), [key]);
        }

        if (SalesRequestTabMap.IsAllTab(key))
        {
            var allowed = SalesRequestTabPermissions.AllowedWorkflowTabs(user);
            if (allowed.Count == 0)
                return SalesRequestErrors.TabForbidden;

            var union = ApplyUnionTabs(db, branches, allowed);
            return new TabScope(union, allowed);
        }

        if (!SalesRequestTabPermissions.CanReadTab(user, key))
            return SalesRequestErrors.TabForbidden;

        if (SalesRequestTabMap.IsUnassignedTab(key))
            return new TabScope(
                SalesRequestAccess.UnassignedInScope(db, branches),
                [SalesRequestTabMap.Unassigned]);

        var statuses = SalesRequestTabMap.StatusesForTab(key);
        if (statuses.Count == 0)
            return SalesRequestErrors.InvalidTab;

        var q = BaseInBranches(db, branches).Where(x => statuses.Contains(x.Status));
        return new TabScope(q, [key]);
    }

    public static IQueryable<SalesRequest> ExtraViewQuery(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branches,
        string tab)
    {
        var key = SalesRequestTabMap.Normalize(tab);

        if (key == SalesRequestTabMap.Deleted)
            return db.SalesRequests.AsNoTracking()
                .Where(x => x.IsDeleted && branches.Contains(x.BranchId));

        var live = BaseInBranches(db, branches);

        if (key == SalesRequestTabMap.Sent)
        {
            var sentSources = SalesRequestTabMap.SentSourceTypes;
            return live.Where(x => sentSources.Contains(x.SourceType));
        }

        if (key == SalesRequestTabMap.Exceptions)
            return live.Where(x => x.ExceptionHoldStatus != null
                                   && (x.ExceptionHoldStatus != SalesRequestExceptionHoldStatus.Approved
                                       || x.ExceptionConsumedAtUtc == null)
                                   && x.Status != SalesRequestStatus.Rejected
                                   && x.Status != SalesRequestStatus.Completed
                                   && x.Status != SalesRequestStatus.LegalFollowup);

        if (key == SalesRequestTabMap.Legal)
            return live.Where(x => x.Status == SalesRequestStatus.LegalFollowup);

        return live.Where(_ => false);
    }

    public sealed record TabScope(IQueryable<SalesRequest> Query, IReadOnlyList<string> AppliedTabs);

    public static IQueryable<SalesRequest> ApplyUnionTabs(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branches,
        IReadOnlyList<string> tabs)
    {
        IQueryable<SalesRequest>? combined = null;
        foreach (var t in tabs)
        {
            IQueryable<SalesRequest> part;
            if (SalesRequestTabMap.IsUnassignedTab(t))
                part = SalesRequestAccess.UnassignedInScope(db, branches);
            else
            {
                var statuses = SalesRequestTabMap.StatusesForTab(t);
                if (statuses.Count == 0) continue;
                part = BaseInBranches(db, branches).Where(x => statuses.Contains(x.Status));
            }

            combined = combined == null ? part : combined.Union(part);
        }

        return combined ?? BaseInBranches(db, branches).Where(_ => false);
    }

    public static IQueryable<SalesRequest> ApplyFilters(
        IQueryable<SalesRequest> query,
        int? branchId,
        int? provinceId,
        int? employeeId,
        SalesRequestSourceType? sourceType,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? search,
        ICurrentUserService user,
        SalesRequestExceptionHoldStatus? exceptionStatus = null)
    {
        if (exceptionStatus is { } hold)
            query = query.Where(x => x.ExceptionHoldStatus == hold);

        if (branchId is int b)
        {
            if (!SalesRequestAccess.CanAccessBranch(user, b))
                return query.Where(_ => false);
            query = query.Where(x => x.BranchId == b);
        }

        if (provinceId is int p)
            query = query.Where(x => x.ProvinceId == p);

        if (employeeId is int e)
            query = query.Where(x => x.TargetEmployeeId == e);

        if (sourceType is { } source)
            query = query.Where(x => x.SourceType == source);

        if (fromDate is DateOnly from)
        {
            var fromDto = from.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.CreatedAt != null && x.CreatedAt.Value.DateTime >= fromDto);
        }

        if (toDate is DateOnly to)
        {
            var toDto = to.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(x => x.CreatedAt != null && x.CreatedAt.Value.DateTime <= toDto);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (int.TryParse(term, out var idTerm))
            {
                query = query.Where(x =>
                    x.Id == idTerm
                    || (x.CustomerName != null && x.CustomerName.Contains(term))
                    || (x.CustomerPhone != null && x.CustomerPhone.Contains(term))
                    || (x.OriginalExternalId != null && x.OriginalExternalId.Contains(term)));
            }
            else
            {
                query = query.Where(x =>
                    (x.CustomerName != null && x.CustomerName.Contains(term))
                    || (x.CustomerPhone != null && x.CustomerPhone.Contains(term))
                    || (x.OriginalExternalId != null && x.OriginalExternalId.Contains(term)));
            }
        }

        return query;
    }

    public static bool CanReadRequest(ICurrentUserService user, SalesRequest entity)
        => CanReadRow(
            user,
            entity.IsDeleted,
            entity.Status,
            entity.TargetEmployeeId,
            entity.ExceptionHoldStatus,
            entity.ExceptionConsumedAtUtc,
            entity.SourceType);

    public static bool CanReadRow(
        ICurrentUserService user,
        bool isDeleted,
        SalesRequestStatus status,
        int? targetEmployeeId,
        SalesRequestExceptionHoldStatus? hold,
        DateTimeOffset? exceptionConsumedAtUtc,
        SalesRequestSourceType sourceType)
    {
        if (isDeleted)
            return SalesRequestTabPermissions.CanReadTab(user, SalesRequestTabMap.Deleted);

        var tab = TabKeyFor(status, targetEmployeeId, hold, exceptionConsumedAtUtc);

        if (SalesRequestTabPermissions.CanReadTab(user, tab))
            return true;

        var liveHold = hold switch
        {
            null => false,
            SalesRequestExceptionHoldStatus.Approved => exceptionConsumedAtUtc is null,
            _ => true
        };

        if (liveHold
            && SalesRequestExtraViewRules.IsTerminal(status) == false
            && SalesRequestTabPermissions.CanReadTab(user, SalesRequestTabMap.Exceptions))
            return true;

        return SalesRequestTabMap.SentSourceTypes.Contains(sourceType)
               && SalesRequestTabPermissions.CanReadTab(user, SalesRequestTabMap.Sent);
    }

    public static string TabKeyFor(
        SalesRequestStatus status,
        int? targetEmployeeId,
        SalesRequestExceptionHoldStatus? hold,
        DateTimeOffset? exceptionConsumedAtUtc)
    {
        var terminal = SalesRequestExtraViewRules.IsTerminal(status);
        var liveHold = hold switch
        {
            null => false,
            SalesRequestExceptionHoldStatus.Approved => exceptionConsumedAtUtc is null,
            _ => true
        };
        var noTarget = targetEmployeeId is null or <= 0;

        var unassigned = terminal == false
                         && (liveHold == false || noTarget == false)
                         && (status == SalesRequestStatus.New || noTarget);

        return unassigned ? SalesRequestTabMap.Unassigned : SalesRequestTabMap.DisplayTabForStatus(status);
    }
}
