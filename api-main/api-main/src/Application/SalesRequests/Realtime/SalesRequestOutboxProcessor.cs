using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.SalesRequests.Realtime;

public class SalesRequestOutboxProcessor
{
    public const int BatchSize = 50;
    public const int MaxRetryCount = 20;
    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly IApplicationDbContext _db;
    private readonly ISalesRequestRealtimePublisher _publisher;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<SalesRequestOutboxProcessor> _logger;

    public SalesRequestOutboxProcessor(
        IApplicationDbContext db,
        ISalesRequestRealtimePublisher publisher,
        IDateTimeProvider clock,
        ILogger<SalesRequestOutboxProcessor> logger)
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

        var rows = await _db.SalesRequestChangeOutboxes
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
            var payload = SalesRequestChangedPayload.From(row);
            var groups = SalesRequestHubGroups.ForChange(
                row.BranchId, row.TargetEmployeeId, row.PreviousTargetEmployeeId);

            try
            {
                await _publisher.PublishAsync(payload, groups, cancellationToken);
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
                    "SalesRequest outbox publish failed. EventId={EventId} Attempt={Attempt} Error={Error}",
                    row.EventId, row.RetryCount, ex.GetType().Name);
            }
        }

        await _db.SaveChangesAsync(CancellationToken.None);
        return published;
    }
}
