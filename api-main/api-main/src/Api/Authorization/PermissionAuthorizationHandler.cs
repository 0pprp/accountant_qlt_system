using Application.Common.Models.TokenFactory;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissionsInString = context.User.Claims.FirstOrDefault(x => x.Type == CustomClaims.Permissions)?.Value;
        if (permissionsInString is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var permissions = permissionsInString.Split(',').ToList();

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
        else
            context.Fail();

        return Task.CompletedTask;
    }
}