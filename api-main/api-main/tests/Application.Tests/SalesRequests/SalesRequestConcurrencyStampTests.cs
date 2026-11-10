using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.SalesRequests;

public class SalesRequestConcurrencyStampTests
{
    [Fact]
    public void Assign_increments_concurrency_stamp()
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        Assert.Equal(0, request.ConcurrencyStamp);
        request.Assign(9, DateTimeOffset.UtcNow);
        Assert.Equal(1, request.ConcurrencyStamp);
        Assert.Equal(SalesRequestStatus.Assigned, request.Status);
    }

    [Fact]
    public void Reject_after_assign_increments_again()
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        request.Assign(9, DateTimeOffset.UtcNow);
        var stamp = request.ConcurrencyStamp;
        request.Reject("سبب", DateTimeOffset.UtcNow);
        Assert.Equal(stamp + 1, request.ConcurrencyStamp);
        Assert.Equal(SalesRequestStatus.Rejected, request.Status);
    }

    [Fact]
    public void Concurrent_logical_outcomes_cannot_both_remain_unassigned_after_mutations()
    {
        var a = new SalesRequest("أ", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        var b = new SalesRequest("أ", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        a.Assign(1, DateTimeOffset.UtcNow);
        b.Reject("x", DateTimeOffset.UtcNow);
        Assert.False(a.IsUnassigned());
        Assert.False(b.IsUnassigned());
        Assert.NotEqual(a.Status, b.Status);
        Assert.Equal(1, a.ConcurrencyStamp);
        Assert.Equal(1, b.ConcurrencyStamp);
    }
}
