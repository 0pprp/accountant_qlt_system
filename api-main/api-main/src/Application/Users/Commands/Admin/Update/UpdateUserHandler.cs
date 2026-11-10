using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Users.Commands.Admin.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public UpdateUserHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Include(x => x.UserBranches)
            .Include(x => x.UserLogins)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        var isUsernameExist = await _dbContext.Users.AnyAsync(
            x => x.Username == request.Username && request.Id != x.Id,
            cancellationToken);
        if (isUsernameExist)
            return UserErrors.UsernameIsAlreadyExist;

        var previousRoleId = user.UserRoles.Select(x => x.RoleId).FirstOrDefault();
        var previousBranchIds = user.UserBranches.Select(x => x.BranchId).ToList();
        var targetIsProtected = user.UserRoles.Any(x => x.Role!.Name == SeedRoles.Admin.Name);

        var scopeChanged = UserAssignmentGuard.RoleOrBranchesChanged(
            previousRoleId,
            previousBranchIds,
            request.RoleId,
            request.BranchIds);

        if (targetIsProtected && scopeChanged)
            return UserErrors.CannotChangeProtectedAccountRoleOrBranches;

        if (scopeChanged)
        {
            var assignment = await UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync(
                _dbContext,
                _currentUserService,
                targetUserId: request.Id,
                requestedRoleId: request.RoleId,
                requestedBranchIds: request.BranchIds,
                currentRoleId: previousRoleId,
                currentBranchIds: previousBranchIds,
                cancellationToken);
            if (assignment.IsFailed)
                return assignment;
        }
        else
        {
            // بيانات شخصية فقط — يمنع تعديل الحساب المحمي من غير السوبر أدمن
            if (targetIsProtected)
            {
                if (_currentUserService.UserId is null)
                    return UserErrors.NotAuthorizedToAssignRoleOrBranches;
                var isSuper = await UserAssignmentGuard.IsRealSuperAdminAsync(
                    _dbContext,
                    _currentUserService.UserId.Value,
                    cancellationToken);
                if (isSuper == false)
                    return UserErrors.ProtectedAccount;
            }

            var roleExists = await _dbContext.Roles.AnyAsync(x => x.Id == request.RoleId, cancellationToken);
            if (roleExists == false)
                return UserErrors.RoleNotFound;
        }

        var userBranches = request.BranchIds.Distinct().Select(x => new UserBranch(x)).ToHashSet();
        user.Update(
            request.FullName,
            request.MotherName,
            request.NationalCode,
            request.BirthDate,
            request.Username,
            request.Address,
            request.PhoneNumber,
            [new UserRole(request.RoleId)],
            userBranches);

        if (scopeChanged)
        {
            if (previousRoleId != request.RoleId)
                user.BumpPermissionsVersion();

            user.InvalidateSessions();
            _dbContext.UserLogins.RemoveRange(user.UserLogins);
        }

        foreach (var userBranch in user.UserBranches)
        {
            await _activityLogService.AddAsync(
                userBranch.BranchId,
                ActivityType.UserUpdated,
                TargetEntityType.User,
                request.Id,
                details: scopeChanged
                    ? $"role:{previousRoleId}->{request.RoleId}; branches:[{string.Join(',', previousBranchIds)}]->[{string.Join(',', request.BranchIds.Distinct())}]"
                    : "personal-fields");
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return UserErrors.PermissionsVersionConflict;
        }

        return Result.Success();
    }
}
