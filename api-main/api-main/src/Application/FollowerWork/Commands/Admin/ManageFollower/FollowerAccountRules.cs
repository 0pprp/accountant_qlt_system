using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.FollowerWork.Commands.Admin.ManageFollower;

public static class FollowerAccountRules
{
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 100;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 50;
    public const int MaxFullNameLength = 50;
    public const int MaxPhoneLength = 30;

    public static async Task<int?> FollowerRoleIdAsync(IApplicationDbContext db, CancellationToken ct)
        => await db.Roles.AsNoTracking()
            .Where(r => r.Name == SeedRoles.Motaba.Name)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync(ct);

    public static async Task<Result<List<Domain.Entities.OrderListAggregate.OrderList>>> LoadAssignableListsAsync(
        IApplicationDbContext db,
        IReadOnlyList<int> listIds,
        IReadOnlyList<int> allowedBranchIds,
        CancellationToken ct)
    {
        var ids = listIds.Distinct().ToList();
        if (ids.Count == 0)
            return new List<Domain.Entities.OrderListAggregate.OrderList>();

        var lists = await db.OrderLists
            .Where(l => ids.Contains(l.Id) && allowedBranchIds.Contains(l.BranchId))
            .ToListAsync(ct);
        if (lists.Count != ids.Count)
            return FollowerWorkErrors.ListsOutOfScope;
        return lists;
    }

    public static string NormalizeName(string raw)
        => string.Join(' ', raw.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string? NormalizeOptional(string? raw)
        => string.IsNullOrWhiteSpace(raw) ? null : NormalizeName(raw);
}
