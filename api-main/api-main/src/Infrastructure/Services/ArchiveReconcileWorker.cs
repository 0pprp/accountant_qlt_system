using Application.Archive.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// المرجع الذي يضمن تمام الأرشيف. الخطّاف في مسار البيع يُسرّع الظهور فقط،
/// وهذا يُصلح ما فاته — فلا يعتمد تمام البيانات على نجاح خطّاف.
/// دورة كل دقيقة بأرضية يومين، فلا يمسح التاريخ كلّه في كل مرة.
/// </summary>
public class ArchiveReconcileWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan Floor = TimeSpan.FromDays(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ArchiveReconcileWorker> _logger;

    public ArchiveReconcileWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ArchiveReconcileWorker> logger)
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
            await using var scope = _scopeFactory.CreateAsyncScope();
            var reconciler = scope.ServiceProvider.GetRequiredService<ArchiveReconciler>();

            var report = await reconciler.RunAsync(DateTimeOffset.UtcNow - Floor, stoppingToken);

            if (report.Created > 0 || report.Voided > 0 || report.Revived > 0)
            {
                _logger.LogInformation(
                    "Archive reconcile: created={Created} voided={Voided} revived={Revived}",
                    report.Created, report.Voided, report.Revived);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Archive reconcile cycle failed. Error={Error}", ex.GetType().Name);
        }
    }
}
