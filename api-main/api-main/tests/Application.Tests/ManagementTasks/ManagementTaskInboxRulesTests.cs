using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.Tests.ManagementTasks;

public class ManagementTaskInboxRulesTests
{
    [Fact]
    public void Holder_sees_open_task()
    {
        Assert.True(ManagementTaskInboxRules.MatchesInboxForMe(10, 10, 10, ManagementTaskStatus.Accepted));
        Assert.True(ManagementTaskInboxRules.MatchesInboxForMe(20, 20, 10, ManagementTaskStatus.Transferred));
    }

    [Fact]
    public void Assignee_sees_returned_and_awaiting_confirm()
    {
        Assert.True(ManagementTaskInboxRules.MatchesInboxForMe(10, 20, 10, ManagementTaskStatus.Returned));
        Assert.True(ManagementTaskInboxRules.MatchesInboxForMe(10, 10, 10, ManagementTaskStatus.AwaitingDelegatedConfirm));
        Assert.False(ManagementTaskInboxRules.MatchesInboxForMe(10, 20, 10, ManagementTaskStatus.Accepted));
    }

    [Fact]
    public void Default_open_excludes_completed_only()
    {
        Assert.True(ManagementTaskInboxRules.IsDefaultOpenStatus(ManagementTaskStatus.Returned));
        Assert.False(ManagementTaskInboxRules.IsDefaultOpenStatus(ManagementTaskStatus.Completed));
    }
}
