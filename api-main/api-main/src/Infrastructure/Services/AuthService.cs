using Application.Branches.Common;
using Application.Common;
using Application.Common.AdminPanel;
using Application.Common.Interfaces;
using Application.Common.Models.SignIn;
using Application.Common.Models.TokenFactory;
using Application.Common.Settings;
using Application.Permissions.Common;
using Application.Users.Common;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using PermissionEntity = Domain.Entities.RoleAggregate.Permission;

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
        // JWT: إجراءات فقط (بدون Panel.Tab.*) لتفادي تجاوز حد ترويسة Authorization.
        // جسم الاستجابة للعميل يحتفظ بـ PanelTab لواجهة السايدبار.
        var createJwtDto = new CreateJwtDto
        {
            Id = request.Id,
            Roles = request.Roles,
            Permissions = JwtPermissionNames(request.Permissions),
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
        
        var groupedPermissions = GroupPermissionsForClient(request.Permissions);
        
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
                DeniedPermissions = x.User.UserPermissionDenials.Select(d => d.Permission).ToList()!,
                Branches = x.User.UserBranches.Select(ub => ub.Branch).ToList()!
            })
            .FirstOrDefaultAsync(x => x.RefreshToken == hashedRefreshToken, cancellationToken);

        if (userLogin is null || userLogin.RefreshTokenExpiresAt < _dateTimeProvider.UtcNowOffset)
            return UserErrors.RefreshTokenIsNotValid;
        
        var createJwtDto = new CreateJwtDto
        {
            Id = userLogin.UserId,
            Roles = userLogin.Roles,
            Permissions = JwtPermissionNames(userLogin.Permissions),
            SecurityStamp = userLogin.UserSecurityStamp,
            BranchIds = userLogin.Branches.Select(x => x.Id).ToList()
        };
        var newJwtToken = _tokenFactoryService.CreateUserJwt(createJwtDto);
        var newRefreshToken = _tokenFactoryService.CreateRefreshToken();

        await _dbContext.UserLogins
            .Where(x => x.RefreshToken == hashedRefreshToken)
            .ExecuteUpdateAsync(x => x.SetProperty(ul => ul.RefreshToken, HashRefreshToken(newRefreshToken)), cancellationToken);
        
        var groupedPermissions = GroupPermissionsForClient(userLogin.Permissions);
        
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

    /// <summary>
    /// أسماء الصلاحيات داخل JWT — بدون Panel.Tab.* (تُرسل للعميل في Permissions فقط).
    /// </summary>
    private static List<string> JwtPermissionNames(IEnumerable<PermissionEntity> permissions) =>
        permissions
            .Where(p => p is not null && string.IsNullOrWhiteSpace(p.Name) == false)
            .Select(p => p.Name)
            .Where(n => AdminPanelTabs.IsPanelTabPermission(n) == false)
            .Distinct(StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Panel.Tab.{tabKey} → module=PanelTab, action=tabKey.
    /// غير ذلك: الجزء قبل أول نقطة = module، والباقي = action.
    /// </summary>
    private static Dictionary<string, List<GetPermissionDto>> GroupPermissionsForClient(
        IEnumerable<PermissionEntity> permissions)
    {
        const string panelPrefix = "Panel.Tab.";
        return permissions
            .Where(p => string.IsNullOrWhiteSpace(p.Name) == false)
            .GroupBy(p =>
            {
                if (p.Name.StartsWith(panelPrefix, StringComparison.Ordinal))
                    return "PanelTab";
                var dot = p.Name.IndexOf('.');
                return dot > 0 ? p.Name[..dot] : p.Name;
            })
            .ToDictionary(
                g => g.Key,
                g => g.Select(p =>
                {
                    string action;
                    if (p.Name.StartsWith(panelPrefix, StringComparison.Ordinal))
                        action = p.Name[panelPrefix.Length..];
                    else
                    {
                        var dot = p.Name.IndexOf('.');
                        action = dot > 0 ? p.Name[(dot + 1)..] : p.Name;
                    }

                    return new GetPermissionDto { Id = p.Id, Name = action };
                }).ToList());
    }
}