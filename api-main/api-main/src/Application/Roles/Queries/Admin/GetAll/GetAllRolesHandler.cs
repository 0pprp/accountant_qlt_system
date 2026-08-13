using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Roles.Queries.Admin.GetAll;

public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery, Result<List<GetAllRolesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllRolesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<GetAllRolesResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles.AsNoTracking()
            .Select(x => new GetAllRolesResponse
            {
                Id = x.Id,
                Name = x.Name,
                DisplayName = x.DisplayName
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}