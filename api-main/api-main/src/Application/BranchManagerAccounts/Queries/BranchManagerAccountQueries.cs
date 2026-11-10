using Application.BranchManagerAccounts.Common;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.BranchManagerAccounts.Queries;

public sealed class ListBranchManagerAccountsQuery : IRequest<Result<BranchManagerAccountListResponse>>;

public sealed class ListBranchManagerAccountCitiesQuery : IRequest<Result<IReadOnlyList<BranchOptionDto>>>;

public sealed class ListBranchManagerAccountsHandler
    : IRequestHandler<ListBranchManagerAccountsQuery, Result<BranchManagerAccountListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListBranchManagerAccountsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<BranchManagerAccountListResponse>> Handle(
        ListBranchManagerAccountsQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchManagerAccount.Read) == false)
            return BranchManagerAccountErrors.Forbidden;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return new BranchManagerAccountListResponse([], 0);

        var bmRoleId = await _db.Roles.AsNoTracking()
            .Where(r => r.Name == SeedRoles.BranchManager.Name)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);
        if (bmRoleId == 0)
            return new BranchManagerAccountListResponse([], 0);

        var rows = await _db.Users.AsNoTracking()
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == bmRoleId)
                        || u.UserBranches.Any(ub => scope.Contains(ub.BranchId)))
            .Where(u => u.UserBranches.Any(ub => scope.Contains(ub.BranchId))
                        && u.UserRoles.All(ur => ur.Role!.Name != SeedRoles.Admin.Name)
                        && (u.UserRoles.Any(ur => ur.RoleId == bmRoleId)
                            || u.UserRoles.Count == 0))
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.FullName,
                IsActive = u.UserRoles.Any(ur => ur.RoleId == bmRoleId),
                Branches = u.UserBranches
                    .Where(ub => scope.Contains(ub.BranchId))
                    .Select(ub => new { ub.BranchId, Name = ub.Branch!.Name })
                    .ToList()
            })
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        // فقط حسابات مدير فرع ضمن نطاق الفاعل (كل فروعه ⊆ النطاق)
        var items = rows
            .Where(u => u.Branches.Count > 0
                        && u.Branches.All(b => scope.Contains(b.BranchId)))
            .Select(u => new BranchManagerAccountDto(
                u.Id,
                u.Username,
                u.FullName,
                u.IsActive,
                u.Branches.Select(b => b.BranchId).ToList(),
                u.Branches.Select(b => b.Name ?? $"فرع {b.BranchId}").ToList()))
            .ToList();

        return new BranchManagerAccountListResponse(items, items.Count);
    }
}

public sealed class ListBranchManagerAccountCitiesHandler
    : IRequestHandler<ListBranchManagerAccountCitiesQuery, Result<IReadOnlyList<BranchOptionDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListBranchManagerAccountCitiesHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<IReadOnlyList<BranchOptionDto>>> Handle(
        ListBranchManagerAccountCitiesQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchManagerAccount.Read) == false
            && _current.HasPermission(SeedPermissions.BranchManagerAccount.Write) == false)
            return BranchManagerAccountErrors.Forbidden;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return Array.Empty<BranchOptionDto>();

        var cities = await _db.Branches.AsNoTracking()
            .Where(b => scope.Contains(b.Id))
            .OrderBy(b => b.Name)
            .Select(b => new BranchOptionDto(b.Id, b.Name))
            .ToListAsync(ct);
        return cities;
    }
}
