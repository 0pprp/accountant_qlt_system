using Application.SalesExcelSearch.Common;
using Application.SalesExcelSearch.Queries;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesExcelSearch;

public class SalesExcelSearchTests
{
    [Fact]
    public void Name_match_family_and_logical_rules()
    {
        Assert.True(SalesCustomerNameMatch.IsMatch("حسين محمد علي", SalesArabicText.Normalize("حسين محمد")));
        Assert.False(SalesCustomerNameMatch.IsMatch("علي حسين محمد", SalesArabicText.Normalize("حسين محمد")));
        Assert.True(SalesCustomerNameMatch.UsesFamilySearch(SalesArabicText.Normalize("أحمد محمد علي")));
        Assert.True(SalesCustomerNameMatch.IsFamilyMatch(
            "علي محمد علي حسن",
            SalesCustomerNameMatch.FamilySearchKey(SalesArabicText.Normalize("أحمد محمد علي"))));
    }

    [Fact]
    public async Task Search_requires_customer_read_and_scope()
    {
        await using var db = CreateDb();
        var denied = await new SearchSalesCustomersByNamesHandler(
                db, User("", [1]))
            .Handle(new SearchSalesCustomersByNamesQuery { Names = ["أحمد"] }, default);
        Assert.Equal("SalesExcelSearch.Forbidden", denied.Error!.Value.Code);

        var noScope = await new SearchSalesCustomersByNamesHandler(
                db, User(SeedPermissions.Customer.Read, []))
            .Handle(new SearchSalesCustomersByNamesQuery { Names = ["أحمد"] }, default);
        Assert.Equal("SalesExcelSearch.BranchScope", noScope.Error!.Value.Code);
    }

    [Fact]
    public async Task Search_matches_within_branch_scope_only()
    {
        await using var db = CreateDb();
        var b1 = new Branch("فرع أ", 1);
        var b2 = new Branch("فرع ب", 1);
        db.Branches.AddRange(b1, b2);
        await db.SaveChangesAsync();

        db.Customers.Add(new Customer(
            "حسين محمد علي", "أم", "N1", new DateOnly(1990, 1, 1),
            "0700", "0700",             new Business("محل", "عنوان", "علامة"), b1.Id));
        db.Customers.Add(new Customer(
            "حسين محمد علي", "أم", "N2", new DateOnly(1990, 1, 1),
            "0701", "0701", new Business("محل", "عنوان", "علامة"), b2.Id));
        await db.SaveChangesAsync();

        var result = await new SearchSalesCustomersByNamesHandler(
                db, User(SeedPermissions.Customer.Read, [b1.Id]))
            .Handle(new SearchSalesCustomersByNamesQuery { Names = ["حسين محمد"] }, default);

        Assert.True(result.IsSucceeded);
        Assert.Equal(1, result.Data!.FoundCount);
        Assert.Single(result.Data.Queries[0].Matches);
        Assert.Equal(b1.Id, result.Data.Queries[0].Matches[0].BranchId);
    }

    private static TestApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CurrentUserServiceStub User(string permissions, List<int> branchIds, int userId = 1)
    {
        var identity = new System.Security.Claims.ClaimsIdentity(
        [
            new System.Security.Claims.Claim(
                Application.Common.Models.TokenFactory.CustomClaims.Permissions, permissions)
        ], "test");
        return new CurrentUserServiceStub
        {
            UserId = userId,
            User = new System.Security.Claims.ClaimsPrincipal(identity),
            BranchIds = branchIds
        };
    }
}
