using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.LegalFollowups;

public class LegalFollowupWorkflowTests
{
    [Fact]
    public void CanSend_allows_unassigned_new_and_rejected_only()
    {
        var unassigned = new SalesRequest("زبون", "07", SalesRequestSourceType.Manual, 1, null, null, 1);
        Assert.True(SalesRequest.CanSendToLegalFollowup(unassigned));

        unassigned.TargetEmployeeId = 9;
        unassigned.Status = SalesRequestStatus.Assigned;
        Assert.False(SalesRequest.CanSendToLegalFollowup(unassigned));

        var rejected = new SalesRequest("زبون", "07", SalesRequestSourceType.Manual, 1, null, null, 1)
        {
            Status = SalesRequestStatus.Rejected,
        };
        Assert.True(SalesRequest.CanSendToLegalFollowup(rejected));
    }

    [Fact]
    public void Cancel_in_progress_is_idempotent_when_already_cancelled()
    {
        Assert.True(LegalFollowupCase.CanCancel(LegalFollowupStatus.InProgress));
        Assert.False(LegalFollowupCase.CanCancel(LegalFollowupStatus.Completed));
        Assert.False(LegalFollowupCase.CanCancel(LegalFollowupStatus.Cancelled));
    }

    [Fact]
    public void Return_from_legal_cancel_sets_new_without_deleting()
    {
        var sr = new SalesRequest("زبون", "07", SalesRequestSourceType.Manual, 1, null, null, 1);
        sr.SetSubmission("عنوان", "منتج", null, Guid.NewGuid(), "fp", DateTimeOffset.UtcNow);
        sr.SendToLegalFollowup("note", DateTimeOffset.UtcNow);
        Assert.Equal(SalesRequestStatus.LegalFollowup, sr.Status);

        var ok = sr.TryReturnFromLegalFollowupCancel("سبب", DateTimeOffset.UtcNow);
        Assert.True(ok);
        Assert.Equal(SalesRequestStatus.New, sr.Status);
        Assert.False(sr.IsDeleted);
        Assert.False(sr.TryReturnFromLegalFollowupCancel("مرة ثانية", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cancel_case_requires_reason_and_bumps_version()
    {
        var c = new LegalFollowupCase(
            1, null, 2, "متابع", "زبون", "07", "عنوان", "n", 9, "a", DateTimeOffset.UtcNow);
        Assert.Equal(1, c.RowVersion);
        c.Cancel("سبب كافٍ", DateTimeOffset.UtcNow, 1);
        Assert.Equal(LegalFollowupStatus.Cancelled, c.Status);
        Assert.Equal(2, c.RowVersion);
    }
}
