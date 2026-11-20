using System.Security.Claims;
using Application.Common.Interfaces;
using Application.FieldPromoters.Admin;
using Application.FieldPromoters.App;
using Application.FieldPromoters.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.FieldPromoterAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SalesLocationAggregate;
using Domain.Entities.SalesLocationAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.SalesShiftAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Tests.FieldPromoters;

public class FieldPromoterRuleTests
{
    [Theory]
    [InlineData("محمد جاسم علي", true)]
    [InlineData("عبد الله حسن كاظم", true)]
    [InlineData("عبد الله حسن", false)]
    [InlineData("Mohammed Jasim Ali", false)]
    [InlineData("محمد جاسم", false)]
    [InlineData("مُحمّد جاسم علي", true)]
    public void Triple_name_requires_three_arabic_names(string name, bool valid)
        => Assert.Equal(valid, FieldPromoterRules.NormalizeTripleName(name) is not null);

    [Theory]
    [InlineData("07701234567", "07701234567")]
    [InlineData("+964 770 123 4567", "07701234567")]
    [InlineData("٠٧٧٠١٢٣٤٥٦٧", "07701234567")]
    [InlineData("7701234567", "07701234567")]
    [InlineData("07201234567", null)]
    [InlineData("0770123", null)]
    public void Phone_is_normalized_to_iraqi_mobile(string input, string? expected)
        => Assert.Equal(expected, FieldPromoterRules.NormalizePhone(input));

    [Fact]
    public void Periods_break_on_fifteen_minute_gaps()
    {
        var t = new DateTimeOffset(2026, 11, 21, 6, 0, 0, TimeSpan.Zero);
        var periods = WorkPeriods.From([t, t.AddMinutes(10), t.AddMinutes(20), t.AddMinutes(50), t.AddMinutes(62)]);
        Assert.Equal(2, periods.Count);
        Assert.Equal(20, periods[0].Minutes);
        Assert.Equal(12, periods[1].Minutes);
        Assert.Equal(32, WorkPeriods.TotalMinutes(periods));
    }

    [Fact]
    public void Stationary_needs_an_hour_inside_the_radius_and_a_fresh_point()
    {
        var now = new DateTimeOffset(2026, 11, 21, 9, 0, 0, TimeSpan.Zero);
        var still = Enumerable.Range(0, 14).Select(i => new LocationSample(32.0 + i * 0.00001, 44.4, now.AddMinutes(-i * 5))).ToList();
        Assert.NotNull(StationaryDetector.Detect(still, 150, 60, now));

        var moved = still.Select((s, i) => i == 6 ? s with { Latitude = 32.01 } : s).ToList();
        Assert.Null(StationaryDetector.Detect(moved, 150, 60, now));

        Assert.Null(StationaryDetector.Detect(still, 150, 60, now.AddMinutes(30)));
    }

    [Fact]
    public void Jpeg_metadata_is_removed_and_unknown_files_are_refused()
    {
        byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x06, 0x45, 0x78, 0x69, 0x66, 0xFF, 0xDB, 0x00, 0x03, 0x01, 0xFF, 0xDA, 0x00, 0x02, 0x11, 0xFF, 0xD9];
        var clean = ShopPhotoSanitizer.Sanitize(jpeg);
        Assert.NotNull(clean);
        Assert.Equal("image/jpeg", clean!.Value.ContentType);
        Assert.DoesNotContain((byte)0xE1, clean.Value.Bytes.Skip(2).Take(2));
        Assert.Equal(jpeg.Length - 8, clean.Value.Bytes.Length);
        Assert.Null(ShopPhotoSanitizer.Sanitize([0x47, 0x49, 0x46, 0x38]));
    }
}

public class FieldPromoterFlowTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Submit_creates_a_promoter_sale_request_with_shop_and_starts_the_day()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        var result = await Submit(db, seed.PromoterA, seed.BranchId, "07701234567", seed.Key);

        Assert.True(result.IsSucceeded, result.Error?.Message);
        var request = await db.SalesRequests.SingleAsync();
        Assert.Equal(SalesRequestSourceType.FieldPromoter, request.SourceType);
        Assert.Equal(SalesRequestStatus.New, request.Status);
        Assert.Equal("ايفون", request.RequestedProduct);
        Assert.NotNull(await db.FieldPromoterOrderShops.SingleAsync(s => s.SalesRequestId == request.Id));
        Assert.True(await db.SalesShifts.AnyAsync(s => s.UserId == seed.PromoterA && s.EndedAtUtc == null));

        var again = await Submit(db, seed.PromoterA, seed.BranchId, "07701234567", seed.Key);
        Assert.True(again.IsSucceeded);
        Assert.True(again.Data!.AlreadySubmitted);
    }

    [Fact]
    public async Task Same_customer_and_product_open_from_another_promoter_is_refused_without_naming_him()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        Assert.True((await Submit(db, seed.PromoterA, seed.BranchId, "07701234567")).IsSucceeded);

        var dup = await Submit(db, seed.PromoterB, seed.BranchId, "+9647701234567");
        Assert.True(dup.IsFailed);
        Assert.Equal(FieldPromoterErrors.DuplicateOpenOrder.Code, dup.Error!.Value.Code);
        Assert.DoesNotContain("مروّج", dup.Error.Value.Message);
    }

    [Fact]
    public async Task Suspended_promoter_and_foreign_branch_are_refused()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        Assert.Equal(FieldPromoterErrors.BranchForbidden.Code,
            (await Submit(db, seed.PromoterA, seed.BranchId + 99, "07701234567")).Error!.Value.Code);

        var profile = await db.FieldPromoterProfiles.SingleAsync(p => p.UserId == seed.PromoterA);
        profile.Suspend("غياب");
        await db.SaveChangesAsync();
        Assert.Equal(FieldPromoterErrors.Suspended.Code,
            (await Submit(db, seed.PromoterA, seed.BranchId, "07701234567")).Error!.Value.Code);
    }

    [Fact]
    public async Task Colleagues_see_pin_location_and_interest_but_not_the_phone()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        var created = await new PromoterPinHandlers(db, Promoter(seed.PromoterA, seed.BranchId), new Clock())
            .Handle(new CreatePinCommand(new PinInput(32.03, 44.38, "مقهى الساعدي", 5, "07801112233", "يفتح ليلاً", null, null)), default);
        Assert.True(created.IsSucceeded, created.Error?.Message);

        var colleague = new PromoterPinHandlers(db, Promoter(seed.PromoterB, seed.BranchId), new Clock());
        var map = await colleague.Handle(new GetPinsInBoxQuery(32.0, 44.3, 32.1, 44.5), default);
        var pin = Assert.Single(map.Data!);
        Assert.False(pin.Mine);
        var detail = await colleague.Handle(new GetPinQuery(pin.Id), default);
        Assert.Null(detail.Data!.Phone);
        Assert.Null(detail.Data.Note);
        var edit = await colleague.Handle(new UpdatePinCommand(pin.Id, new PinInput(0, 0, "تغيير", 1, null, null, null, null)), default);
        Assert.Equal(FieldPromoterErrors.NotOwner.Code, edit.Error!.Value.Code);
    }

    [Fact]
    public async Task Sale_progress_is_read_per_order_not_per_customer()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        Assert.True((await Submit(db, seed.PromoterA, seed.BranchId, "07701234567")).IsSucceeded);
        var request = await db.SalesRequests.SingleAsync();
        var order = new Order { SellAmount = 3_000_000, BranchId = seed.BranchId };
        var other = new Order { SellAmount = 1_000_000, BranchId = seed.BranchId };
        db.Orders.AddRange(order, other);
        await db.SaveChangesAsync();
        db.InstallmentPayments.AddRange(
            new InstallmentPayment(170_000, new DateOnly(2026, 11, 20)) { OrderId = order.Id },
            new InstallmentPayment(1_000_000, new DateOnly(2026, 11, 20)) { OrderId = other.Id });
        request.LinkedOrderId = order.Id;
        request.Status = SalesRequestStatus.Completed;
        await db.SaveChangesAsync();

        var row = (await new PromoterOrderQueries(db, Promoter(seed.PromoterA, seed.BranchId), new Clock())
            .Handle(new GetPromoterOrderQuery(request.Id), default)).Data!;
        Assert.Equal("sold", row.Stage);
        Assert.Equal(170_000, row.Sale!.Paid);
        Assert.Equal(2_830_000, row.Sale.Remaining);
        Assert.Equal(6, row.Sale.Percent);
    }

    [Fact]
    public async Task Cancel_only_while_new_and_unassigned()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        Assert.True((await Submit(db, seed.PromoterA, seed.BranchId, "07701234567")).IsSucceeded);
        var request = await db.SalesRequests.SingleAsync();
        var handler = new PromoterOrderQueries(db, Promoter(seed.PromoterA, seed.BranchId), new Clock());

        request.TargetEmployeeId = seed.PromoterB;
        await db.SaveChangesAsync();
        Assert.Equal(FieldPromoterErrors.CannotCancel.Code, (await handler.Handle(new CancelPromoterOrderCommand(request.Id, null), default)).Error!.Value.Code);

        request.TargetEmployeeId = null;
        await db.SaveChangesAsync();
        Assert.True((await handler.Handle(new CancelPromoterOrderCommand(request.Id, "الزبون تراجع"), default)).IsSucceeded);
        Assert.True((await db.SalesRequests.SingleAsync()).IsDeleted);
    }

    [Fact]
    public async Task Points_need_an_event_id_inside_iraq_and_are_deduplicated()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        var handler = new IngestPromoterLocationsHandler(db, Promoter(seed.PromoterA, seed.BranchId), new Clock());
        var noShift = await handler.Handle(new IngestPromoterLocationsCommand([new PromoterPointInput(32, 44.4, 5, Now, "a")]), default);
        Assert.False(noShift.Data!.IsWorking);

        db.SalesShifts.Add(new SalesShift(seed.PromoterA, seed.BranchId, Now.AddHours(-1)));
        await db.SaveChangesAsync();
        var result = await handler.Handle(new IngestPromoterLocationsCommand([
            new PromoterPointInput(32, 44.4, 5, Now, "p1"),
            new PromoterPointInput(32, 44.4, 5, Now, "p1"),
            new PromoterPointInput(51.5, -0.1, 5, Now, "p2"),
            new PromoterPointInput(32, 44.4, 5, Now, "")
        ]), default);
        Assert.Equal(1, result.Data!.Accepted);
        Assert.Equal(1, result.Data.Duplicate);
        Assert.Equal(2, result.Data.Rejected);
        Assert.Equal(SalesLocationActorKind.FieldPromoter, (await db.SalesLocationPoints.SingleAsync()).ActorKind);
    }

    [Fact]
    public async Task Maintenance_raises_a_stationary_alert_and_closes_yesterdays_shift()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        var stale = new SalesShift(seed.PromoterB, seed.BranchId, Now.AddDays(-1));
        var live = new SalesShift(seed.PromoterA, seed.BranchId, Now.AddHours(-2));
        db.SalesShifts.AddRange(stale, live);
        await db.SaveChangesAsync();
        for (var i = 0; i < 16; i++)
            db.SalesLocationPoints.Add(new SalesLocationPoint(seed.PromoterA, seed.BranchId, 32.0, 44.4, Now.AddMinutes(-i * 5),
                Now, false, SalesLocationActorKind.FieldPromoter, 5, live.Id, "e" + i));
        await db.SaveChangesAsync();

        var report = await new FieldPromoterMaintenance(db).RunAsync(Now, default);

        Assert.Equal(1, report.ClosedShifts);
        Assert.Equal(1, report.RaisedAlerts);
        Assert.NotNull((await db.SalesShifts.SingleAsync(s => s.Id == stale.Id)).EndedAtUtc);
        var alert = await db.FieldPromoterAlerts.SingleAsync();
        Assert.Equal(seed.PromoterA, alert.UserId);
    }

    [Fact]
    public async Task Suspending_from_the_panel_kicks_the_promoter_out_immediately()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        var before = (await db.Users.SingleAsync(u => u.Id == seed.PromoterA)).SecurityStamp;
        var admin = Admin(seed.BranchId);
        var result = await new PromoterAccountHandlers(db, admin, new Clock(), new ActivityLogServiceStub())
            .Handle(new SuspendPromoterCommand(seed.PromoterA, true, "غياب متكرر"), default);
        Assert.True(result.IsSucceeded, result.Error?.Message);
        Assert.NotEqual(before, (await db.Users.SingleAsync(u => u.Id == seed.PromoterA)).SecurityStamp);
        Assert.True((await db.FieldPromoterProfiles.SingleAsync(p => p.UserId == seed.PromoterA)).IsSuspended);
    }

    [Fact]
    public async Task Panel_live_view_counts_today_and_requires_read_permission()
    {
        await using var db = Db();
        var seed = await SeedAsync(db);
        Assert.True((await Submit(db, seed.PromoterA, seed.BranchId, "07701234567")).IsSucceeded);
        var live = await new GetPromotersLiveHandler(db, Admin(seed.BranchId), new Clock()).Handle(new GetPromotersLiveQuery(null), default);
        Assert.True(live.IsSucceeded, live.Error?.Message);
        Assert.Equal(2, live.Data!.Total);
        Assert.Equal(1, live.Data.OrdersToday);
        Assert.Equal(1, live.Data.Working);

        var denied = await new GetPromotersLiveHandler(db, Promoter(seed.PromoterA, seed.BranchId), new Clock())
            .Handle(new GetPromotersLiveQuery(null), default);
        Assert.Equal(FieldPromoterErrors.Forbidden.Code, denied.Error!.Value.Code);
    }

    private static Task<Result<PromoterOrderReceipt>> Submit(TestApplicationDbContext db, int promoterId, int branchId,
        string phone, Guid? key = null)
        => new SubmitPromoterOrderHandler(db, Promoter(promoterId, branchId), new Clock(), new NoLock())
            .Handle(new SubmitPromoterOrderCommand
            {
                SubmissionKey = key ?? Guid.NewGuid(), BranchId = branchId, CustomerName = "نعيم عبد راضي كاظم",
                CustomerPhone = phone, Product = "ايفون", Address = "الكوفة — حي الجمعية",
                Latitude = 32.02, Longitude = 44.38
            }, default);

    private sealed record Seed(int BranchId, int PromoterA, int PromoterB, Guid Key);

    private static async Task<Seed> SeedAsync(TestApplicationDbContext db)
    {
        var province = new Province("النجف");
        db.Provinces.Add(province);
        await db.SaveChangesAsync();
        var branch = new Branch("الكوفة", province.Id);
        db.Branches.Add(branch);
        var role = new Role(SeedRoles.FieldPromoter.Name, SeedRoles.FieldPromoter.DisplayName);
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        int Make(string name, string username)
        {
            var u = new User(name, null, null, null, username, "hash", null, null);
            u.UserBranches.Add(new UserBranch(branch.Id));
            u.UserRoles.Add(new UserRole(role.Id) { Role = role });
            db.Users.Add(u);
            db.SaveChanges();
            db.FieldPromoterProfiles.Add(new FieldPromoterProfile(u.Id, Now));
            db.SaveChanges();
            return u.Id;
        }
        var a = Make("علي كاظم", "ali");
        var b = Make("حيدر علي", "haider");
        return new Seed(branch.Id, a, b, Guid.NewGuid());
    }

    private static CurrentUserServiceStub Promoter(int userId, int branchId) => new()
    {
        UserId = userId,
        BranchIds = [branchId],
        User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", SeedPermissions.FieldPromoter.App)], "test"))
    };

    private static CurrentUserServiceStub Admin(int branchId) => new()
    {
        UserId = 900,
        BranchIds = [branchId],
        User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permissions", SeedPermissions.FieldPromoter.Read + "," + SeedPermissions.FieldPromoter.Manage)], "test"))
    };

    private static TestApplicationDbContext Db() => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    private sealed class Clock : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => Now;
    }

    private sealed class NoLock : ISalesRequestSubmissionLock
    {
        public Task AcquireAsync(int userId, Guid submissionKey, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
