namespace Application.Users.Queries.Admin.GetPermissionOverrides;

public record GetUserPermissionOverridesQuery(int UserId)
    : IRequest<Result<GetUserPermissionOverridesResponse>>;
