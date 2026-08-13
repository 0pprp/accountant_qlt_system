using System.Security.Claims;
using System.Security.Principal;
using Application.Common.Interfaces;
using Application.Common.Models.TokenFactory;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class TokenValidator
{
    private readonly IApplicationDbContext _dbContext;

    public TokenValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ValidateSecurityStamp(IPrincipal? principal)
    {
        if (int.TryParse(principal?.Identity?.GetUserClaimValue(ClaimTypes.NameIdentifier), out var id) == false)
            return false;

        if (Guid.TryParse(principal.Identity?.GetUserClaimValue(CustomClaims.SecurityStamp), out var securityStamp) == false)
            return false;

        return await _dbContext.Users.AnyAsync(x => x.Id == id && x.SecurityStamp == securityStamp);
    }
}