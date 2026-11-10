using Domain.Entities.TutorialVideoAggregate;
using Domain.Entities.TutorialVideoAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.TutorialVideos;

public class TutorialVideoDomainTests
{
    [Fact]
    public void Rejects_non_https_and_non_allowlisted_hosts()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new TutorialVideo(
                TutorialAppKey.Delegate, "سؤال", "javascript:alert(1)", true, 1,
                1, "م", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() =>
            new TutorialVideo(
                TutorialAppKey.Delegate, "سؤال", "https://example.com/v", true, 1,
                1, "م", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() =>
            new TutorialVideo(
                TutorialAppKey.Delegate, "سؤال", "http://youtube.com/watch?v=1", true, 1,
                1, "م", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Update_is_concurrency_guarded()
    {
        var v = new TutorialVideo(
            TutorialAppKey.Follower, "كيف أبدأ؟", "https://www.youtube.com/watch?v=abc", true, 1,
            1, "م", DateTimeOffset.UtcNow);
        v.Update("كيف أبدأ؟", "https://youtu.be/abc2", false, 2, "م", 0, DateTimeOffset.UtcNow);
        Assert.False(v.IsEnabled);
        Assert.Throws<InvalidOperationException>(() =>
            v.Update("x", "https://vimeo.com/1", true, 2, "م", 0, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("delegate", TutorialAppKey.Delegate)]
    [InlineData("branch-manager", TutorialAppKey.BranchManager)]
    [InlineData("follower", TutorialAppKey.Follower)]
    [InlineData("committee", TutorialAppKey.Follower)]
    [InlineData("motaba", TutorialAppKey.Follower)]
    public void App_key_parse(string raw, TutorialAppKey expected)
    {
        Assert.True(TutorialAppKeyLabels.TryParse(raw, out var k));
        Assert.Equal(expected, k);
    }

    [Fact]
    public void Permissions_outside_all_and_seeded()
    {
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.Tutorial.Create);
        var director = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Director.Name);
        Assert.Contains(SeedPermissions.Tutorial.Create, director);

        var mandob = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.Mandob.Name);
        Assert.Contains(SeedPermissions.Tutorial.AppRead, mandob.Permissions);

        var motaba = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.Motaba.Name);
        Assert.Contains(SeedPermissions.Tutorial.AppRead, motaba.Permissions);
        Assert.Contains(SeedPermissions.Announcement.AppRead, motaba.Permissions);
    }
}
