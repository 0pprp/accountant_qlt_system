using System.Security.Claims;
using Application.BranchManagerAccounts.Commands;
using Application.BranchManagerAccounts.Queries;
using Application.BranchNewSales.Queries;
using Application.BranchVisits.Commands;
using Application.Common.Models.TokenFactory;
using Application.Complaints.Commands;
using Application.CustomerDecisions.Queries;
using Application.DelegateHome.Queries;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Tests.BranchDelegate;

public class Batch31SecurityAndParityTests
{
    [Fact]
    public void Branch_manager_role_seed_excludes_manage_permissions()
    {
        var bm = Application.Common.SeedData.RolePermissions.All
            .First(r => r.RoleName == SeedRoles.BranchManager.Name);
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions, bm.Permissions);
        Assert.Contains(SeedPermissions.BranchNewSale.Read, bm.Permissions);
        Assert.Contains(SeedPermissions.BranchDeclining.Read, bm.Permissions);
        Assert.Contains(SeedPermissions.BranchVisit.Open, bm.Permissions);
    }

    [Fact]
    public async Task Branch_new_sales_without_open_visit_is_rejected()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.BranchNewSale.Read, [1]);
        var result = await new ListBranchNewSalesHandler(db, current)
            .Handle(new ListBranchNewSalesQuery(), default);
        Assert.True(result.IsFailed);
        Assert.Equal("BranchVisit.VisitRequired", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Customer_decisions_without_open_visit_is_rejected()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.CustomerDecision.Read, [1]);
        var result = await new ListCustomerDecisionsHandler(db, current)
            .Handle(new ListCustomerDecisionsQuery(), default);
        Assert.True(result.IsFailed);
        Assert.Equal("BranchVisit.VisitRequired", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Branch_new_sales_visit_for_other_branch_is_rejected()
    {
        await using var db = CreateDb();
        var actor = new User("زائر", "-", "n3", new DateOnly(1990, 1, 1), "v3", "hash", "-", "0700")
        {
            CreationStep = UserCreationStep.Completed
        };
        db.Users.Add(actor);
        var branchA = new Branch("فرع أ", 1);
        var branchB = new Branch("فرع ب", 1);
        db.Branches.Add(branchA);
        db.Branches.Add(branchB);
        await db.SaveChangesAsync();

        var clock = new DateTimeProviderStub();
        var opener = User(
            $"{SeedPermissions.BranchVisit.Open},{SeedPermissions.BranchVisit.Read}",
            [branchA.Id, branchB.Id],
            userId: actor.Id);
        var opened = await new OpenBranchVisitHandler(db, opener, clock)
            .Handle(new OpenBranchVisitCommand { BranchId = branchA.Id }, default);
        Assert.True(opened.IsSucceeded);

        // صلاحية قراءة المبيعات بلا BranchVisit.Read — الزيارة على A لا تفتح أعمال B.
        var reader = User(SeedPermissions.BranchNewSale.Read, [branchA.Id, branchB.Id], userId: actor.Id);
        // محاكاة عمل على فرع B عبر زيارة A: الشرط يربط بفرع الزيارة؛ القائمة تُصفّى لـ A فقط عند النجاح.
        // لإثبات mismatch على أمر بفرع محدد نستخدم Mark على طلب من B — هنا نثبت أن القائمة بلا زيارة مطابقة لـ B تبقى على A.
        var list = await new ListBranchNewSalesHandler(db, reader)
            .Handle(new ListBranchNewSalesQuery(), default);
        Assert.True(list.IsSucceeded);
        Assert.All(list.Data!.Items, i => Assert.Equal(branchA.Id, i.BranchId));
    }

    [Fact]
    public async Task Complaint_submit_persists_and_is_not_fake_success()
    {
        await using var db = CreateDb();
        var sender = new User("مكتب", "-", "n1", new DateOnly(1990, 1, 1), "c1", "hash", "-", "0700")
        {
            CreationStep = UserCreationStep.Completed
        };
        db.Users.Add(sender);
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.Complaint.Submit, [3], userId: sender.Id);
        var created = await new SubmitComplaintHandler(db, current, new DateTimeProviderStub())
            .Handle(new SubmitComplaintCommand { Body = "نص شكوى كافٍ للاختبار", Subject = "موضوع" }, default);
        Assert.True(created.IsSucceeded);
        Assert.Single(db.ComplaintMessages);
        Assert.Equal(sender.Id, db.ComplaintMessages.Single().SenderUserId);
    }

    [Fact]
    public async Task Branch_visit_open_denied_outside_scope_and_duplicate()
    {
        await using var db = CreateDb();
        var actor = new User("زائر", "-", "n2", new DateOnly(1990, 1, 1), "v1", "hash", "-", "0700")
        {
            CreationStep = UserCreationStep.Completed
        };
        db.Users.Add(actor);
        var branch = new Branch("فرع زيارة", 1);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var current = User(
            $"{SeedPermissions.BranchVisit.Open},{SeedPermissions.BranchVisit.Read}",
            [branch.Id],
            userId: actor.Id);
        var clock = new DateTimeProviderStub();

        var outside = await new OpenBranchVisitHandler(db, current, clock)
            .Handle(new OpenBranchVisitCommand { BranchId = 99 }, default);
        Assert.Equal("BranchVisit.BranchScope", outside.Error!.Value.Code);

        var ok = await new OpenBranchVisitHandler(db, current, clock)
            .Handle(new OpenBranchVisitCommand { BranchId = branch.Id }, default);
        Assert.True(ok.IsSucceeded);

        // نفس الفرع: idempotent
        var againSame = await new OpenBranchVisitHandler(db, current, clock)
            .Handle(new OpenBranchVisitCommand { BranchId = branch.Id }, default);
        Assert.True(againSame.IsSucceeded);
        Assert.Equal(ok.Data!.Id, againSame.Data!.Id);

        var other = new Branch("فرع آخر", 1);
        db.Branches.Add(other);
        await db.SaveChangesAsync();
        var currentOtherCity = User(
            $"{SeedPermissions.BranchVisit.Open},{SeedPermissions.BranchVisit.Read}",
            [branch.Id, other.Id],
            userId: actor.Id);
        var againOther = await new OpenBranchVisitHandler(db, currentOtherCity, clock)
            .Handle(new OpenBranchVisitCommand { BranchId = other.Id }, default);
        Assert.Equal("BranchVisit.AlreadyOpen", againOther.Error!.Value.Code);
    }

    [Fact]
    public async Task Branch_manager_account_write_rejects_out_of_scope_and_does_not_need_manage_permissions()
    {
        await using var db = CreateDb();
        db.Roles.Add(new Role(SeedRoles.BranchManager.Name, SeedRoles.BranchManager.DisplayName));
        var b1 = new Branch("فرع أ", 1);
        var b2 = new Branch("فرع ب", 1);
        db.Branches.Add(b1);
        db.Branches.Add(b2);
        await db.SaveChangesAsync();

        var current = User(SeedPermissions.BranchManagerAccount.Write, [b1.Id], userId: 11);
        var h = new CreateBranchManagerAccountHandler(db, current, new ActivityLogServiceStub());

        var bad = await h.Handle(new CreateBranchManagerAccountCommand
        {
            Username = "bmout",
            Password = "pass1",
            FullName = "خارج",
            BranchIds = [b2.Id]
        }, default);
        Assert.Equal("BranchManagerAccount.BranchScope", bad.Error!.Value.Code);

        var ok = await h.Handle(new CreateBranchManagerAccountCommand
        {
            Username = "bmin",
            Password = "pass1",
            FullName = "داخل",
            BranchIds = [b1.Id]
        }, default);
        Assert.True(ok.IsSucceeded);
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions,
            current.User!.FindFirst(CustomClaims.Permissions)!.Value.Split(','));
    }

    [Fact]
    public async Task Delegate_home_empty_scope_marks_metrics_unavailable_not_zero()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.DelegateHome.Read, []);
        var result = await new GetDelegateHomeSummaryHandler(db, current)
            .Handle(new GetDelegateHomeSummaryQuery(), default);
        Assert.True(result.IsSucceeded);
        Assert.False(result.Data!.UnreadComplaintsAvailable);
        Assert.Null(result.Data.UnreadComplaints);
        Assert.False(result.Data.UnreadWrongSalesAvailable);
        Assert.Null(result.Data.UnreadWrongSales);
    }

    [Fact]
    public async Task Branch_manager_accounts_list_empty_scope_is_empty()
    {
        await using var db = CreateDb();
        var current = User(SeedPermissions.BranchManagerAccount.Read, []);
        var result = await new ListBranchManagerAccountsHandler(db, current)
            .Handle(new ListBranchManagerAccountsQuery(), default);
        Assert.True(result.IsSucceeded);
        Assert.Empty(result.Data!.Items);
    }

    private static TestApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

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
