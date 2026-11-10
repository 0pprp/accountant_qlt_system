using Application.SalesRequests.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SalesRequestOutboxDispatcher : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(1500);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SalesRequestOutboxDispatcher> _logger;

    public SalesRequestOutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<SalesRequestOutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        try
        {
            do
            {
                await DispatchOnceAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task DispatchOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            int processed;
            do
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<SalesRequestOutboxProcessor>();
                processed = await processor.ProcessBatchAsync(stoppingToken);
            } while (processed >= SalesRequestOutboxProcessor.BatchSize && stoppingToken.IsCancellationRequested == false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning("SalesRequest outbox dispatch cycle failed. Error={Error}", ex.GetType().Name);
        }
    }
}
