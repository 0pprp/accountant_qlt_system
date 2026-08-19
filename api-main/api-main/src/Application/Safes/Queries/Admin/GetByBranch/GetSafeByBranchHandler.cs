using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Safes.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetByBranch;

public class GetSafeByBranchHandler : IRequestHandler<GetSafeByBranchQuery, Result<GetSafeByBranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSafeByBranchHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetSafeByBranchResponse>> Handle(GetSafeByBranchQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUserService.Role == Application.Common.SeedData.Roles.Admin.Name;

        if (request.BranchId is null && isAdmin == false)
            return SafeErrors.SafeAccessDenied;

        if (request.BranchId is not null && isAdmin == false)
        {
            var userBranchIds = _currentUserService.BranchIds ?? [];
            if (userBranchIds.Contains(request.BranchId.Value) == false)
                return SafeErrors.SafeAccessDenied;
        }

        var safe = await _dbContext.Safes.AsNoTracking()
            .When(request.BranchId.HasValue, x => x.BranchId == request.BranchId)
            .When(request.BranchId.HasValue == false, x => x.BranchId == null)
            .Select(x => new GetSafeByBranchResponse
            {
                Id = x.Id,
                Name = x.Name,
                RemainingCashAmount = x.RemainingCashAmount,
                NetBalance = x.CreditAmount - x.DebitAmount,
                BranchId = x.BranchId
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        if (request.BranchId.HasValue)
        {
            var cashHolderIds = SafeCashHolderQuery.UserIdsForBranch(
                _dbContext.OrderLists,
                _dbContext.Orders,
                request.BranchId.Value);

            safe.TotalUndeliveredCashAmount = await _dbContext.Users.AsNoTracking()
                .Where(x => cashHolderIds.Contains(x.Id))
                .SumAsync(x => x.UndeliveredCashAmount, cancellationToken);
        }
        else
        {
            safe.TotalBranchesRemainingCashAmount = await _dbContext.Safes.AsNoTracking()
                .SumAsync(x => x.RemainingCashAmount, cancellationToken);
        }

        return safe;
    }
}