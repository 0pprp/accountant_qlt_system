using Application.Common.Interfaces;
using Application.Common.Models.TokenFactory;

namespace Application.SalesRequests.Common;

public static class CurrentUserPermissionExtensions
{
    public static bool HasPermission(this ICurrentUserService user, string permission)
    {
        var raw = user.User?.FindFirst(CustomClaims.Permissions)?.Value;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(permission, StringComparer.Ordinal);
    }
}
