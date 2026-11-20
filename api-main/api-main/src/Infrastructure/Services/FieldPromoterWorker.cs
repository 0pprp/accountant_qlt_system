using Application.FieldPromoters.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>كل دقيقتين: إغلاق دوام الأيام السابقة، وتنبيهات الوقوف، وتجميد الأيام، والاحتفاظ بنقاط الموقع.</summary>
public class FieldPromoterWorker(IServiceScopeFactory scopeFactory, ILogger<FieldPromoterWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        try
        {
            do
            {
                await RunOnceAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var maintenance = scope.ServiceProvider.GetRequiredService<FieldPromoterMaintenance>();
            var report = await maintenance.RunAsync(DateTimeOffset.UtcNow, stoppingToken);
            if (report.RaisedAlerts > 0 || report.ClosedShifts > 0 || report.FrozenDays > 0 || report.DeletedPoints > 0)
                logger.LogInformation(
                    "Field promoters: closed={Closed} alerts={Raised}/{Resolved} frozen={Frozen} deletedPoints={Deleted}",
                    report.ClosedShifts, report.RaisedAlerts, report.ResolvedAlerts, report.FrozenDays, report.DeletedPoints);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogWarning("Field promoter cycle failed. Error={Error}", ex.GetType().Name);
        }
    }
}
