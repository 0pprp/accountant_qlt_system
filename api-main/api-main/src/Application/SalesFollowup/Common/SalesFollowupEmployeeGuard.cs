using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesFollowup.Common;

public static class SalesFollowupEmployeeGuard
{
    public static async Task<Error?> EnsureAssignedInBranchAsync(
        IApplicationDbContext db,
        int employeeId,
        int branchId,
        DateTimeOffset utcNow,
        CancellationToken ct)
    {
        var ok = await SalesWorkforce.HasActiveAssignmentAsync(db, employeeId, branchId, utcNow, ct);
        if (!ok)
            return SalesFollowupErrors.EmployeeNotFound;

        var exists = await db.Users.AsNoTracking().AnyAsync(u => u.Id == employeeId, ct);
        return exists ? null : SalesFollowupErrors.EmployeeNotFound;
    }
}
