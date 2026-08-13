using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateUserHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
            .Include(x => x.UserBranches)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        var isUsernameExist = await _dbContext.Users.AnyAsync(x => x.Username == request.Username && request.Id != x.Id, cancellationToken);
        if (isUsernameExist)
            return UserErrors.UsernameIsAlreadyExist;

        var isRoleExist = await _dbContext.Roles.AnyAsync(x => x.Id == request.RoleId, cancellationToken);
        if (isRoleExist == false)
            return UserErrors.RoleNotFound;

        var userBranches = request.BranchIds.Select(x => new UserBranch(x)).ToHashSet();
        user.Update(request.FullName, request.MotherName, request.NationalCode, request.BirthDate, request.Username, request.Address,
            request.PhoneNumber, [new UserRole(request.RoleId)], userBranches);

        foreach (var userBranch in user.UserBranches)
        {
            await _activityLogService.AddAsync(
                userBranch.BranchId,
                ActivityType.UserUpdated,
                TargetEntityType.User,
                request.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}