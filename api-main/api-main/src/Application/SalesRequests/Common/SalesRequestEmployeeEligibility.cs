using Application.Common.Interfaces;
using Application.SalesEmployees.Common;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Common;

/// <summary>
/// أهلية موحّدة للإسناد اليدوي وtransfer-peers وtransfer-name.
/// القاعدة: تكليف SalesEmployeeAssignment نشط على فرع الطلب + حساب صالح + AppRead الفعلية مع احترام Deny.
/// تعيد استخدام <see cref="SalesWorkforce"/> — ليست عضوية فرع ولا دور Mandob وحدهما.
/// </summary>
public static class SalesRequestEmployeeEligibility
{
    public static async Task<bool> IsEligibleAssigneeAsync(
        IApplicationDbContext db,
        int employeeId,
        int requestBranchId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        if (!await SalesWorkforce.HasActiveAssignmentAsync(
                db, employeeId, requestBranchId, utcNow, cancellationToken))
            return false;

        return await QueryEligibleAssignees(db, requestBranchId, utcNow)
            .AnyAsync(x => x.Id == employeeId, cancellationToken);
    }

    /// <summary>مرشحون لنفس قاعدة الأهلية — قائمة الإسناد ونقل الاسم.</summary>
    public static IQueryable<Domain.Entities.UserAggregate.User> QueryEligibleAssignees(
        IApplicationDbContext db,
        int requestBranchId,
        DateTimeOffset utcNow)
    {
        var appRead = SeedPermissions.SalesRequest.AppRead;
        var activeUserIds = SalesWorkforce
            .ActiveAssignments(db, utcNow, [requestBranchId])
            .Select(a => a.UserId);

        return db.Users.AsNoTracking()
            .Where(x => activeUserIds.Contains(x.Id))
            .Where(x => x.CreationStep == UserCreationStep.Completed)
            .Where(x => x.Username != null && x.Username != "")
            .Where(x => x.PasswordHash != null && x.PasswordHash != "")
            .Where(x => !x.UserPermissionDenials.Any(d =>
                d.Permission != null && d.Permission.Name == appRead))
            .Where(x =>
                x.UserRoles.Any(ur =>
                    ur.Role != null
                    && ur.Role.RolePermissions.Any(rp =>
                        rp.Permission != null && rp.Permission.Name == appRead))
                || x.UserPermissions.Any(up =>
                    up.Permission != null && up.Permission.Name == appRead));
    }
}
