using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Realtime;

public class SalesRequestHubIdentityResolver
{
    private readonly IApplicationDbContext _db;

    public SalesRequestHubIdentityResolver(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SalesRequestHubIdentity?> ResolveAsync(int userId, CancellationToken cancellationToken)
    {
        var exists = await _db.Users.AsNoTracking().AnyAsync(x => x.Id == userId, cancellationToken);
        if (exists == false)
            return null;

        var branchIds = await _db.UserBranches.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.BranchId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var rolePermissions = await _db.UserRoles.AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role!.RolePermissions.Select(rp => rp.Permission!.Name))
            .ToListAsync(cancellationToken);

        var allowed = await _db.UserPermissions.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Permission!.Name)
            .ToListAsync(cancellationToken);

        var denied = await _db.UserPermissionDenials.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Permission!.Name)
            .ToListAsync(cancellationToken);

        var effective = rolePermissions
            .Concat(allowed)
            .Except(denied, StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return new SalesRequestHubIdentity(userId, branchIds, effective);
    }
}
