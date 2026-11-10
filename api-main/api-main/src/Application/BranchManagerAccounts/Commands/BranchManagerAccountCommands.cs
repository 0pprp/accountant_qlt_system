using Application.BranchManagerAccounts.Common;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.BranchManagerAccounts.Commands;

public sealed class CreateBranchManagerAccountCommand : IRequest<Result<BranchManagerAccountDto>>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FullName { get; init; }
    public required IReadOnlyList<int> BranchIds { get; init; }
}

public sealed class UpdateBranchManagerAccountCommand : IRequest<Result<BranchManagerAccountDto>>
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public string? Password { get; init; }
    public required string FullName { get; init; }
    public bool IsActive { get; init; }
    public required IReadOnlyList<int> BranchIds { get; init; }
}

public sealed class CreateBranchManagerAccountHandler
    : IRequestHandler<CreateBranchManagerAccountCommand, Result<BranchManagerAccountDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IActivityLogService _log;

    public CreateBranchManagerAccountHandler(
        IApplicationDbContext db, ICurrentUserService current, IActivityLogService log)
    {
        _db = db;
        _current = current;
        _log = log;
    }

    public async Task<Result<BranchManagerAccountDto>> Handle(
        CreateBranchManagerAccountCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchManagerAccount.Write) == false)
            return BranchManagerAccountErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return BranchManagerAccountErrors.Forbidden;

        // لا تسمح ManagePermissions بتجاوز هذه السياسة
        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return BranchManagerAccountErrors.BranchScope;

        var name = (request.Username ?? "").Trim();
        var full = (request.FullName ?? "").Trim();
        var branches = (request.BranchIds ?? []).Distinct().Where(id => id > 0).ToList();
        if (name.Length < 3 || full.Length < 2 || string.IsNullOrEmpty(request.Password) || request.Password.Length < 4)
            return BranchManagerAccountErrors.Validation;
        if (branches.Count == 0 || branches.Any(id => scope.Contains(id) == false))
            return BranchManagerAccountErrors.BranchScope;

        if (await _db.Users.AnyAsync(u => u.Username == name, ct))
            return BranchManagerAccountErrors.UsernameTaken;

        var bmRole = await _db.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == SeedRoles.BranchManager.Name, ct);
        if (bmRole is null)
            return BranchManagerAccountErrors.RoleForbidden;

        var existingBranches = await _db.Branches.AsNoTracking()
            .CountAsync(b => branches.Contains(b.Id), ct);
        if (existingBranches != branches.Count)
            return BranchManagerAccountErrors.BranchScope;

        var user = new User(
            full,
            motherName: "-",
            nationalCode: $"BM-{Guid.NewGuid():N}"[..16],
            birthDate: new DateOnly(1990, 1, 1),
            username: name,
            passwordHash: PasswordHash.HashPassword(request.Password),
            address: "-",
            phoneNumber: "07000000000")
        {
            CreationStep = UserCreationStep.Completed,
            UserRoles = [new UserRole(bmRole.Id)],
            UserBranches = branches.Select(id => new UserBranch(id)).ToHashSet()
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        foreach (var bid in branches)
        {
            await _log.AddAsync(bid, ActivityType.UserCreated, TargetEntityType.User, user.Id,
                details: $"branchManagerAccount;actor={actorId};branches=[{string.Join(',', branches)}]");
        }
        await _db.SaveChangesAsync(ct);

        var names = await _db.Branches.AsNoTracking()
            .Where(b => branches.Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToListAsync(ct);
        return new BranchManagerAccountDto(
            user.Id, user.Username, user.FullName, true,
            branches, names.OrderBy(x => x.Id).Select(x => x.Name).ToList());
    }
}

public sealed class UpdateBranchManagerAccountHandler
    : IRequestHandler<UpdateBranchManagerAccountCommand, Result<BranchManagerAccountDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IActivityLogService _log;

    public UpdateBranchManagerAccountHandler(
        IApplicationDbContext db, ICurrentUserService current, IActivityLogService log)
    {
        _db = db;
        _current = current;
        _log = log;
    }

    public async Task<Result<BranchManagerAccountDto>> Handle(
        UpdateBranchManagerAccountCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchManagerAccount.Write) == false)
            return BranchManagerAccountErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return BranchManagerAccountErrors.Forbidden;
        if (actorId == request.Id)
            return BranchManagerAccountErrors.Protected;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return BranchManagerAccountErrors.BranchScope;

        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Id == request.Id, ct);
        if (user is null)
            return BranchManagerAccountErrors.NotFound;

        if (user.UserRoles.Any(ur => ur.Role!.Name == SeedRoles.Admin.Name))
            return BranchManagerAccountErrors.Protected;

        var wasBm = user.UserRoles.Any(ur => ur.Role!.Name == SeedRoles.BranchManager.Name);
        var hadNoRoles = user.UserRoles.Count == 0;
        if (wasBm == false && hadNoRoles == false)
            return BranchManagerAccountErrors.RoleForbidden;

        var currentBranches = user.UserBranches.Select(ub => ub.BranchId).ToList();
        if (currentBranches.Count == 0 || currentBranches.Any(id => scope.Contains(id) == false))
            return BranchManagerAccountErrors.BranchScope;

        var name = (request.Username ?? "").Trim();
        var full = (request.FullName ?? "").Trim();
        var branches = (request.BranchIds ?? []).Distinct().Where(id => id > 0).ToList();
        if (name.Length < 3 || full.Length < 2)
            return BranchManagerAccountErrors.Validation;
        if (branches.Count == 0 || branches.Any(id => scope.Contains(id) == false))
            return BranchManagerAccountErrors.BranchScope;
        if (string.IsNullOrEmpty(request.Password) == false && request.Password.Length < 4)
            return BranchManagerAccountErrors.Validation;

        if (await _db.Users.AnyAsync(u => u.Username == name && u.Id != request.Id, ct))
            return BranchManagerAccountErrors.UsernameTaken;

        var bmRole = await _db.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == SeedRoles.BranchManager.Name, ct);
        if (bmRole is null)
            return BranchManagerAccountErrors.RoleForbidden;

        var scopeChanged = currentBranches.OrderBy(x => x).SequenceEqual(branches.OrderBy(x => x)) == false
                           || wasBm != request.IsActive
                           || string.IsNullOrEmpty(request.Password) == false;

        user.FullName = full;
        user.Username = name;
        if (string.IsNullOrEmpty(request.Password) == false)
            user.PasswordHash = PasswordHash.HashPassword(request.Password);

        user.UserBranches = branches.Select(id => new UserBranch(id)).ToHashSet();
        user.UserRoles = request.IsActive
            ? [new UserRole(bmRole.Id)]
            : [];

        if (scopeChanged)
            user.InvalidateSessions();

        foreach (var bid in branches)
        {
            await _log.AddAsync(bid, ActivityType.UserUpdated, TargetEntityType.User, user.Id,
                details: $"branchManagerAccount;actor={actorId};active={request.IsActive};branches=[{string.Join(',', branches)}]");
        }
        await _db.SaveChangesAsync(ct);

        var names = await _db.Branches.AsNoTracking()
            .Where(b => branches.Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToListAsync(ct);
        return new BranchManagerAccountDto(
            user.Id, user.Username, user.FullName, request.IsActive,
            branches, names.OrderBy(x => x.Id).Select(x => x.Name).ToList());
    }
}
