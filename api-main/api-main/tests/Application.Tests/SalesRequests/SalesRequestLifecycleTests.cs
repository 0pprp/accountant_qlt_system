using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;

namespace Application.Tests.SalesRequests;

public class SalesRequestLifecycleTests
{
    private static SalesRequest NewRequest() =>
        new("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);

    [Fact]
    public void Assign_sets_owner_and_leaves_unassigned_board()
    {
        var r = NewRequest();
        var at = DateTimeOffset.UtcNow;
        r.Assign(10, at);
        Assert.False(r.IsUnassigned());
        Assert.Equal(10, r.TargetEmployeeId);
        Assert.Equal(SalesRequestStatus.Assigned, r.Status);
        Assert.True(r.IsOwnedBy(10));
        Assert.False(r.IsOwnedBy(99));
    }

    [Fact]
    public void Reassign_blocks_previous_owner_semantics()
    {
        var r = NewRequest();
        r.Assign(1, DateTimeOffset.UtcNow);
        r.Reassign(2, "نقل", DateTimeOffset.UtcNow);
        Assert.True(r.IsOwnedBy(2));
        Assert.False(r.IsOwnedBy(1));
        Assert.Equal(SalesRequestStatus.Assigned, r.Status);
    }

    [Fact]
    public void View_is_idempotent_for_status_after_first()
    {
        var r = NewRequest();
        r.Assign(1, DateTimeOffset.UtcNow);
        Assert.True(r.MarkViewed(DateTimeOffset.UtcNow));
        Assert.Equal(SalesRequestStatus.Viewed, r.Status);
        var stamp = r.ConcurrencyStamp;
        Assert.False(r.MarkViewed(DateTimeOffset.UtcNow));
        Assert.Equal(SalesRequestStatus.Viewed, r.Status);
        Assert.Equal(stamp + 1, r.ConcurrencyStamp);
    }

    [Fact]
    public void Illegal_complete_without_linked_order()
    {
        var r = NewRequest();
        r.Assign(1, DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => r.CompleteFromSale(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void LinkDraft_idempotent_same_order_and_rejects_other()
    {
        var r = NewRequest();
        r.Assign(1, DateTimeOffset.UtcNow);
        r.LinkDraftOrder(50, DateTimeOffset.UtcNow);
        Assert.Equal(50, r.LinkedOrderId);
        Assert.Equal(SalesRequestStatus.ConvertedToSale, r.Status);
        r.LinkDraftOrder(50, DateTimeOffset.UtcNow);
        Assert.Equal(50, r.LinkedOrderId);
        Assert.Throws<InvalidOperationException>(() => r.LinkDraftOrder(51, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Incoming_tab_excludes_SalesRequest_Pending()
    {
        var incoming = SalesRequestTabMap.StatusesForTab(SalesRequestTabMap.Incoming);
        Assert.Contains(SalesRequestStatus.Assigned, incoming);
        Assert.DoesNotContain(SalesRequestStatus.Pending, incoming);
        var pendingTab = SalesRequestTabMap.StatusesForTab(SalesRequestTabMap.Pending);
        Assert.Equal([SalesRequestStatus.Pending], pendingTab);
    }

    [Fact]
    public void RequestId_is_not_orderId()
    {
        var r = NewRequest();
        r.Assign(1, DateTimeOffset.UtcNow);
        r.LinkDraftOrder(777, DateTimeOffset.UtcNow);
        Assert.NotEqual(r.Id, r.LinkedOrderId);
        // Id may be 0 before persist — distinct fields nonetheless
        Assert.Equal(777, r.LinkedOrderId);
    }

    [Fact]
    public void Concurrent_assign_and_reject_cannot_both_stay_unassigned()
    {
        var a = NewRequest();
        var b = NewRequest();
        a.Assign(1, DateTimeOffset.UtcNow);
        b.Reject("x", DateTimeOffset.UtcNow, clearAssignee: true);
        Assert.False(a.IsUnassigned());
        Assert.False(b.IsUnassigned());
    }
}
