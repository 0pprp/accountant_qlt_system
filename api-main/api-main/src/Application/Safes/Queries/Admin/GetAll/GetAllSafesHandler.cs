using Application.Common.Interfaces;
using Application.Safes.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetAll;

public class GetAllSafesHandler : IRequestHandler<GetAllSafesQuery, Result<List<GetAllSafesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetAllSafesHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<List<GetAllSafesResponse>>> Handle(
        GetAllSafesQuery request, CancellationToken cancellationToken)
    {
        var scoped = SafeAccess.EnsureBranchInScope(_currentUser, request.BranchId);
        if (scoped.IsFailed)
            return scoped.Error;

        var safes = await _dbContext.Safes.AsNoTracking()
            .Where(x => x.BranchId == request.BranchId)
            .Select(x => new GetAllSafesResponse
            {
                Id = x.Id,
                Name = x.Name,
                Kind = x.Kind,
                BranchId = x.BranchId,
                OrderListId = x.OrderListId,
                OrderListName = x.OrderList != null ? x.OrderList.Name : null,
                UserId = x.UserId,
                UserFullName = x.User != null ? x.User.FullName : null,
                Balance = x.Balance,
                IsSystem = x.IsSystem,
                IsActive = x.IsActive,
                MovementsCount = x.Movements.Count
            })
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return safes;
    }
}
