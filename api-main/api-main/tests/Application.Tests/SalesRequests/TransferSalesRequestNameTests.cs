using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.SalesRequests;

public class TransferSalesRequestNameTests
{
    private static SalesRequest OwnedBy(int employeeId)
    {
        var r = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.EmployeeSubmitted, 1, null, null, null);
        r.Assign(employeeId, DateTimeOffset.UtcNow);
        return r;
    }

    [Fact]
    public void Reassign_revokes_previous_owner()
    {
        var r = OwnedBy(11);
        r.Reassign(22, "نقل لزميل", DateTimeOffset.UtcNow);
        Assert.True(r.IsOwnedBy(22));
        Assert.False(r.IsOwnedBy(11));
        Assert.Equal(SalesRequestStatus.Assigned, r.Status);
        Assert.Equal("نقل لزميل", r.LastStatusNote);
        Assert.Null(r.ViewedAtUtc);
    }

    [Fact]
    public void Terminal_completed_cannot_employee_transition()
    {
        var r = OwnedBy(11);
        r.LinkDraftOrder(90, DateTimeOffset.UtcNow);
        r.CompleteFromSale(DateTimeOffset.UtcNow);
        Assert.False(r.CanEmployeeTransition());
    }

    [Fact]
    public void Prepared_can_still_reassign()
    {
        var r = OwnedBy(11);
        r.Prepare("جاهز", DateTimeOffset.UtcNow);
        Assert.True(r.CanEmployeeTransition());
        r.Reassign(33, "سبب", DateTimeOffset.UtcNow);
        Assert.True(r.IsOwnedBy(33));
    }
}
