using Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskConcurrency
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
            return ManagementTaskErrors.ConcurrencyConflict;
        }
    }
}
