using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.DelegateHome.Queries;

public sealed record DelegateAccountDto(
    string FullName,
    string Username,
    bool AllBranches,
    IReadOnlyList<string> BranchNames);

public sealed class GetDelegateAccountQuery : IRequest<Result<DelegateAccountDto>>
{
}

public sealed class GetDelegateAccountHandler
    : IRequestHandler<GetDelegateAccountQuery, Result<DelegateAccountDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetDelegateAccountHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<DelegateAccountDto>> Handle(GetDelegateAccountQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.DelegateHome.Read) == false)
            return new Error("لا صلاحية لعرض بيانات الحساب.", "DelegateHome.Forbidden");
        if (_current.UserId is not int userId)
            return new Error("لا صلاحية لعرض بيانات الحساب.", "DelegateHome.Forbidden");

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.FullName, u.Username })
            .FirstOrDefaultAsync(ct);
        if (user is null)
            return new Error("المستخدم غير موجود.", "DelegateHome.UserNotFound");

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return new DelegateAccountDto(user.FullName, user.Username, false, []);

        var names = await _db.Branches.AsNoTracking()
            .Where(b => scope.Contains(b.Id))
            .OrderBy(b => b.Name)
            .Select(b => b.Name)
            .ToListAsync(ct);
        var total = await _db.Branches.AsNoTracking().CountAsync(ct);
        var all = total > 0 && names.Count >= total;

        return new DelegateAccountDto(user.FullName, user.Username, all, names);
    }
}
