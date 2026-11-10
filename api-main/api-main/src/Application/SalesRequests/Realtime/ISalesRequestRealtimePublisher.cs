namespace Application.SalesRequests.Realtime;

public interface ISalesRequestRealtimePublisher
{
    Task PublishAsync(
        SalesRequestChangedPayload payload,
        IReadOnlyCollection<string> groups,
        CancellationToken cancellationToken);
}
