using System.Security.Claims;
using Application.SalesRequests.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

[Authorize]
public class SalesRequestHub : Hub
{
    public const string Route = "/hubs/sales-requests";

    private readonly SalesRequestHubIdentityResolver _identityResolver;
    private readonly SalesRequestConnectionRegistry _registry;

    public SalesRequestHub(
        SalesRequestHubIdentityResolver identityResolver,
        SalesRequestConnectionRegistry registry)
    {
        _identityResolver = identityResolver;
        _registry = registry;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            Context.Abort();
            return;
        }

        var identity = await _identityResolver.ResolveAsync(userId.Value, Context.ConnectionAborted);
        if (identity is null)
        {
            Context.Abort();
            return;
        }

        var groups = SalesRequestHubGroups.ForIdentity(identity);
        foreach (var group in groups)
            await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);

        _registry.Set(Context.ConnectionId, userId.Value, groups);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _registry.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task<int> RefreshSubscriptions()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            Context.Abort();
            return 0;
        }

        var identity = await _identityResolver.ResolveAsync(userId.Value, Context.ConnectionAborted);
        if (identity is null)
        {
            await RemoveAllAsync();
            Context.Abort();
            return 0;
        }

        var desired = SalesRequestHubGroups.ForIdentity(identity);
        _registry.TryGet(Context.ConnectionId, out var current);
        var currentGroups = current?.Groups ?? new HashSet<string>();

        foreach (var group in currentGroups.Except(desired, StringComparer.Ordinal))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);

        foreach (var group in desired.Except(currentGroups, StringComparer.Ordinal))
            await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);

        _registry.Set(Context.ConnectionId, userId.Value, desired);
        return desired.Count;
    }

    private async Task RemoveAllAsync()
    {
        if (_registry.TryGet(Context.ConnectionId, out var current) && current is not null)
        {
            foreach (var group in current.Groups)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        }

        _registry.Remove(Context.ConnectionId);
    }

    private int? GetUserId()
    {
        var raw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) && id > 0 ? id : null;
    }
}
