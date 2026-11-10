using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.LegalFollowups;

public class LegalFollowupConcurrencyAndCancelTests
{
    [Fact]
    public void Cancel_with_stale_version_throws()
    {
        var c = new LegalFollowupCase(
            1, null, 2, "متابع", "زبون", "07", "عنوان", "n", 9, "a", DateTimeOffset.UtcNow);
        Assert.Equal(1, c.RowVersion);
        Assert.Throws<InvalidOperationException>(() =>
            c.Cancel("سبب كافٍ", DateTimeOffset.UtcNow, expectedVersion: 99));
        Assert.Equal(LegalFollowupStatus.InProgress, c.Status);
    }

    [Fact]
    public void Cancel_does_not_delete_customer_or_request()
    {
        var sr = new SalesRequest("زبون", "07", SalesRequestSourceType.Manual, 1, null, null, 1);
        sr.SetSubmission("عنوان", "منتج", null, Guid.NewGuid(), "fp", DateTimeOffset.UtcNow);
        sr.SendToLegalFollowup("note", DateTimeOffset.UtcNow);

        var c = new LegalFollowupCase(
            sr.Id == 0 ? 1 : sr.Id,
            sr.ExistingCustomerId,
            2,
            "متابع",
            sr.CustomerName,
            sr.CustomerPhone!,
            sr.Address!,
            "n",
            9,
            "a",
            DateTimeOffset.UtcNow);

        c.Cancel("سبب كافٍ للإلغاء", DateTimeOffset.UtcNow, 1);
        Assert.True(sr.TryReturnFromLegalFollowupCancel("سبب", DateTimeOffset.UtcNow));
        Assert.Equal(SalesRequestStatus.New, sr.Status);
        Assert.False(sr.IsDeleted);
        Assert.NotNull(sr.CustomerName);
        Assert.Equal(LegalFollowupStatus.Cancelled, c.Status);
    }

    [Fact]
    public void Second_return_from_legal_is_no_op()
    {
        var sr = new SalesRequest("زبون", "07", SalesRequestSourceType.Manual, 1, null, null, 1);
        sr.SetSubmission("عنوان", "منتج", null, Guid.NewGuid(), "fp", DateTimeOffset.UtcNow);
        sr.SendToLegalFollowup("note", DateTimeOffset.UtcNow);
        Assert.True(sr.TryReturnFromLegalFollowupCancel("مرة", DateTimeOffset.UtcNow));
        Assert.False(sr.TryReturnFromLegalFollowupCancel("مرة ثانية", DateTimeOffset.UtcNow));
        Assert.Equal(SalesRequestStatus.New, sr.Status);
    }
}
