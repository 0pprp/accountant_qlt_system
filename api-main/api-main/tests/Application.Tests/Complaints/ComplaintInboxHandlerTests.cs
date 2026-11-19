using System.Security.Claims;
using Application.Common.Models.TokenFactory;
using Application.Complaints.Commands;
using Application.Complaints.Queries;
using Domain.Entities.ComplaintAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.Complaints;

public class ComplaintInboxHandlerTests
{
    [Fact]
    public async Task List_search_matches_subject_body_and_sender_case_insensitive()
    {
        await using var db = CreateDb();
        db.ComplaintMessages.AddRange(
            Make(1, "Delivery Delay", "نص رسالة عادي بدون كلمة", "أحمد"),
            Make(1, "عنوان", "the DELIVERY truck was late today", "علي"),
            Make(1, "عنوان آخر", "نص رسالة عادي بدون كلمة", "delivery team"),
            Make(1, "غير مطابق", "نص رسالة عادي بدون كلمة", "سعد"));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.Read, [1]);
        var result = await new ListComplaintsHandler(db, current)
            .Handle(new ListComplaintsQuery { Search = "  delivery " }, default);

        Assert.True(result.IsSucceeded);
        Assert.Equal(3, result.Data!.TotalCount);
        Assert.DoesNotContain(result.Data.Items, x => x.Subject == "غير مطابق");
    }

    [Fact]
    public async Task List_defaults_to_page_size_20_newest_first_and_clamps_to_100()
    {
        await using var db = CreateDb();
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        for (var i = 0; i < 25; i++)
            db.ComplaintMessages.Add(Make(1, $"م{i}", "نص رسالة عادي للاختبار", "مرسل", start.AddMinutes(i)));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.Read, [1]);
        var h = new ListComplaintsHandler(db, current);

        var first = await h.Handle(new ListComplaintsQuery(), default);
        Assert.Equal(20, first.Data!.PageSize);
        Assert.Equal(20, first.Data.Items.Count);
        Assert.Equal(25, first.Data.TotalCount);
        Assert.Equal("م24", first.Data.Items[0].Subject);

        var big = await h.Handle(new ListComplaintsQuery { PageSize = 500 }, default);
        Assert.Equal(100, big.Data!.PageSize);
    }

    [Fact]
    public async Task List_respects_branch_scope_and_branch_filter()
    {
        await using var db = CreateDb();
        db.ComplaintMessages.AddRange(
            Make(1, "فرع1", "نص رسالة عادي للاختبار", "أ"),
            Make(2, "فرع2", "نص رسالة عادي للاختبار", "ب"),
            Make(3, "فرع3", "نص رسالة عادي للاختبار", "ج"),
            Make(null, "بلا فرع", "نص رسالة عادي للاختبار", "د"));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.Read, [1, 2]);
        var h = new ListComplaintsHandler(db, current);

        var all = await h.Handle(new ListComplaintsQuery(), default);
        Assert.Equal(3, all.Data!.TotalCount);
        Assert.DoesNotContain(all.Data.Items, x => x.BranchId == 3);

        var filtered = await h.Handle(new ListComplaintsQuery { BranchId = 2 }, default);
        Assert.Single(filtered.Data!.Items);
        Assert.Equal(2, filtered.Data.Items[0].BranchId);
        Assert.Equal(1, filtered.Data.UnreadCount);

        var outside = await h.Handle(new ListComplaintsQuery { BranchId = 3 }, default);
        Assert.True(outside.IsSucceeded);
        Assert.Empty(outside.Data!.Items);
        Assert.Equal(0, outside.Data.UnreadCount);

        var noScope = await new ListComplaintsHandler(db, User(SeedPermissions.Complaint.Read, []))
            .Handle(new ListComplaintsQuery(), default);
        Assert.Empty(noScope.Data!.Items);
    }

    [Fact]
    public async Task Mark_all_read_only_touches_unread_in_scope()
    {
        await using var db = CreateDb();
        var alreadyRead = Make(1, "مقروءة", "نص رسالة عادي للاختبار", "أ");
        alreadyRead.MarkRead(77, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), 0);
        db.ComplaintMessages.AddRange(
            alreadyRead,
            Make(1, "جديدة1", "نص رسالة عادي للاختبار", "ب"),
            Make(2, "جديدة2", "نص رسالة عادي للاختبار", "ج"),
            Make(3, "خارج النطاق", "نص رسالة عادي للاختبار", "د"));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.MarkRead, [1, 2], userId: 9);
        var result = await new MarkAllComplaintsReadHandler(db, current, new DateTimeProviderStub())
            .Handle(new MarkAllComplaintsReadCommand(), default);

        Assert.True(result.IsSucceeded);
        Assert.Equal(2, result.Data!.Updated);
        var rows = await db.ComplaintMessages.AsNoTracking().ToListAsync();
        Assert.Equal(ComplaintStatus.Unread, rows.Single(x => x.BranchId == 3).Status);
        Assert.Equal(77, rows.Single(x => x.Subject == "مقروءة").ReadByUserId);
        Assert.All(rows.Where(x => x.BranchId is 1 or 2), x => Assert.Equal(ComplaintStatus.Read, x.Status));
    }

    [Fact]
    public async Task Mark_all_read_with_branch_filter_and_outside_scope()
    {
        await using var db = CreateDb();
        db.ComplaintMessages.AddRange(
            Make(1, "أ", "نص رسالة عادي للاختبار", "أ"),
            Make(2, "ب", "نص رسالة عادي للاختبار", "ب"),
            Make(3, "ج", "نص رسالة عادي للاختبار", "ج"));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.MarkRead, [1, 2], userId: 9);
        var h = new MarkAllComplaintsReadHandler(db, current, new DateTimeProviderStub());

        var outside = await h.Handle(new MarkAllComplaintsReadCommand { BranchId = 3 }, default);
        Assert.Equal(0, outside.Data!.Updated);

        var one = await h.Handle(new MarkAllComplaintsReadCommand { BranchId = 2 }, default);
        Assert.Equal(1, one.Data!.Updated);

        var rows = await db.ComplaintMessages.AsNoTracking().ToListAsync();
        Assert.Equal(ComplaintStatus.Read, rows.Single(x => x.BranchId == 2).Status);
        Assert.Equal(ComplaintStatus.Unread, rows.Single(x => x.BranchId == 1).Status);
        Assert.Equal(ComplaintStatus.Unread, rows.Single(x => x.BranchId == 3).Status);
    }

    [Fact]
    public async Task Mark_all_read_requires_permission()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.Complaint.Read, [1]);
        var result = await new MarkAllComplaintsReadHandler(db, current, new DateTimeProviderStub())
            .Handle(new MarkAllComplaintsReadCommand(), default);
        Assert.Equal("Complaint.Forbidden", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Mark_read_denied_when_branch_scope_empty()
    {
        await using var db = CreateDb();
        var row = Make(1, "أ", "نص رسالة عادي للاختبار", "أ");
        db.ComplaintMessages.Add(row);
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.MarkRead, [], userId: 9);
        var result = await new MarkComplaintReadHandler(db, current, new DateTimeProviderStub())
            .Handle(new MarkComplaintReadCommand { Id = row.Id, ExpectedVersion = 0 }, default);
        Assert.Equal("Complaint.BranchScope", result.Error!.Value.Code);
    }

    private static TestApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ComplaintMessage Make(
        int? branchId,
        string subject,
        string body,
        string sender,
        DateTimeOffset? at = null) =>
        new("committee", 5, sender, "Motaba", branchId, branchId is null ? null : $"فرع {branchId}",
            subject, body, at ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static CurrentUserServiceStub User(string permissions, List<int> branchIds, int userId = 1)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(CustomClaims.Permissions, permissions)
        ], "test");
        return new CurrentUserServiceStub
        {
            UserId = userId,
            User = new ClaimsPrincipal(identity),
            BranchIds = branchIds
        };
    }
}
