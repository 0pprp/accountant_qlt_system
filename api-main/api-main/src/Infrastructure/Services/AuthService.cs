using Application.Branches.Common;
using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Models.SignIn;
using Application.Common.Models.TokenFactory;
using Application.Common.Settings;
using Application.Permissions.Common;
using Application.Users.Common;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly TokenFactoryService _tokenFactoryService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISecurityService _securityService;
    private readonly BearerTokenSettings _bearerTokenSettings;

    public AuthService(TokenFactoryService tokenFactoryService, IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider,
        ISecurityService securityService, BearerTokenSettings bearerTokenSettings)
    {
        _tokenFactoryService = tokenFactoryService;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _securityService = securityService;
        _bearerTokenSettings = bearerTokenSettings;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default)
    {
        var createJwtDto = new CreateJwtDto
        {
            Id = request.Id,
            Roles = request.Roles,
            Permissions = request.Permissions.Select(x => x.Name).ToList(),
            SecurityStamp = request.SecurityStamp,
            BranchIds = request.Branches.Select(x => x.Id).ToList()
        };
        var jwtToken = _tokenFactoryService.CreateUserJwt(createJwtDto);
        var refreshToken = _tokenFactoryService.CreateRefreshToken();

        var hashedRefreshToken = HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = _dateTimeProvider.UtcNowOffset.AddMinutes(_bearerTokenSettings.RefreshTokenExpirationMinutes);

        var userLogin = new UserLogin(hashedRefreshToken, refreshTokenExpiresAt, request.Id);

        _dbContext.UserLogins.Add(userLogin);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        var groupedPermissions = request.Permissions
            .GroupBy(p => p.Name.Split('.')[0])
            .ToDictionary(
                group => group.Key,
                group => group.Select(p => new GetPermissionDto
                {
                    Id = p.Id,
                    Name = p.Name.Split('.')[1]
                }).ToList()
            );
        
        var branches = request.Branches.Select(x => new GetBranchDto
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();
        
        return new SignInResponse
        {
            AccessToken = jwtToken,
            RefreshToken = refreshToken,
            Permissions = groupedPermissions,
            Branches = branches,
            Roles = request.Roles
        };
    }

    public async Task<Result<SignInResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hashedRefreshToken = HashRefreshToken(refreshToken);
        
        var userLogin = await _dbContext.UserLogins
            .Select(x => new UserLoginDto
            {
                UserId = x.UserId!.Value,
                UserSecurityStamp = x.User!.SecurityStamp,
                RefreshToken = x.RefreshToken,
                RefreshTokenExpiresAt = x.RefreshTokenExpiresAt,
                Roles = x.User.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                UserPermissions = x.User.UserPermissions.Select(up => up.Permission).ToList()!,
                RolePermissions = x.User.UserRoles.SelectMany(ur => ur.Role!.RolePermissions.Select(rp => rp.Permission)).ToList()!,
                Branches = x.User.UserBranches.Select(ub => ub.Branch).ToList()!
            })
            .FirstOrDefaultAsync(x => x.RefreshToken == hashedRefreshToken, cancellationToken);

        if (userLogin is null || userLogin.RefreshTokenExpiresAt < _dateTimeProvider.UtcNowOffset)
            return UserErrors.RefreshTokenIsNotValid;
        
        var createJwtDto = new CreateJwtDto
        {
            Id = userLogin.UserId,
            Roles = userLogin.Roles,
            Permissions = userLogin.Permissions.Select(x => x.Name).ToList(),
            SecurityStamp = userLogin.UserSecurityStamp,
            BranchIds = userLogin.Branches.Select(x => x.Id).ToList()
        };
        var newJwtToken = _tokenFactoryService.CreateUserJwt(createJwtDto);
        var newRefreshToken = _tokenFactoryService.CreateRefreshToken();

        await _dbContext.UserLogins
            .Where(x => x.RefreshToken == hashedRefreshToken)
            .ExecuteUpdateAsync(x => x.SetProperty(ul => ul.RefreshToken, HashRefreshToken(newRefreshToken)), cancellationToken);
        
        var groupedPermissions = userLogin.Permissions
            .GroupBy(p => p.Name.Split('.')[0])
            .ToDictionary(
                group => group.Key,
                group => group.Select(p => new GetPermissionDto
                {
                    Id = p.Id,
                    Name = p.Name.Split('.')[1]
                }).ToList()
            );
        
        var branches = userLogin.Branches.Select(x => new GetBranchDto
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();

        return new SignInResponse
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken,
            Permissions = groupedPermissions,
            Branches = branches,
            Roles = userLogin.Roles
        };
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserLogins
            .Where(x => x.RefreshToken == HashRefreshToken(refreshToken))
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }

    private string HashRefreshToken(string input) => _securityService.GetSha256Hash(input);
}