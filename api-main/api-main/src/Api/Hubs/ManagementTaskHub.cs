using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

[Authorize]
public class ManagementTaskHub : Hub
{
    public const string Route = "/hubs/management-tasks";
    public const string ChangedMethod = "managementTaskChanged";

    public override async Task OnConnectedAsync()
    {
        var branchClaim = Context.User?.FindFirst("branchIds")?.Value
            ?? Context.User?.FindFirst("BranchIds")?.Value;
        if (string.IsNullOrWhiteSpace(branchClaim) == false)
        {
            foreach (var part in branchClaim.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(part, out var branchId) && branchId > 0)
                    await Groups.AddToGroupAsync(Context.ConnectionId, BranchGroup(branchId));
            }
        }

        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId) == false)
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

        await base.OnConnectedAsync();
    }

    public static string BranchGroup(int branchId) => $"mt-branch-{branchId}";
    public static string UserGroup(string userId) => $"mt-user-{userId}";
}
