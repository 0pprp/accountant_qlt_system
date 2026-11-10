using Application.SalesRequests.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class SignalRSalesRequestRealtimePublisher : ISalesRequestRealtimePublisher
{
    private readonly IHubContext<SalesRequestHub> _hubContext;

    public SignalRSalesRequestRealtimePublisher(IHubContext<SalesRequestHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(
        SalesRequestChangedPayload payload,
        IReadOnlyCollection<string> groups,
        CancellationToken cancellationToken)
    {
        return _hubContext.Clients
            .Groups(groups.ToList())
            .SendAsync(SalesRequestChangedPayload.HubMethodName, payload, cancellationToken);
    }
}
