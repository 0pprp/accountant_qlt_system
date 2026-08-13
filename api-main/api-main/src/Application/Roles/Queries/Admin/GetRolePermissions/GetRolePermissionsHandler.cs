using Application.Common.Interfaces;
using Application.Permissions.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Roles.Queries.Admin.GetRolePermissions;

public class GetRolePermissionsHandler : IRequestHandler<GetRolePermissionsQuery, Result<GetRolePermissionsResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRolePermissionsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetRolePermissionsResponse>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions.AsNoTracking()
            .Where(x => x.RolePermissions.Any(rp => rp.RoleId == request.RoleId))
            .Select(x => new GetPermissionDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);

        return new GetRolePermissionsResponse(permissions
            .GroupBy(p => p.Name.Split('.')[0])
            .ToDictionary(
                group => group.Key,
                group => group.Select(p => new GetPermissionDto
                {
                    Id = p.Id,
                    Name = p.Name.Split('.')[1]
                }).ToList()
            ));
    }
}