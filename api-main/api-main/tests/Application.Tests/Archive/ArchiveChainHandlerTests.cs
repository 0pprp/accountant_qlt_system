using System.Security.Claims;
using Application.Archive.Commands.ReceiveTransactions;
using Application.Archive.Commands.ReturnTransactions;
using Application.Archive.Commands.SendBundle;
using Application.Archive.Commands.UndoSend;
using Application.Archive.Queries.GetBundle;
using Application.Common.Models.TokenFactory;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.Archive;

/// <summary>
/// السلسلة كاملة على مُعالِجات حقيقية: الإرسال الجزئي، والاسترجاع، وإلغاء الإرسال.
/// هذا ما يحرس المعنى — لا فحص يدوي عابر.
/// </summary>
public class ArchiveChainHandlerTests
{
    private const int BranchId = 1;
    private const int SellerId = 70;
    private const int BranchManagerId = 80;
    private const int FollowerId = 90;
    private const int ArchiveOfficerId = 95;

    private static readonly DateTimeOffset Now = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock : Application.Common.Interfaces.IDateTimeProvider
    {
        public DateTime UtcNow => Now.UtcDateTime;
        public DateTimeOffset UtcNowOffset => Now;
        public DateOnly Today => DateOnly.FromDateTime(Now.UtcDateTime);
    }

    private static CurrentUserServiceStub UserWith(int id, params string[] permissions)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(CustomClaims.Permissions, string.Join(',', permissions)));

        return new CurrentUserServiceStub
        {
            UserId = id,
            BranchIds = [BranchId],
            User = new ClaimsPrincipal(identity)
        };
    }

    private static CurrentUserServiceStub SalesEmployee() => UserWith(
        SellerId, SeedPermissions.Archive.Read, SeedPermissions.Archive.Send,
        SeedPermissions.Archive.UndoSend);

    private static CurrentUserServiceStub BranchManager() => UserWith(
        BranchManagerId, SeedPermissions.Archive.Read, SeedPermissions.Archive.Send,
        SeedPermissions.Archive.UndoSend, SeedPermissions.Archive.ReceiveBranch);

    private static CurrentUserServiceStub Follower() => UserWith(
        FollowerId, SeedPermissions.Archive.Read, SeedPermissions.Archive.Send,
        SeedPermissions.Archive.ReceiveFollower);

    private static CurrentUserServiceStub ArchiveOfficer() => UserWith(
        ArchiveOfficerId, SeedPermissions.Archive.Read, SeedPermissions.Archive.ReceiveArchive,
        SeedPermissions.Archive.Return, SeedPermissions.Archive.Oversight);

    private static async Task<(TestApplicationDbContext Db, int BundleId, List<int> Ids)> SeedAsync(
        int itemCount, ArchivePeriod period)
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);

        db.Branches.Add(new Branch("فرع النجف", 1) { Id = BranchId });
        db.Users.Add(new User("ضياء فالح حسن", "أم", "1", new DateOnly(1990, 1, 1),
            "dhiaa", "x", "عنوان", "07") { Id = SellerId });
        await db.SaveChangesAsync();

        var bundle = new ArchiveBundle(BranchId, SellerId, "ضياء فالح حسن", period);
        db.ArchiveBundles.Add(bundle);
        await db.SaveChangesAsync();

        var ids = new List<int>();
        for (var i = 0; i < itemCount; i++)
        {
            var t = new ArchiveTransaction(
                bundle.Id, 1000 + i, 2000 + i, $"زبون {i + 1}", "0780",
                FollowerId, period.Start, ArchivePeriodSource.SaleDate);
            db.ArchiveTransactions.Add(t);
            await db.SaveChangesAsync();
            ids.Add(t.Id);
        }

        return (db, bundle.Id, ids);
    }

    private static SendArchiveBundleHandler Send(TestApplicationDbContext db, CurrentUserServiceStub user)
        => new(db, user, new FixedClock(), new ActivityLogServiceStub());

    private static ReceiveArchiveTransactionsHandler Receive(TestApplicationDbContext db, CurrentUserServiceStub user)
        => new(db, user, new FixedClock(), new ActivityLogServiceStub());

    private static ReturnArchiveTransactionsHandler Return(TestApplicationDbContext db, CurrentUserServiceStub user)
        => new(db, user, new FixedClock(), new ActivityLogServiceStub());

    private static UndoArchiveSendHandler Undo(TestApplicationDbContext db, CurrentUserServiceStub user)
        => new(db, user, new FixedClock(), new ActivityLogServiceStub());

    [Fact]
    public async Task Send_is_refused_while_the_period_is_still_open()
    {
        // العشرة الثانية من تشرين الثاني تنتهي يوم 20، واليوم 18.
        var (db, bundleId, _) = await SeedAsync(3, ArchivePeriod.For(2026, 11, 2));

        var result = await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal("Archive_Period_Not_Closed", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Send_is_allowed_once_the_period_has_closed()
    {
        var (db, bundleId, ids) = await SeedAsync(3, ArchivePeriod.For(2026, 11, 1));

        var result = await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        Assert.True(result.IsSucceeded);
        Assert.Equal(3, result.Data!.AffectedCount);

        var statuses = await db.ArchiveTransactions.Where(x => ids.Contains(x.Id))
            .Select(x => x.Status).ToListAsync();
        Assert.All(statuses, s => Assert.Equal(ArchiveTransactionStatus.SentToBranch, s));
    }

    [Fact]
    public async Task A_stage_forwards_only_what_it_actually_received()
    {
        var (db, bundleId, ids) = await SeedAsync(5, ArchivePeriod.For(2026, 11, 1));

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        // إدارة الفرع تستلم ثلاثاً من خمس.
        var received = await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand
            {
                TransactionIds = ids.Take(3).ToList(),
                AtStage = ArchiveStage.BranchManagement
            },
            CancellationToken.None);

        Assert.True(received.IsSucceeded);
        Assert.Equal(3, received.Data!.AffectedCount);

        // ثم ترسل، فتمضي الثلاث وتبقى اثنتان بانتظار التسليم.
        var forwarded = await Send(db, BranchManager()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.BranchManagement },
            CancellationToken.None);

        Assert.True(forwarded.IsSucceeded);
        Assert.Equal(3, forwarded.Data!.AffectedCount);

        var rows = await db.ArchiveTransactions.Where(x => x.BundleId == bundleId).ToListAsync();
        Assert.Equal(3, rows.Count(x => x.Status == ArchiveTransactionStatus.SentToFollower));
        Assert.Equal(2, rows.Count(x => x.Status == ArchiveTransactionStatus.SentToBranch));
    }

    [Fact]
    public async Task Unreceived_items_lead_the_detail_list()
    {
        var (db, bundleId, ids) = await SeedAsync(4, ArchivePeriod.For(2026, 11, 1));

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand
            {
                TransactionIds = [ids[0], ids[1]],
                AtStage = ArchiveStage.BranchManagement
            },
            CancellationToken.None);

        var detail = await new GetArchiveBundleHandler(db, BranchManager(), new FixedClock())
            .Handle(new GetArchiveBundleQuery { BundleId = bundleId, Stage = ArchiveStage.BranchManagement },
                CancellationToken.None);

        Assert.True(detail.IsSucceeded);
        var list = detail.Data!.Transactions;
        Assert.True(list[0].AwaitsMyReceipt);
        Assert.True(list[1].AwaitsMyReceipt);
        Assert.False(list[2].AwaitsMyReceipt);
        // استلمتُ اثنتين من أربع، ولم يستلم المتابع شيئاً بعد.
        Assert.Equal(2, detail.Data.Bundle.ReceivedByMeCount);
        Assert.Equal(0, detail.Data.Bundle.ReceivedOnwardCount);
    }

    [Fact]
    public async Task Receiving_an_already_received_item_is_skipped_not_doubled()
    {
        var (db, bundleId, ids) = await SeedAsync(2, ArchivePeriod.For(2026, 11, 1));

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand
            {
                TransactionIds = ids, AtStage = ArchiveStage.BranchManagement
            },
            CancellationToken.None);

        var again = await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand
            {
                TransactionIds = ids, AtStage = ArchiveStage.BranchManagement
            },
            CancellationToken.None);

        Assert.True(again.IsSucceeded);
        Assert.Equal(0, again.Data!.AffectedCount);
        Assert.Equal(2, again.Data.SkippedCount);

        var events = await db.ArchiveTransactionEvents
            .CountAsync(x => x.EventType == ArchiveEventType.Received);
        Assert.Equal(2, events);
    }

    [Fact]
    public async Task Whole_chain_then_return_then_resend_then_archive()
    {
        var (db, bundleId, ids) = await SeedAsync(1, ArchivePeriod.For(2026, 11, 1));
        var id = ids[0];

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);
        await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.BranchManagement },
            CancellationToken.None);
        await Send(db, BranchManager()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.BranchManagement },
            CancellationToken.None);
        await Receive(db, Follower()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.Follower },
            CancellationToken.None);
        await Send(db, Follower()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.Follower },
            CancellationToken.None);
        await Receive(db, ArchiveOfficer()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.Archive },
            CancellationToken.None);

        Assert.Equal(ArchiveTransactionStatus.Archived,
            (await db.ArchiveTransactions.FindAsync(id))!.Status);

        var returned = await Return(db, ArchiveOfficer()).Handle(
            new ReturnArchiveTransactionsCommand
            {
                TransactionIds = [id],
                Destination = ArchiveStage.SalesEmployee,
                Reason = "ناقص وصل أمانة"
            },
            CancellationToken.None);

        Assert.True(returned.IsSucceeded);
        var afterReturn = await db.ArchiveTransactions.FindAsync(id);
        Assert.Equal(ArchiveTransactionStatus.WithSalesEmployee, afterReturn!.Status);
        Assert.True(afterReturn.HasOpenReturn);
        Assert.Equal("ناقص وصل أمانة", afterReturn.LastReturnReason);

        // يعود ويمشي ثانيةً حتى الأرشفة، فيُغلق الاسترجاع.
        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);
        await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.BranchManagement },
            CancellationToken.None);
        await Send(db, BranchManager()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.BranchManagement },
            CancellationToken.None);
        await Receive(db, Follower()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.Follower },
            CancellationToken.None);
        await Send(db, Follower()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.Follower },
            CancellationToken.None);
        await Receive(db, ArchiveOfficer()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.Archive },
            CancellationToken.None);

        var settled = await db.ArchiveTransactions.FindAsync(id);
        Assert.Equal(ArchiveTransactionStatus.Archived, settled!.Status);
        Assert.False(settled.HasOpenReturn);
        Assert.True(settled.IsSettled);
        Assert.Equal(1, settled.ReturnCount);
    }

    [Fact]
    public async Task Only_the_archive_officer_may_return()
    {
        var (db, bundleId, ids) = await SeedAsync(1, ArchivePeriod.For(2026, 11, 1));
        var id = ids[0];

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);
        await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand { TransactionIds = [id], AtStage = ArchiveStage.BranchManagement },
            CancellationToken.None);

        var attempt = await Return(db, BranchManager()).Handle(
            new ReturnArchiveTransactionsCommand
            {
                TransactionIds = [id],
                Destination = ArchiveStage.SalesEmployee,
                Reason = "ناقص"
            },
            CancellationToken.None);

        Assert.True(attempt.IsFailed);
        Assert.Equal("Archive_Action_Forbidden", attempt.Error!.Value.Code);
    }

    [Fact]
    public async Task A_sales_employee_cannot_send_another_employees_bundle()
    {
        var (db, bundleId, _) = await SeedAsync(1, ArchivePeriod.For(2026, 11, 1));

        var stranger = UserWith(999, SeedPermissions.Archive.Read, SeedPermissions.Archive.Send);

        var result = await Send(db, stranger).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal("Archive_Not_Bundle_Owner", result.Error!.Value.Code);
    }

    [Fact]
    public async Task Undo_send_returns_unreceived_paper_to_the_sender()
    {
        var (db, bundleId, ids) = await SeedAsync(2, ArchivePeriod.For(2026, 11, 1));

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        var undone = await Undo(db, SalesEmployee()).Handle(
            new UndoArchiveSendCommand { TransactionIds = ids, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);

        Assert.True(undone.IsSucceeded);
        Assert.Equal(2, undone.Data!.AffectedCount);

        var statuses = await db.ArchiveTransactions.Where(x => ids.Contains(x.Id))
            .Select(x => x.Status).ToListAsync();
        Assert.All(statuses, s => Assert.Equal(ArchiveTransactionStatus.WithSalesEmployee, s));
    }

    [Fact]
    public async Task Bundle_counters_follow_the_chain()
    {
        var (db, bundleId, ids) = await SeedAsync(4, ArchivePeriod.For(2026, 11, 1));

        await Send(db, SalesEmployee()).Handle(
            new SendArchiveBundleCommand { BundleId = bundleId, FromStage = ArchiveStage.SalesEmployee },
            CancellationToken.None);
        await Receive(db, BranchManager()).Handle(
            new ReceiveArchiveTransactionsCommand
            {
                TransactionIds = [ids[0], ids[1], ids[2]],
                AtStage = ArchiveStage.BranchManagement
            },
            CancellationToken.None);

        var bundle = await db.ArchiveBundles.FindAsync(bundleId);
        Assert.Equal(4, bundle!.ItemCount);
        Assert.Equal(4, bundle.SentCount);
        Assert.Equal(0, bundle.ArchivedCount);
        Assert.NotNull(bundle.FirstSentAtUtc);
    }
}
