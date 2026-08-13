using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Permissions.Queries.Admin.GetAll;

public class GetAllPermissionsHandler : IRequestHandler<GetAllPermissionsQuery, Result<GetAllPermissionsResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllPermissionsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetAllPermissionsResponse>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions.AsNoTracking()
            .Select(x => new GetAllPermissionsItem
            {
                Id = x.Id,
                Name = x.Name,
                DisplayName = x.DisplayName,
                Scope = x.Scope,
                ScopeDisplayName = x.ScopeDisplayName
            })
            .ToListAsync(cancellationToken);

        return new GetAllPermissionsResponse(permissions
            .GroupBy(p => p.Name.Split('.')[0])
            .ToDictionary(
                group => group.Key,
                group => group.Select(p => new GetAllPermissionsItem
                {
                    Id = p.Id,
                    Name = p.Name.Split('.')[1],
                    DisplayName = p.DisplayName,
                    Scope = p.Scope,
                    ScopeDisplayName = p.ScopeDisplayName
                }).ToList()
            ));
    }
}