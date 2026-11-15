using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskAudienceResolver
{
    public const int MaxRecipients = 500;
    public const int PreviewSampleSize = 20;

    public static async Task<(List<ManagementTaskAudienceRecipient>? Recipients, Error? Error)> ResolveAsync(
        IApplicationDbContext db,
        ManagementTaskAudience audience,
        int branchId,
        int? assigneeUserId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        if (branchId <= 0)
            return (null, ManagementTaskErrors.BranchRequired);

        if (audience == ManagementTaskAudience.User)
        {
            var (user, error) = await ManagementTaskAccess.ResolveEligibleUserAsync(
                db, assigneeUserId ?? 0, branchId, cancellationToken);
            if (error is not null || user is null)
                return (null, error ?? ManagementTaskErrors.AssigneeNotEligible);

            return ([new ManagementTaskAudienceRecipient(user.Id, user.FullName, branchId)], null);
        }

        var list = audience switch
        {
            ManagementTaskAudience.Delegate => await ResolveByRoleAsync(
                db, SeedRoles.Mandob.Name, branchId, cancellationToken),
            ManagementTaskAudience.Committee => await ResolveByRoleAsync(
                db, SeedRoles.Motaba.Name, branchId, cancellationToken),
            ManagementTaskAudience.SalesEmployee => await ResolveSalesEmployeesAsync(
                db, branchId, utcNow, cancellationToken),
            ManagementTaskAudience.AllFieldApps => await ResolveAllFieldAppsAsync(
                db, branchId, utcNow, cancellationToken),
            _ => []
        };

        if (list.Count == 0)
            return (null, ManagementTaskErrors.AudienceEmpty);

        if (list.Count > MaxRecipients)
            return (null, ManagementTaskErrors.AudienceTooLarge);

        return (list, null);
    }

    private static async Task<List<ManagementTaskAudienceRecipient>> ResolveAllFieldAppsAsync(
        IApplicationDbContext db,
        int branchId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var mandob = await ResolveByRoleAsync(db, SeedRoles.Mandob.Name, branchId, cancellationToken);
        var motaba = await ResolveByRoleAsync(db, SeedRoles.Motaba.Name, branchId, cancellationToken);
        var sales = await ResolveSalesEmployeesAsync(db, branchId, utcNow, cancellationToken);

        return mandob
            .Concat(motaba)
            .Concat(sales)
            .GroupBy(x => x.UserId)
            .Select(g => g.First())
            .OrderBy(x => x.DisplayName)
            .ToList();
    }

    private static async Task<List<ManagementTaskAudienceRecipient>> ResolveByRoleAsync(
        IApplicationDbContext db,
        string roleName,
        int branchId,
        CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.CreationStep == UserCreationStep.Completed)
            .Where(u => u.UserBranches.Any(b => b.BranchId == branchId))
            .Where(u => u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == roleName))
            .OrderBy(u => u.FullName)
            .Select(u => new ManagementTaskAudienceRecipient(u.Id, u.FullName, branchId))
            .ToListAsync(cancellationToken);
    }

    private static async Task<List<ManagementTaskAudienceRecipient>> ResolveSalesEmployeesAsync(
        IApplicationDbContext db,
        int branchId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var rows = await SalesWorkforce
            .ActiveAssignments(db, utcNow, [branchId])
            .Join(
                db.Users.AsNoTracking().Where(u => u.CreationStep == UserCreationStep.Completed),
                a => a.UserId,
                u => u.Id,
                (a, u) => new { a.UserId, u.FullName, a.BranchId })
            .Distinct()
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.UserId)
            .Select(g =>
            {
                var first = g.First();
                return new ManagementTaskAudienceRecipient(first.UserId, first.FullName, first.BranchId);
            })
            .ToList();
    }
}
