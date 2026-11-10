using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Users.Common;

public static class UserPermissionGuard
{
    public static async Task<Result> EnsureRealSuperAdminCanManageAsync(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        int targetUserId,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
            return UserErrors.NotAuthorizedToManagePermissions;

        var actorId = currentUserService.UserId.Value;

        var isRealSuperAdmin = await dbContext.UserRoles
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == actorId && x.Role!.Name == SeedRoles.Admin.Name,
                cancellationToken);

        if (isRealSuperAdmin == false)
            return UserErrors.NotAuthorizedToManagePermissions;

        if (actorId == targetUserId)
            return UserErrors.CannotManageOwnPermissions;

        var targetIsProtectedAdmin = await dbContext.UserRoles
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == targetUserId && x.Role!.Name == SeedRoles.Admin.Name,
                cancellationToken);

        if (targetIsProtectedAdmin)
            return UserErrors.ProtectedAccount;

        return Result.Success();
    }
}
