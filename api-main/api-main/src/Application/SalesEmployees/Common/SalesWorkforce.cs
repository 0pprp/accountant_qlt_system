using Application.Common.Interfaces;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesEmployees.Common;

/// <summary>
/// قاعدة أهلية موظف المبيعات المشتركة (Overview / Employees / Followup / Tracking / Ingest).
/// المصدر الوحيد: SalesEmployeeAssignment النشط — ليس Mandob وحده ولا SellerId وحده.
/// </summary>
public static class SalesWorkforce
{
    public const string ShiftActive = "Active";
    public const string ShiftNoShift = "NoShift";

    public static readonly HashSet<string> AllowedShiftStates =
        new(StringComparer.OrdinalIgnoreCase) { ShiftActive, ShiftNoShift };

    public static readonly HashSet<string> AllowedSorts =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "name", "branch", "shift", "rating", "lastLocation"
        };

    public static IQueryable<Domain.Entities.SalesEmployeeAggregate.SalesEmployeeAssignment> ActiveAssignments(
        IApplicationDbContext db,
        DateTimeOffset utcNow,
        IReadOnlyList<int> branchIds)
    {
        return db.SalesEmployeeAssignments.AsNoTracking()
            .Where(a => branchIds.Contains(a.BranchId))
            .Where(a => a.StartsAtUtc <= utcNow)
            .Where(a => a.EndsAtUtc == null || a.EndsAtUtc > utcNow);
    }

    public static async Task<List<int>> ActiveUserIdsAsync(
        IApplicationDbContext db,
        DateTimeOffset utcNow,
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0)
            return [];

        return await ActiveAssignments(db, utcNow, branchIds)
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync(ct);
    }

    public static async Task<bool> HasActiveAssignmentAsync(
        IApplicationDbContext db,
        int userId,
        int branchId,
        DateTimeOffset utcNow,
        CancellationToken ct)
    {
        return await db.SalesEmployeeAssignments.AsNoTracking()
            .AnyAsync(
                a => a.UserId == userId
                     && a.BranchId == branchId
                     && a.StartsAtUtc <= utcNow
                     && (a.EndsAtUtc == null || a.EndsAtUtc > utcNow),
                ct);
    }

    public static async Task<bool> HasAnyActiveAssignmentAsync(
        IApplicationDbContext db,
        int userId,
        DateTimeOffset utcNow,
        CancellationToken ct)
    {
        return await db.SalesEmployeeAssignments.AsNoTracking()
            .AnyAsync(
                a => a.UserId == userId
                     && a.StartsAtUtc <= utcNow
                     && (a.EndsAtUtc == null || a.EndsAtUtc > utcNow),
                ct);
    }

    /// <summary>حسابات مكتملة ضمن تكليف نشط في النطاق.</summary>
    public static IQueryable<Domain.Entities.UserAggregate.User> QueryAssignedUsers(
        IApplicationDbContext db,
        DateTimeOffset utcNow,
        IReadOnlyList<int> branchIds)
    {
        var activeUserIds = ActiveAssignments(db, utcNow, branchIds).Select(a => a.UserId);
        return db.Users.AsNoTracking()
            .Where(u => u.CreationStep == UserCreationStep.Completed)
            .Where(u => activeUserIds.Contains(u.Id));
    }

    public static IReadOnlyList<int> VisibleBranches(
        IEnumerable<int> assignmentBranchIds,
        IReadOnlyList<int> viewerScope)
    {
        var set = viewerScope.ToHashSet();
        return assignmentBranchIds.Where(set.Contains).Distinct().OrderBy(x => x).ToList();
    }
}
