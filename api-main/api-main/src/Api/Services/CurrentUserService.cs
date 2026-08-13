using System.Security.Claims;
using Application.Common.Models.TokenFactory;

namespace Api.Services;

public class CurrentUserService : ICurrentUserService
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        string? userId;
        string? role;
        string? branchIds;
        if (httpContext == null)
        {
            userId = null;
            role = null;
            branchIds = null;
        }
        else
        {
            var user = httpContext.User;
            userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            role = user.FindFirstValue(ClaimTypes.Role);
            branchIds = user.FindFirstValue(CustomClaims.BranchIds);
        }
        UserId = string.IsNullOrWhiteSpace(userId) ? null : int.Parse(userId);
        User = httpContextAccessor.HttpContext?.User;
        Role = string.IsNullOrWhiteSpace(role) ? null : role;
        BranchIds = string.IsNullOrWhiteSpace(branchIds) ? null : branchIds.Split(',').Select(int.Parse).ToList();
    }

    public int? UserId { get; }
    public ClaimsPrincipal? User { get; }
    public string? Role { get; set; }
    public List<int>? BranchIds { get; set; }
}