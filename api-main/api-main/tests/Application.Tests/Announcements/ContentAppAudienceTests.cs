using Application.Announcements.Common;
using Application.Announcements.Queries;
using Application.Common.Content;
using Application.TutorialVideos.Common;
using Application.TutorialVideos.Queries;
using Domain.Entities.AnnouncementAggregate;
using Domain.Entities.AnnouncementAggregate.Enums;
using Domain.Entities.TutorialVideoAggregate;
using Domain.Entities.TutorialVideoAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.Announcements;

public class ContentAppAudienceTests
{
    [Fact]
    public async Task Mandob_cannot_read_follower_announcements()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.Announcement.AppRead, role: "Mandob");
        var result = await new ListPublicAnnouncementsHandler(db, current, new DateTimeProviderStub())
            .Handle(new ListPublicAnnouncementsQuery { AppKey = "follower" }, default);
        Assert.True(result.IsFailed);
        Assert.Equal("Announcement.AudienceDenied", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Motaba_can_read_follower_announcements_only()
    {
        await using var db = CreateDb();
        db.Announcements.Add(new Announcement(
            AnnouncementAppKey.Follower, AnnouncementContentType.Text,
            "نص كافٍ للإعلان", null, null, false, null, true, 1,
            1, "م", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Announcement.AppRead, role: "Motaba");
        var ok = await new ListPublicAnnouncementsHandler(db, current, new DateTimeProviderStub())
            .Handle(new ListPublicAnnouncementsQuery { AppKey = "follower" }, default);
        Assert.True(ok.IsSucceeded);
        Assert.Single(ok.Data!);

        var denied = await new ListPublicAnnouncementsHandler(db, current, new DateTimeProviderStub())
            .Handle(new ListPublicAnnouncementsQuery { AppKey = "delegate" }, default);
        Assert.Equal("Announcement.AudienceDenied", denied.Error!.Value.Code);
    }

    [Fact]
    public async Task Mandob_cannot_read_sales_employee_tutorials()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.Tutorial.AppRead, role: "Mandob");
        var result = await new ListPublicTutorialVideosHandler(db, current, new DateTimeProviderStub())
            .Handle(new ListPublicTutorialVideosQuery { AppKey = "sales-employee" }, default);
        Assert.Equal("Tutorial.AudienceDenied", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Branch_manager_reads_own_tutorial_audience()
    {
        await using var db = CreateDb();
        db.TutorialVideos.Add(new TutorialVideo(
            TutorialAppKey.BranchManager, "كيف؟", "https://www.youtube.com/watch?v=1",
            true, 1, 1, "م", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Tutorial.AppRead, role: "BranchManager");
        var ok = await new ListPublicTutorialVideosHandler(db, current, new DateTimeProviderStub())
            .Handle(new ListPublicTutorialVideosQuery { AppKey = "branch-manager" }, default);
        Assert.True(ok.IsSucceeded);
        Assert.Single(ok.Data!);
    }

    [Fact]
    public void Resolve_helpers_match_roles()
    {
        Assert.True(ContentAppAudience.AllowsAnnouncement(
            AnnouncementAppKey.Delegate, AnnouncementAppKey.Delegate));
        Assert.False(ContentAppAudience.AllowsAnnouncement(
            AnnouncementAppKey.Delegate, AnnouncementAppKey.Follower));
        Assert.True(ContentAppAudience.AllowsTutorial(
            TutorialAppKey.SalesEmployee, TutorialAppKey.SalesEmployee));
    }

    private static TestApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CurrentUserServiceStub User(string permissions, string role, int userId = 9) =>
        new()
        {
            UserId = userId,
            Role = role,
            BranchIds = [1],
            User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                [
                    new System.Security.Claims.Claim(
                        Application.Common.Models.TokenFactory.CustomClaims.Permissions, permissions)
                ], "test"))
        };
}
