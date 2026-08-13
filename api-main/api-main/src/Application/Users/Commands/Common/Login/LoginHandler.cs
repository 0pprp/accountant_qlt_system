using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.SignIn;
using Application.Common.Utilities;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Common.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly IActivityLogService _activityLogService;

    public LoginHandler(IApplicationDbContext dbContext, IAuthService authService, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _authService = authService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.PasswordHash,
                HasRoles = x.UserRoles.Any()
            })
            .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

        if (user is null)
            return UserErrors.UsernameOrPasswordIsNotCorrect;

        var isPasswordValid = PasswordHash.VerifyHashedPassword(user.PasswordHash, request.Password);
        if (isPasswordValid == false)
            return UserErrors.UsernameOrPasswordIsNotCorrect;

        if (user.HasRoles == false)
            return UserErrors.UsernameOrPasswordIsNotCorrect;

        var userDetails = await _dbContext.Users.AsNoTracking()
            .AsSplitQuery()
            .Select(x => new SignInRequest
            {
                Id = x.Id,
                SecurityStamp = x.SecurityStamp,
                Roles = x.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                UserPermissions = x.UserPermissions.Select(up => up.Permission).ToList()!,
                RolePermissions = x.UserRoles.SelectMany(ur => ur.Role!.RolePermissions.Select(rp => rp.Permission)).ToList()!,
                Branches = x.UserBranches.Select(ub => ub.Branch).ToList()!
            })
            .FirstAsync(x => x.Id == user.Id, cancellationToken);

        var roleDisplayNames = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == user.Id)
            .SelectMany(x => x.UserRoles.Select(ur => ur.Role!.DisplayName))
            .ToListAsync(cancellationToken);

        var actor = new ActivityLogActor(user.Id, user.Username, string.Join(",", roleDisplayNames));

        foreach (var branch in userDetails.Branches)
        {
            await _activityLogService.AddAsync(
                branch.Id,
                ActivityType.Login,
                TargetEntityType.User,
                user.Id,
                actor);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var signInResponse = await _authService.SignInAsync(userDetails, cancellationToken);

        return new LoginResponse
        {
            AccessToken = signInResponse.AccessToken,
            RefreshToken = signInResponse.RefreshToken,
            Permissions = signInResponse.Permissions,
            Branches = signInResponse.Branches,
            Roles = signInResponse.Roles,
            IsSuperAdmin = userDetails.Roles.Any(x => x == Application.Common.SeedData.Roles.Admin.Name)
        };
    }
}
