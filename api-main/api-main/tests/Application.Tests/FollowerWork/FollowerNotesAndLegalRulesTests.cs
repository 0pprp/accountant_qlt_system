using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.FollowerWork;

public class FollowerNotesAndLegalRulesTests
{
    [Fact]
    public void Notes_read_does_not_grant_write()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.NotesRead,
            SeedPermissions.FollowerWork.NotesWrite);
    }

    [Fact]
    public void Old_customers_read_does_not_grant_cancel()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.OldCustomersRead,
            SeedPermissions.LegalFollowup.Cancel);
    }

    [Theory]
    [InlineData(LegalFollowupStatus.InProgress, true)]
    [InlineData(LegalFollowupStatus.Completed, false)]
    [InlineData(LegalFollowupStatus.Cancelled, false)]
    public void Cancel_only_in_progress(LegalFollowupStatus status, bool ok)
        => Assert.Equal(ok, LegalFollowupCase.CanCancel(status));

    [Fact]
    public void Cancel_requires_reason_and_bumps_version()
    {
        var c = new LegalFollowupCase(
            10, 1, 5, "متابع", "زبون", "07", "عنوان", "ملاحظة",
            2, "مدير", DateTimeOffset.UtcNow);
        Assert.Equal(1, c.RowVersion);
        c.Cancel("سبب الإلغاء", DateTimeOffset.UtcNow, expectedVersion: 1);
        Assert.Equal(LegalFollowupStatus.Cancelled, c.Status);
        Assert.Equal(2, c.RowVersion);
        Assert.Equal("سبب الإلغاء", c.CompletionNote);
    }

    [Fact]
    public void Cancel_rejects_stale_version()
    {
        var c = new LegalFollowupCase(
            10, 1, 5, "متابع", "زبون", "07", "عنوان", null,
            2, "مدير", DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            c.Cancel("سبب", DateTimeOffset.UtcNow, expectedVersion: 99));
    }

    [Fact]
    public void Second_cancel_on_cancelled_is_domain_reject()
    {
        var c = new LegalFollowupCase(
            10, 1, 5, "متابع", "زبون", "07", "عنوان", null,
            2, "مدير", DateTimeOffset.UtcNow);
        c.Cancel("أول", DateTimeOffset.UtcNow, 1);
        Assert.Throws<InvalidOperationException>(() =>
            c.Cancel("ثاني", DateTimeOffset.UtcNow, 2));
    }

    [Fact]
    public void Sales_request_returns_from_legal_to_new_once()
    {
        var sr = new SalesRequest("زبون", "07", SalesRequestSourceType.Manual, 1, null, null, 1);
        sr.SendToLegalFollowup("إرسال", DateTimeOffset.UtcNow);
        Assert.Equal(SalesRequestStatus.LegalFollowup, sr.Status);
        Assert.True(sr.TryReturnFromLegalFollowupCancel("إلغاء", DateTimeOffset.UtcNow));
        Assert.Equal(SalesRequestStatus.New, sr.Status);
        Assert.Null(sr.TargetEmployeeId);
        Assert.False(sr.TryReturnFromLegalFollowupCancel("مرة أخرى", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Arabic_labels_match_source()
    {
        Assert.Equal("قيد العمل", LegalFollowupStatusLabels.Arabic(LegalFollowupStatus.InProgress));
        Assert.Equal("تمت المتابعة", LegalFollowupStatusLabels.Arabic(LegalFollowupStatus.Completed));
        Assert.Equal("ملغاة", LegalFollowupStatusLabels.Arabic(LegalFollowupStatus.Cancelled));
    }
}
