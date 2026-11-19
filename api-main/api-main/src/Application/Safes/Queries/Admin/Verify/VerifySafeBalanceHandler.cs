using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.SafeAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.Verify;

public class VerifySafeBalanceHandler : IRequestHandler<VerifySafeBalanceQuery, Result<VerifySafeBalanceResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public VerifySafeBalanceHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<VerifySafeBalanceResponse>> Handle(
        VerifySafeBalanceQuery request, CancellationToken cancellationToken)
    {
        var scope = await SafeAccess.EnsureSafeInScopeAsync(
            _dbContext, _currentUser, request.SafeId, cancellationToken);
        if (scope.IsFailed)
            return scope.Error;

        var safe = await _dbContext.Safes.AsNoTracking()
            .Where(x => x.Id == request.SafeId)
            .Select(x => new { x.Id, x.Name, x.Balance })
            .FirstOrDefaultAsync(cancellationToken);

        if (safe is null)
            return SafeErrors.SafeNotFound;

        var movements = await _dbContext.SafeMovements.AsNoTracking()
            .Where(x => x.SafeId == request.SafeId)
            .OrderBy(x => x.Id)
            .Select(x => new { x.Amount, x.BalanceAfter })
            .ToListAsync(cancellationToken);

        var ledgerBalance = movements.Sum(x => x.Amount);
        var difference = safe.Balance - ledgerBalance;

        var running = 0d;
        var inconsistent = 0;
        foreach (var movement in movements)
        {
            running += movement.Amount;
            if (Math.Abs(running - movement.BalanceAfter) >= Safe.Tolerance)
                inconsistent++;
        }

        return new VerifySafeBalanceResponse
        {
            SafeId = safe.Id,
            SafeName = safe.Name,
            StoredBalance = safe.Balance,
            LedgerBalance = ledgerBalance,
            Difference = difference,
            MovementsCount = movements.Count,
            IsBalanced = Math.Abs(difference) < Safe.Tolerance,
            InconsistentSnapshots = inconsistent
        };
    }
}
