using Application.Common.Interfaces;
using Application.Common.SeedData;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Committee.Common;

public static class CommitteeAccess
{
    /// <summary>مسؤول اللجنة = دور المتابع (Motaba) في النظام.</summary>
    public static bool IsCommitteeOfficerRole(string? role) =>
        string.Equals(role, SeedRoles.Motaba.Name, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, "Motaba", StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, "المتابع", StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, "مسؤول اللجنة", StringComparison.OrdinalIgnoreCase);

    public static async Task<bool> OwnsListAsync(
        IApplicationDbContext db,
        int userId,
        int listId,
        CancellationToken ct)
    {
        return await db.OrderLists.AsNoTracking()
            .AnyAsync(x => x.Id == listId && (x.MotabaId == userId || x.MandobId == userId), ct);
    }
}
