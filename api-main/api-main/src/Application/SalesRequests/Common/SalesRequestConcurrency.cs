using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Common;

public static class SalesRequestConcurrency
{
    public static async Task<Result> SaveWithConflictGuardAsync(
        Func<CancellationToken, Task> saveAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            await saveAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return SalesRequestErrors.ConcurrencyConflict;
        }
    }
}
