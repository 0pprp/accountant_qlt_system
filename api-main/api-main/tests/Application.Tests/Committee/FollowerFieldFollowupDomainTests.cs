using Domain.Entities.FollowerWorkAggregate;

namespace Application.Tests.Committee;

public class FollowerFieldFollowupDomainTests
{
    [Fact]
    public void Complete_requires_note_and_transitions_pending()
    {
        var row = new FollowerFieldFollowup(
            followerUserId: 7,
            listId: 3,
            listName: "قائمة أ",
            customerId: 11,
            customerName: "زبون",
            phoneNumber: "07",
            createdAtUtc: DateTimeOffset.UtcNow);

        Assert.True(row.IsPending);
        Assert.Throws<InvalidOperationException>(() => row.Complete("  ", DateTimeOffset.UtcNow));

        row.Complete("تمت الزيارة", DateTimeOffset.UtcNow);
        Assert.False(row.IsPending);
        Assert.Equal(FollowerFieldFollowup.StatusCompleted, row.Status);
        Assert.Equal("تمت الزيارة", row.CompletionNote);
        Assert.NotNull(row.CompletedAtUtc);

        Assert.Throws<InvalidOperationException>(() => row.Complete("مرة أخرى", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Ctor_rejects_incomplete_ids()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new FollowerFieldFollowup(0, 1, null, 1, null, null, DateTimeOffset.UtcNow));
    }
}
