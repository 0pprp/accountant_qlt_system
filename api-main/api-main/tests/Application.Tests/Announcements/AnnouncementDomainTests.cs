using Domain.Entities.AnnouncementAggregate;
using Domain.Entities.AnnouncementAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.Announcements;

public class AnnouncementDomainTests
{
    [Fact]
    public void Text_announcement_requires_body()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new Announcement(
                AnnouncementAppKey.Delegate, AnnouncementContentType.Text, " ",
                null, null, false, null, true, 1, 1, "م", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Image_requires_both_keys_and_http_link_when_enabled()
    {
        var a = new Announcement(
            AnnouncementAppKey.Follower, AnnouncementContentType.Image, "",
            "Attachments/Announcements/a.jpg", "Attachments/Announcements/b.jpg",
            true, "https://example.com", true, 1, 1, "م", DateTimeOffset.UtcNow);
        Assert.Equal(AnnouncementContentType.Image, a.ContentType);
        Assert.True(a.IsEnabled);

        Assert.Throws<InvalidOperationException>(() =>
            new Announcement(
                AnnouncementAppKey.Delegate, AnnouncementContentType.Image, "",
                "a.jpg", "b.jpg", true, "javascript:alert(1)", true, 1, 1, "م",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Update_is_concurrency_guarded()
    {
        var a = new Announcement(
            AnnouncementAppKey.Delegate, AnnouncementContentType.Text, "مرحبا",
            null, null, false, null, true, 1, 1, "م", DateTimeOffset.UtcNow);
        a.Update(AnnouncementContentType.Text, "مرحبا 2", null, null, false, null, false, 2, "م", 0,
            DateTimeOffset.UtcNow);
        Assert.False(a.IsEnabled);
        Assert.Throws<InvalidOperationException>(() =>
            a.SetEnabled(true, 2, "م", 0, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Permissions_outside_all_director_write_apps_read()
    {
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.Announcement.Create);
        var director = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Director.Name);
        Assert.Contains(SeedPermissions.Announcement.Create, director);
        Assert.Contains(SeedPermissions.Announcement.Read, director);

        var mandob = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.Mandob.Name);
        Assert.Contains(SeedPermissions.Announcement.AppRead, mandob.Permissions);
        Assert.DoesNotContain(SeedPermissions.Announcement.Create, mandob.Permissions);

        Assert.NotEqual(SeedPermissions.Announcement.Create, SeedPermissions.AppNote.Create);
    }

    [Theory]
    [InlineData("delegate", AnnouncementAppKey.Delegate)]
    [InlineData("follower", AnnouncementAppKey.Follower)]
    [InlineData("committee", AnnouncementAppKey.Follower)]
    [InlineData("motaba", AnnouncementAppKey.Follower)]
    public void App_key_parse(string raw, AnnouncementAppKey expected)
    {
        Assert.True(AnnouncementLabels.TryParseAppKey(raw, out var k));
        Assert.Equal(expected, k);
    }
}
