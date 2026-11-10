using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Domain.Entities.AppNoteAggregate.Enums;

namespace Application.AppNotes.Common;

/// <summary>
/// مصدر AppNote من الجلسة + تكليف موظف المبيعات — لا يُشتق من دور Mandob وحده.
/// </summary>
public static class AppNoteSourceResolver
{
    public static async Task<AppNoteSourceApp?> ResolveAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        CancellationToken ct)
    {
        if (current.UserId is not int userId)
            return null;

        var role = current.Role ?? "";
        var now = clock.UtcNowOffset;

        // تكليف نشط يسبق تصنيف Mandob كمندوب جباية.
        if (await SalesWorkforce.HasAnyActiveAssignmentAsync(db, userId, now, ct))
            return AppNoteSourceApp.SalesEmployee;

        if (role.Equals("Motaba", StringComparison.OrdinalIgnoreCase)
            || role.Equals("Follower", StringComparison.OrdinalIgnoreCase))
            return AppNoteSourceApp.Follower;

        if (role.Equals("BranchManager", StringComparison.OrdinalIgnoreCase))
            return AppNoteSourceApp.BranchManager;

        if (role.Equals("Mandob", StringComparison.OrdinalIgnoreCase)
            || role.Equals("Delegate", StringComparison.OrdinalIgnoreCase))
            return AppNoteSourceApp.Delegate;

        return null;
    }
}
