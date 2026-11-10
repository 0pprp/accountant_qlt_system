using System.Security.Claims;
using Application.AppNotes.Commands.Create;
using Application.AppNotes.Commands.MarkRead;
using Application.AppNotes.Queries;
using Application.Common.Models.TokenFactory;
using Domain.Entities.AppNoteAggregate;
using Domain.Entities.AppNoteAggregate.Enums;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.AppNotes;

public class AppNoteHandlerTests
{
    private static ClaimsPrincipal Principal(params string[] permissions)
        => new(new ClaimsIdentity(
            [new Claim(CustomClaims.Permissions, string.Join(",", permissions))],
            "test"));

    private static async Task<(TestApplicationDbContext db, CurrentUserServiceStub current, DateTimeProviderStub clock)>
        SetupAsync(params string[] permissions)
    {
        var db = new TestApplicationDbContext(
            new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        await db.Database.EnsureCreatedAsync();
        var user = new User("متابع اختبار", "م", "n11", DateOnly.FromDateTime(DateTime.UtcNow), "motaba", "hash", "a", "p");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var current = new CurrentUserServiceStub
        {
            UserId = user.Id,
            Role = "Motaba",
            BranchIds = [],
            User = Principal(permissions)
        };
        return (db, current, new DateTimeProviderStub());
    }

    [Fact]
    public async Task Create_sets_sender_from_session_not_body()
    {
        var (db, current, clock) = await SetupAsync(SeedPermissions.AppNote.Create);
        var handler = new CreateAppNoteHandler(db, current, clock);
        var result = await handler.Handle(new CreateAppNoteCommand
        {
            Body = "هذه ملاحظة تشغيلية كافية",
            Subject = "اختبار"
        }, CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Equal("متابع اختبار", result.Data!.SenderDisplayName);
        Assert.True(result.Data.SenderUserId > 0);
        Assert.True(result.Data.IsUnread);
        Assert.Equal(1, await db.AppNotes.CountAsync());
    }

    [Fact]
    public async Task Create_rejects_short_body_and_forbidden()
    {
        var (db, current, clock) = await SetupAsync(SeedPermissions.AppNote.Create);
        var handler = new CreateAppNoteHandler(db, current, clock);
        var shortBody = await handler.Handle(new CreateAppNoteCommand
        {
            Body = "قصير"
        }, CancellationToken.None);
        Assert.True(shortBody.IsFailed);

        var (db2, current2, clock2) = await SetupAsync();
        var denied = await new CreateAppNoteHandler(db2, current2, clock2)
            .Handle(new CreateAppNoteCommand
            {
                Body = "هذه ملاحظة تشغيلية كافية"
            }, CancellationToken.None);
        Assert.True(denied.IsFailed);
    }

    [Fact]
    public async Task Mark_read_twice_is_idempotent()
    {
        var (db, current, clock) = await SetupAsync(SeedPermissions.AppNote.Read);
        db.AppNotes.Add(new AppNote(
            AppNoteSourceApp.Delegate, null, 11, "م", "Mandob", null, null,
            "س", "نص ملاحظة كافية الطول", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var id = await db.AppNotes.Select(x => x.Id).FirstAsync();

        var handler = new MarkAppNoteReadHandler(db, current, clock);
        var first = await handler.Handle(new MarkAppNoteReadCommand { Id = id }, CancellationToken.None);
        var second = await handler.Handle(new MarkAppNoteReadCommand { Id = id }, CancellationToken.None);
        Assert.True(first.IsSucceeded);
        Assert.True(second.IsSucceeded);
        Assert.False(second.Data!.IsUnread);
        Assert.Equal(1, await db.AppNotes.CountAsync(x => x.Status == AppNoteStatus.Read));
    }

    [Fact]
    public async Task List_orders_by_created_then_id_and_filters_unread()
    {
        var (db, current, clock) = await SetupAsync(SeedPermissions.AppNote.Read);
        var t0 = DateTimeOffset.Parse("2026-11-09T10:00:00Z");
        db.AppNotes.Add(new AppNote(AppNoteSourceApp.Follower, null, 11, "أ", "Motaba", null, null, "1", "نص ملاحظة كافية الطول أ", t0));
        db.AppNotes.Add(new AppNote(AppNoteSourceApp.Delegate, null, 11, "ب", "Mandob", null, null, "2", "نص ملاحظة كافية الطول ب", t0.AddHours(1)));
        await db.SaveChangesAsync();
        var newer = await db.AppNotes.OrderByDescending(x => x.CreatedAtUtc).FirstAsync();
        newer.MarkRead(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        var handler = new ListAppNotesHandler(db, current);
        var all = await handler.Handle(new ListAppNotesQuery { Page = 1, PageSize = 20 }, CancellationToken.None);
        var unread = await handler.Handle(new ListAppNotesQuery
        {
            Page = 1,
            PageSize = 20,
            UnreadOnly = true
        }, CancellationToken.None);

        Assert.True(all.IsSucceeded);
        Assert.Equal(2, all.Data!.TotalCount);
        Assert.Equal(1, all.Data.UnreadCount);
        Assert.Equal("2", all.Data.Items[0].Subject);
        Assert.Single(unread.Data!.Items);
        Assert.Equal("1", unread.Data.Items[0].Subject);
    }

    [Fact]
    public async Task Read_forbidden_without_permission()
    {
        var (db, current, _) = await SetupAsync();
        var handler = new ListAppNotesHandler(db, current);
        var result = await handler.Handle(new ListAppNotesQuery(), CancellationToken.None);
        Assert.True(result.IsFailed);
    }
}
