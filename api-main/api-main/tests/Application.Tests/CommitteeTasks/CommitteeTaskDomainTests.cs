using Domain.Entities.CommitteeTaskAggregate;

namespace Application.Tests.CommitteeTasks;

public class CommitteeTaskDomainTests
{
    [Fact]
    public void Create_sets_body_enabled_and_zero_stamp()
    {
        var at = DateTimeOffset.Parse("2026-11-09T10:00:00Z");
        var item = new CommitteeTaskItem("نص المهمة", true, 2, 7, "مدير", at);
        Assert.Equal("نص المهمة", item.Body);
        Assert.True(item.IsEnabled);
        Assert.Equal(2, item.SortOrder);
        Assert.Equal(0, item.ConcurrencyStamp);
        Assert.Equal(at, item.CreatedAtUtc);
    }

    [Fact]
    public void Update_bumps_concurrency_and_actor()
    {
        var item = new CommitteeTaskItem("قديم", true, 0, 1, "أ", DateTimeOffset.UtcNow);
        var later = DateTimeOffset.UtcNow.AddMinutes(1);
        item.Update("جديد", false, 9, "ب", later);
        Assert.Equal("جديد", item.Body);
        Assert.False(item.IsEnabled);
        Assert.Equal(1, item.ConcurrencyStamp);
        Assert.Equal(9, item.UpdatedByUserId);
        Assert.Equal("ب", item.UpdatedByName);
        Assert.True(item.MatchesExpectedVersion(1));
        Assert.False(item.MatchesExpectedVersion(0));
    }
}
