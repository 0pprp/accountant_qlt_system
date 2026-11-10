using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Waivers.Common;

public static class WaiverConcurrency
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
            return WaiverErrors.Concurrency;
        }
    }
}
