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
    private readonly IActivityLogService _activityLogService;

    public CreateUserHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var isUsernameExist = await _dbContext.Users.AnyAsync(x => x.Username == request.Username, cancellationToken);
        if (isUsernameExist)
            return UserErrors.UsernameIsAlreadyExist;

        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
        if (role is null)
            return UserErrors.RoleNotFound;

        // Super Admin is seeded once; these screens must not create another Admin user.
        if (role.Name == global::Application.Common.SeedData.Roles.Admin.Name)
            return UserErrors.CannotAssignAdminRole;

        var hashedPassword = PasswordHash.HashPassword(request.Password);
        var user = new User(request.FullName, request.MotherName, request.NationalCode, request.BirthDate, request.Username, hashedPassword,
            request.Address, request.PhoneNumber)
        {
            UserBranches = request.BranchIds.Select(x => new UserBranch(x)).ToHashSet(),
            UserRoles = [new UserRole(request.RoleId)]
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var branchId in request.BranchIds)
        {
            await _activityLogService.AddAsync(
                branchId,
                ActivityType.UserCreated,
                TargetEntityType.User,
                user.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(user.Id);
    }
}