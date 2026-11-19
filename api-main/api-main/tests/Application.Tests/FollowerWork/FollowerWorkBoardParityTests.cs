using System.Security.Claims;
using Application.Common.Interfaces;
using Application.Common.Models.SignIn;
using Application.Common.Models.TokenFactory;
using Application.Common.Utilities;
using Application.FollowerWork.Common;
using Application.FollowerWork.Queries.Admin.GetCustomerProfile;
using Application.FollowerWork.Queries.Admin.GetShopCapture;
using Application.SalesRatings.Common;
using Application.Users.Commands.Common.Login;
using Application.Users.Common;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.FollowerWork;

public class FollowerWorkBoardParityTests
{
    private const int BranchId = 4;

    [Fact]
    public async Task FactsAsOfCutoff_IgnoreLaterPaymentsAndLaterSales()
    {
        await using var db = CreateDb();
        var customer = NewCustomer("زبون أول");
        var later = NewCustomer("زبون لاحق");
        db.Customers.AddRange(customer, later);
        await db.SaveChangesAsync();

        var early = NewOrder(customer.Id, 1, new DateOnly(2026, 11, 1));
        early.InstallmentPayments.Add(Payment(new DateOnly(2026, 11, 2), 30_000));
        early.InstallmentPayments.Add(Payment(new DateOnly(2026, 11, 10), 50_000));
        var afterCutoff = NewOrder(later.Id, 1, new DateOnly(2026, 11, 8));
        db.Orders.AddRange(early, afterCutoff);
        await db.SaveChangesAsync();

        var asOfCutoff = await FollowerFactsQuery.LoadAsync(db, [1], new DateOnly(2026, 11, 5), null, CancellationToken.None);
        var current = await FollowerFactsQuery.LoadAsync(db, [1], null, null, CancellationToken.None);

        var past = Assert.Single(asOfCutoff);
        Assert.Equal(30_000, past.ReceiptsTotal);
        Assert.Equal(new DateOnly(2026, 11, 2), past.LastPaymentDate);
        Assert.Equal(2, current.Count);
        Assert.Equal(80_000, current.Single(f => f.CustomerId == customer.Id).ReceiptsTotal);
    }

    [Fact]
    public async Task Facts_IncludeUnallocatedCustomerPaymentsLikeSource()
    {
        await using var db = CreateDb();
        var settled = NewCustomer("زبون سدد بدفعة غير مخصصة");
        db.Customers.Add(settled);
        await db.SaveChangesAsync();
        var order = NewOrder(settled.Id, 1, new DateOnly(2026, 6, 1));
        order.InstallmentPayments.Add(Payment(new DateOnly(2026, 6, 10), 400_000));
        db.Orders.Add(order);
        var lumpSum = Payment(new DateOnly(2026, 11, 3), 600_000);
        lumpSum.CustomerId = settled.Id;
        db.InstallmentPayments.Add(lumpSum);
        await db.SaveChangesAsync();

        var current = Assert.Single(await FollowerFactsQuery.LoadAsync(db, [1], null, null, CancellationToken.None));
        var beforeLumpSum = Assert.Single(await FollowerFactsQuery.LoadAsync(db, [1], new DateOnly(2026, 11, 1), null, CancellationToken.None));

        Assert.Equal(1_000_000, current.ReceiptsTotal);
        Assert.False(ShopCaptureProgressQuery.HasRoundedOpenBalance(current.AmountRemaining));
        Assert.Equal(new DateOnly(2026, 11, 3), current.LastPaymentDate);
        Assert.Equal(400_000, beforeLumpSum.ReceiptsTotal);
    }

    [Fact]
    public void FollowerAverage_SkipsListsWithoutRatedCustomers()
    {
        var averages = FollowerFactsQuery.NonEmptyListAverages(
        [
            new RatingAggregateResult(10, 0, 0, 0, 0, 0, 0, 6, ""),
            new RatingAggregateResult(0, 0, 0, 0, 0, 0, 0, 0, ""),
            new RatingAggregateResult(4, 0, 0, 0, 0, 0, 0, 2, "")
        ]);

        Assert.Equal([6d, 2d], averages);
        Assert.Equal(4, averages.Average());
    }

    [Fact]
    public async Task CustomerProfile_RejectsCustomerOutsideFollowerLists()
    {
        await using var db = CreateDb();
        var follower = NewUser("متابع", "follower1");
        var otherFollower = NewUser("متابع آخر", "follower2");
        db.Users.AddRange(follower, otherFollower);
        await db.SaveChangesAsync();
        var mine = new OrderList("قائمتي", 100, follower.Id, BranchId);
        var other = new OrderList("قائمة غيري", 101, otherFollower.Id, BranchId);
        db.OrderLists.AddRange(mine, other);
        var customer = NewCustomer("زبون غيري");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        db.Orders.Add(NewOrder(customer.Id, other.Id, new DateOnly(2026, 10, 1)));
        await db.SaveChangesAsync();

        var user = UserWith([BranchId], SeedPermissions.FollowerWork.SalesReviewRead);
        var handler = new GetFollowerCustomerProfileHandler(db, user, new DateTimeProviderStub(), new FollowerSalesReviewDataService(db, new DateTimeProviderStub()));

        var result = await handler.Handle(new GetFollowerCustomerProfileQuery { FollowerId = follower.Id, CustomerId = customer.Id, BranchId = BranchId }, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(FollowerWorkErrors.CustomerOutOfScope.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task CustomerProfile_RejectsBranchOutsideUserScope()
    {
        await using var db = CreateDb();
        var user = UserWith([7], SeedPermissions.FollowerWork.SalesReviewRead);
        var handler = new GetFollowerCustomerProfileHandler(db, user, new DateTimeProviderStub(), new FollowerSalesReviewDataService(db, new DateTimeProviderStub()));

        var result = await handler.Handle(new GetFollowerCustomerProfileQuery { FollowerId = 50, CustomerId = 1, BranchId = BranchId }, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(FollowerWorkErrors.BranchAccessDenied.Code, result.Error!.Value.Code);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(250, false)]
    [InlineData(499, false)]
    [InlineData(500, true)]
    [InlineData(1000, true)]
    [InlineData(-800, false)]
    public void ShopCapture_UsesRemainingRoundedToThousandLikeSource(double remaining, bool counted)
        => Assert.Equal(counted, ShopCaptureProgressQuery.HasRoundedOpenBalance(remaining));

    [Theory]
    [InlineData("قائمة أرشيف", true)]
    [InlineData(" قائمة أرشيف ", true)]
    [InlineData("الشمالية", false)]
    [InlineData(null, false)]
    public void ShopCapture_ExcludesArchiveListLikeSource(string? listName, bool excluded)
        => Assert.Equal(excluded, ShopCaptureProgressQuery.IsArchiveList(listName));

    [Fact]
    public void ShopPhoto_LatestWinsAcrossImportedAndAppSources()
    {
        var older = new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);
        var newer = new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

        var latest = FollowerShopPhotoSources.LatestPerCustomer(
        [
            new FollowerShopPhotoRef(1, 10, older, FollowerShopPhotoSources.Imported),
            new FollowerShopPhotoRef(1, 77, newer, FollowerShopPhotoSources.App),
            new FollowerShopPhotoRef(2, 11, newer, FollowerShopPhotoSources.Imported),
            new FollowerShopPhotoRef(3, 12, newer, FollowerShopPhotoSources.Imported),
            new FollowerShopPhotoRef(3, 78, newer, FollowerShopPhotoSources.App)
        ]);

        Assert.Equal((77, FollowerShopPhotoSources.App), (latest[1].PhotoId, latest[1].Source));
        Assert.Equal((11, FollowerShopPhotoSources.Imported), (latest[2].PhotoId, latest[2].Source));
        Assert.Equal(FollowerShopPhotoSources.App, latest[3].Source);
        Assert.False(latest.ContainsKey(4));
    }

    [Fact]
    public async Task DelegateAppLogin_IsLockedWhileFollowerCollectsTheirList()
    {
        await using var db = CreateDb();
        var role = new Role("Mandob", "المندوب");
        db.Roles.Add(role);
        var delegateUser = new User("مندوب", null, null, null, "mandob1", PasswordHash.HashPassword("Secret#123"), null, null)
        {
            UserRoles = [new UserRole(0) { Role = role }]
        };
        db.Users.Add(delegateUser);
        await db.SaveChangesAsync();
        var list = new OrderList("قائمة المندوب", delegateUser.Id, 9, BranchId);
        list.SetFollowerCollectionEnabled(true, DateTimeOffset.UtcNow, null);
        db.OrderLists.Add(list);
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, new AuthServiceStub(), new ActivityLogServiceStub());

        var app = await handler.Handle(new LoginCommand { Username = "mandob1", Password = "Secret#123", EnforceSalesAppBlock = true }, CancellationToken.None);
        Assert.True(app.IsFailed);
        Assert.Equal(UserErrors.DelegateCollectionLocked.Code, app.Error!.Value.Code);

        var panel = await handler.Handle(new LoginCommand { Username = "mandob1", Password = "Secret#123", EnforceSalesAppBlock = false }, CancellationToken.None);
        Assert.True(panel.IsSucceeded);

        list.SetFollowerCollectionEnabled(false, DateTimeOffset.UtcNow, null);
        await db.SaveChangesAsync();
        var afterStop = await handler.Handle(new LoginCommand { Username = "mandob1", Password = "Secret#123", EnforceSalesAppBlock = true }, CancellationToken.None);
        Assert.True(afterStop.IsSucceeded);
    }

    private static CurrentUserServiceStub UserWith(List<int> branches, params string[] permissions)
        => new()
        {
            BranchIds = branches,
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(CustomClaims.Permissions, string.Join(',', permissions))], "test"))
        };

    private static User NewUser(string name, string username)
        => new(name, null, null, null, username, PasswordHash.HashPassword("Secret#123"), null, null);

    private static Customer NewCustomer(string name)
        => new(name, "ام", Guid.NewGuid().ToString("N")[..8], new DateOnly(1990, 1, 1), "07700000000", "07700000000",
            new Business("محل", "عنوان", "علامة"), branchId: BranchId);

    private static Order NewOrder(int customerId, int listId, DateOnly saleDate)
    {
        var order = Order.CreateByAdmin(600_000, 1_000_000, 0, 10_000, customerId, branchId: BranchId, orderItems: []);
        order.OrderListId = listId;
        order.SaleDate = saleDate;
        return order;
    }

    private static Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment Payment(DateOnly date, double amount)
        => new(amount, date);

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class AuthServiceStub : IAuthService
    {
        public Task<SignInResponse> SignInAsync(SignInRequest user, CancellationToken cancellationToken = default)
            => Task.FromResult(new SignInResponse { AccessToken = "a", RefreshToken = "r", Permissions = [], Branches = [], Roles = [] });

        public Task<Result<SignInResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
