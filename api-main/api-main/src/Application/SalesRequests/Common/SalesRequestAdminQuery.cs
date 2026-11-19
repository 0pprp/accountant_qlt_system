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

        var scoped = TabQuery(db, branches, key);
        if (scoped is null)
            return SalesRequestErrors.InvalidTab;

        return new TabScope(scoped, [key]);
    }

    public static IQueryable<SalesRequest>? TabQuery(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branches,
        string tab)
    {
        var key = SalesRequestTabMap.Normalize(tab);
        if (SalesRequestTabMap.IsUnassignedTab(key))
            return SalesRequestAccess.UnassignedInScope(db, branches);

        var live = BaseInBranches(db, branches);
        if (key == SalesRequestTabMap.Exceptions)
            return live.Where(x => x.ExceptionHoldStatus != null
                                   && (x.ExceptionHoldStatus != SalesRequestExceptionHoldStatus.Approved
                                       || x.ExceptionConsumedAtUtc == null)
                                   && x.Status != SalesRequestStatus.Rejected
                                   && x.Status != SalesRequestStatus.Completed
                                   && x.Status != SalesRequestStatus.LegalFollowup);

        var statuses = SalesRequestTabMap.StatusesForTab(key);
        if (statuses.Count == 0)
            return null;

        var q = live.Where(x => statuses.Contains(x.Status));
        if (SalesRequestTabMap.IsActiveWorkTab(key))
            q = q.Where(x => x.TargetEmployeeId != null
                             && x.TargetEmployeeId > 0
                             && (x.ExceptionHoldStatus == null
                                 || (x.ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Approved
                                     && x.ExceptionConsumedAtUtc != null)));
        return q;
    }

    public static IQueryable<SalesRequest> ExtraViewQuery(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branches,
        string tab)
    {
        var key = SalesRequestTabMap.Normalize(tab);

        if (key == SalesRequestTabMap.Deleted)
            return db.SalesRequests.AsNoTracking()
                .Where(x => x.IsDeleted
                            && x.Status != SalesRequestStatus.Completed
                            && branches.Contains(x.BranchId));

        return BaseInBranches(db, branches).Where(_ => false);
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
            var part = TabQuery(db, branches, t);
            if (part is null) continue;
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
        return SalesRequestTabPermissions.CanReadTab(user, tab);
    }

    public static string TabKeyFor(
        SalesRequestStatus status,
        int? targetEmployeeId,
        SalesRequestExceptionHoldStatus? hold,
        DateTimeOffset? exceptionConsumedAtUtc)
    {
        if (SalesRequestExtraViewRules.IsTerminal(status))
            return SalesRequestTabMap.DisplayTabForStatus(status);

        var liveHold = hold switch
        {
            null => false,
            SalesRequestExceptionHoldStatus.Approved => exceptionConsumedAtUtc is null,
            _ => true
        };
        if (liveHold)
            return SalesRequestTabMap.Exceptions;

        var noTarget = targetEmployeeId is null or <= 0;
        return status == SalesRequestStatus.New || noTarget
            ? SalesRequestTabMap.Unassigned
            : SalesRequestTabMap.DisplayTabForStatus(status);
    }
}
