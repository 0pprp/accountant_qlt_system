using Application.Common.Interfaces;
using Application.Common.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Models.TokenFactory;

namespace Infrastructure.Services;

public class TokenFactoryService : ITokenFactoryService
{
    private readonly ISecurityService _securityService;
    private readonly BearerTokenSettings _bearerTokenSettings;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenFactoryService(ISecurityService securityService, BearerTokenSettings bearerTokenSettings,
        IDateTimeProvider dateTimeProvider)
    {
        _securityService = securityService;
        _bearerTokenSettings = bearerTokenSettings;
        _dateTimeProvider = dateTimeProvider;
    }


    public string CreateRefreshToken() => _securityService.CreateRandomString();

    public string CreateUserJwt(CreateJwtDto createJwtDto)
    {
        var claims = CreateUserAccessTokenClaims(createJwtDto);

        return CreateAccessToken(claims);
    }

    private static List<Claim> CreateUserAccessTokenClaims(CreateJwtDto createJwtDto)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(ClaimTypes.NameIdentifier, createJwtDto.Id.ToString()),
            new(CustomClaims.SecurityStamp, createJwtDto.SecurityStamp.ToString()),
            new(ClaimTypes.Role, string.Join(",", createJwtDto.Roles))
        };

        if (createJwtDto.Permissions.Count != 0)
            claims.Add(new Claim(CustomClaims.Permissions, string.Join(",", createJwtDto.Permissions)));

        if (createJwtDto.BranchIds.Count != 0)
            claims.Add(new Claim(CustomClaims.BranchIds, string.Join(",", createJwtDto.BranchIds)));

        return claims;
    }

    private static readonly JwtSecurityTokenHandler JwtHandler = new();

    private string CreateAccessToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerTokenSettings.SecretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var now = _dateTimeProvider.Now;

        var token = new JwtSecurityToken(
            issuer: _bearerTokenSettings.Issuer,
            audience: _bearerTokenSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_bearerTokenSettings.AccessTokenExpirationMinutes),
            signingCredentials: signingCredentials);

        return JwtHandler.WriteToken(token);
    }
}