using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.ManagementTasks.Realtime;

public class ManagementTaskOutboxProcessor
{
    public const int BatchSize = 50;
    public const int MaxRetryCount = 20;
    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly IApplicationDbContext _db;
    private readonly IManagementTaskRealtimePublisher _publisher;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<ManagementTaskOutboxProcessor> _logger;

    public ManagementTaskOutboxProcessor(
        IApplicationDbContext db,
        IManagementTaskRealtimePublisher publisher,
        IDateTimeProvider clock,
        ILogger<ManagementTaskOutboxProcessor> logger)
    {
        _db = db;
        _publisher = publisher;
        _clock = clock;
        _logger = logger;
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNowOffset;
        var retryCutoff = now - RetryDelay;

        var rows = await _db.ManagementTaskChangeOutboxes
            .Where(x => x.PublishedAtUtc == null
                        && x.RetryCount < MaxRetryCount
                        && (x.LastAttemptAtUtc == null || x.LastAttemptAtUtc <= retryCutoff))
            .OrderBy(x => x.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return 0;

        var published = 0;
        foreach (var row in rows)
        {
            try
            {
                await _publisher.PublishAsync(
                    row.TaskId,
                    row.Revision,
                    row.ChangeType,
                    row.BranchId,
                    row.CurrentHolderUserId,
                    row.AssigneeUserId,
                    cancellationToken);
                row.MarkPublished(_clock.UtcNowOffset);
                published++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                row.MarkFailed(_clock.UtcNowOffset, ex.GetType().Name);
                _logger.LogWarning(
                    "ManagementTask outbox publish failed. EventId={EventId} Attempt={Attempt} Error={Error}",
                    row.EventId, row.RetryCount, ex.GetType().Name);
            }
        }

        await _db.SaveChangesAsync(CancellationToken.None);
        return published;
    }
}
