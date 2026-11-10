using System.Security.Claims;
using Application.Common.Models.TokenFactory;
using Application.WrongSales.Commands;
using Application.WrongSales.Queries;
using Domain.Entities.WrongSaleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.WrongSales;

public class WrongSaleSecurityHandlerTests
{
    [Fact]
    public async Task List_with_empty_branch_ids_returns_empty_not_all_rows()
    {
        await using var db = CreateDb();
        db.WrongSaleNotices.Add(MakeNotice(1, branchId: 10));
        db.WrongSaleNotices.Add(MakeNotice(2, branchId: 20));
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.WrongSale.Read, branchIds: []);
        var result = await new ListWrongSalesHandler(db, current).Handle(new ListWrongSalesQuery(), default);
        Assert.True(result.IsSucceeded);
        Assert.Empty(result.Data!.Items);
        Assert.Equal(0, result.Data.TotalCount);
    }

    [Fact]
    public async Task List_rejects_branchId_outside_scope_even_with_manage_permissions()
    {
        await using var db = CreateDb();
        var current = User(
            $"{SeedPermissions.WrongSale.Read},{SeedPermissions.User.ManagePermissions}",
            branchIds: [10]);
        var result = await new ListWrongSalesHandler(db, current)
            .Handle(new ListWrongSalesQuery { BranchId = 99 }, default);
        Assert.True(result.IsFailed);
        Assert.Equal("WrongSale.BranchScope", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Mark_branch_reviewed_requires_note_and_conflicts_on_repeat()
    {
        await using var db = CreateDb();
        var notice = MakeNotice(5, branchId: 3);
        db.WrongSaleNotices.Add(notice);
        await db.SaveChangesAsync();
        var id = notice.Id;

        var current = User(SeedPermissions.WrongSale.MarkBranchReviewed, branchIds: [3], userId: 8);
        var clock = new DateTimeProviderStub();
        var h = new MarkWrongSaleBranchReviewedHandler(db, current, clock);

        var bad = await h.Handle(new MarkWrongSaleBranchReviewedCommand
        {
            Id = id, Note = "ab", ExpectedVersion = 0
        }, default);
        Assert.Equal("WrongSale.NoteRequired", bad.Error!.Value.Code);

        var ok = await h.Handle(new MarkWrongSaleBranchReviewedCommand
        {
            Id = id, Note = "مراجعة صحيحة", ExpectedVersion = 0
        }, default);
        Assert.True(ok.IsSucceeded);

        var again = await h.Handle(new MarkWrongSaleBranchReviewedCommand
        {
            Id = id, Note = "مراجعة ثانية", ExpectedVersion = ok.Data!.ExpectedVersion
        }, default);
        Assert.Equal("WrongSale.AlreadyReviewed", again.Error!.Value.Code);
    }

    [Fact]
    public async Task Mark_delegate_read_denied_when_branch_scope_empty()
    {
        await using var db = CreateDb();
        var notice = MakeNotice(7, branchId: 1);
        db.WrongSaleNotices.Add(notice);
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.WrongSale.MarkDelegateRead, branchIds: [], userId: 2);
        var result = await new MarkWrongSaleDelegateReadHandler(db, current, new DateTimeProviderStub())
            .Handle(new MarkWrongSaleDelegateReadCommand { Id = notice.Id, ExpectedVersion = 0 }, default);
        Assert.Equal("WrongSale.BranchScope", result.Error!.Value.Code);
    }

    private static TestApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static WrongSaleNotice MakeNotice(int orderId, int branchId) =>
        new(orderId, 100 + orderId, branchId, 9, "متابع", "زبون", "07", 4, "ملاحظة",
            1000, 200, "مبيعات خاطئة", DateTimeOffset.UtcNow);

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
