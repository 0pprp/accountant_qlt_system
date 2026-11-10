using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.Create;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public CreateUserHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var assignment = await UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync(
            _dbContext,
            _currentUserService,
            targetUserId: null,
            requestedRoleId: request.RoleId,
            requestedBranchIds: request.BranchIds,
            currentRoleId: null,
            currentBranchIds: [],
            cancellationToken);
        if (assignment.IsFailed)
            return assignment.Error!.Value;

        var isUsernameExist = await _dbContext.Users.AnyAsync(x => x.Username == request.Username, cancellationToken);
        if (isUsernameExist)
            return UserErrors.UsernameIsAlreadyExist;

        var hashedPassword = PasswordHash.HashPassword(request.Password);
        var user = new User(request.FullName, request.MotherName, request.NationalCode, request.BirthDate, request.Username, hashedPassword,
            request.Address, request.PhoneNumber)
        {
            UserBranches = request.BranchIds.Distinct().Select(x => new UserBranch(x)).ToHashSet(),
            UserRoles = [new UserRole(request.RoleId)]
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var branchId in request.BranchIds.Distinct())
        {
            await _activityLogService.AddAsync(
                branchId,
                ActivityType.UserCreated,
                TargetEntityType.User,
                user.Id,
                details: $"roleId={request.RoleId}; branches=[{string.Join(',', request.BranchIds.Distinct())}]");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(user.Id);
    }
}
