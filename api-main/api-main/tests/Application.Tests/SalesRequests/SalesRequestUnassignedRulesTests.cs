using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.SalesRequests;

public class SalesRequestUnassignedRulesTests
{
    [Fact]
    public void New_without_target_is_unassigned()
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        Assert.True(request.IsUnassigned());
    }

    [Fact]
    public void Pending_workflow_style_status_alone_is_not_unassigned_when_assigned()
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        request.Assign(9, DateTimeOffset.UtcNow);
        request.Status = SalesRequestStatus.Pending;
        Assert.False(request.IsUnassigned());
    }

    [Fact]
    public void Rejected_or_completed_or_legal_are_not_unassigned()
    {
        var rejected = new SalesRequest("أ", "07801112233", SalesRequestSourceType.Follower, 1, null, null, null);
        rejected.Reject("x", DateTimeOffset.UtcNow);
        Assert.False(rejected.IsUnassigned());

        var completed = new SalesRequest("ب", "07801112233", SalesRequestSourceType.Delegate, 1, null, null, null);
        completed.Status = SalesRequestStatus.Completed;
        Assert.False(completed.IsUnassigned());
    }

    [Fact]
    public void Exception_hold_without_target_is_excluded_from_unassigned_board()
    {
        var request = new SalesRequest("ج", "07801112233", SalesRequestSourceType.ExcelImport, 1, null, null, null);
        request.HoldForException(SalesRequestExceptionHoldStatus.Pending);
        Assert.False(request.IsUnassigned());
    }

    [Fact]
    public void Same_identity_keeps_single_record_across_source_change_fields()
    {
        var request = new SalesRequest("د", "07801112233", SalesRequestSourceType.ExternalCrm, 1, null, "crm-1", null);
        Assert.Equal("crm-1", request.OriginalExternalId);
        request.SourceType = SalesRequestSourceType.Manual;
        Assert.Equal("crm-1", request.OriginalExternalId);
    }
}
