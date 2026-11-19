using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Tests.Archive;

public class ArchiveTransactionTransitionTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);

    private static ArchiveTransaction NewTransaction() => new(
        bundleId: 1,
        orderId: 10,
        customerId: 100,
        customerName: "حسين كاضم عيسى",
        customerPhone: "07818204253",
        followerUserId: 7,
        periodSourceDate: new DateOnly(2026, 11, 5),
        periodSource: ArchivePeriodSource.SaleDate);

    private static ArchiveTransaction AtStage(ArchiveTransactionStatus target)
    {
        var t = NewTransaction();
        if (target == ArchiveTransactionStatus.WithSalesEmployee) return t;

        Assert.True(t.TrySend(ArchiveStage.SalesEmployee, At, out _));
        if (target == ArchiveTransactionStatus.SentToBranch) return t;

        Assert.True(t.TryReceive(ArchiveStage.BranchManagement, 2, At, out _));
        if (target == ArchiveTransactionStatus.AtBranch) return t;

        Assert.True(t.TrySend(ArchiveStage.BranchManagement, At, out _));
        if (target == ArchiveTransactionStatus.SentToFollower) return t;

        Assert.True(t.TryReceive(ArchiveStage.Follower, 3, At, out _));
        if (target == ArchiveTransactionStatus.AtFollower) return t;

        Assert.True(t.TrySend(ArchiveStage.Follower, At, out _));
        if (target == ArchiveTransactionStatus.SentToArchive) return t;

        Assert.True(t.TryReceive(ArchiveStage.Archive, 4, At, out _));
        return t;
    }

    [Fact]
    public void Happy_path_walks_the_whole_chain()
    {
        var t = AtStage(ArchiveTransactionStatus.Archived);

        Assert.Equal(ArchiveTransactionStatus.Archived, t.Status);
        Assert.True(t.IsSettled);
        Assert.Equal(4, t.ArchivedByUserId);
        Assert.Equal(2, t.ReceivedByBranchUserId);
        Assert.Equal(3, t.ReceivedByFollowerUserId);
    }

    [Theory]
    [InlineData(ArchiveTransactionStatus.SentToBranch, ArchiveStage.SalesEmployee)]
    [InlineData(ArchiveTransactionStatus.AtBranch, ArchiveStage.SalesEmployee)]
    [InlineData(ArchiveTransactionStatus.WithSalesEmployee, ArchiveStage.BranchManagement)]
    [InlineData(ArchiveTransactionStatus.AtFollower, ArchiveStage.BranchManagement)]
    public void Send_is_refused_when_the_paper_is_not_in_your_hands(
        ArchiveTransactionStatus current, ArchiveStage sender)
    {
        var t = AtStage(current);

        Assert.False(t.TrySend(sender, At, out var error));
        Assert.NotNull(error);
        Assert.Equal(current, t.Status);
    }

    [Fact]
    public void Archive_stage_cannot_send_onward()
    {
        var t = AtStage(ArchiveTransactionStatus.Archived);

        Assert.False(t.TrySend(ArchiveStage.Archive, At, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void Sales_employee_cannot_receive()
    {
        var t = NewTransaction();

        Assert.False(t.TryReceive(ArchiveStage.SalesEmployee, 1, At, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void Receiving_twice_is_refused_so_one_handover_writes_one_receipt()
    {
        var t = AtStage(ArchiveTransactionStatus.AtBranch);

        Assert.False(t.TryReceive(ArchiveStage.BranchManagement, 9, At, out var error));
        Assert.NotNull(error);
        Assert.Equal(2, t.ReceivedByBranchUserId);
    }

    [Theory]
    [InlineData(ArchiveStage.SalesEmployee, ArchiveTransactionStatus.SentToBranch, ArchiveTransactionStatus.WithSalesEmployee)]
    [InlineData(ArchiveStage.BranchManagement, ArchiveTransactionStatus.SentToFollower, ArchiveTransactionStatus.AtBranch)]
    [InlineData(ArchiveStage.Follower, ArchiveTransactionStatus.SentToArchive, ArchiveTransactionStatus.AtFollower)]
    public void Sender_may_undo_an_unreceived_send(
        ArchiveStage sender, ArchiveTransactionStatus sent, ArchiveTransactionStatus back)
    {
        var t = AtStage(sent);

        Assert.True(t.TryUndoSend(sender, At, out _));
        Assert.Equal(back, t.Status);
    }

    [Fact]
    public void Undo_send_is_refused_once_received()
    {
        var t = AtStage(ArchiveTransactionStatus.AtBranch);

        Assert.False(t.TryUndoSend(ArchiveStage.SalesEmployee, At, out var error));
        Assert.NotNull(error);
        Assert.Equal(ArchiveTransactionStatus.AtBranch, t.Status);
    }

    [Theory]
    [InlineData(ArchiveStage.SalesEmployee, ArchiveTransactionStatus.WithSalesEmployee)]
    [InlineData(ArchiveStage.BranchManagement, ArchiveTransactionStatus.AtBranch)]
    [InlineData(ArchiveStage.Follower, ArchiveTransactionStatus.AtFollower)]
    public void Return_lands_in_the_destination_custody(ArchiveStage destination, ArchiveTransactionStatus expected)
    {
        var t = AtStage(ArchiveTransactionStatus.Archived);

        Assert.True(t.TryReturn(destination, "ناقص وصل أمانة", At, out _));
        Assert.Equal(expected, t.Status);
        Assert.True(t.HasOpenReturn);
        Assert.Equal(1, t.ReturnCount);
        Assert.Equal("ناقص وصل أمانة", t.LastReturnReason);
        Assert.False(t.IsSettled);
        Assert.Null(t.ArchivedAtUtc);
    }

    [Fact]
    public void Return_is_refused_before_archiving()
    {
        var t = AtStage(ArchiveTransactionStatus.SentToArchive);

        Assert.False(t.TryReturn(ArchiveStage.SalesEmployee, "ناقص", At, out var error));
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ا")]
    public void Return_without_a_real_reason_is_refused(string reason)
    {
        var t = AtStage(ArchiveTransactionStatus.Archived);

        Assert.False(t.TryReturn(ArchiveStage.SalesEmployee, reason, At, out var error));
        Assert.NotNull(error);
        Assert.Equal(ArchiveTransactionStatus.Archived, t.Status);
    }

    [Fact]
    public void Return_to_archive_itself_is_refused()
    {
        var t = AtStage(ArchiveTransactionStatus.Archived);

        Assert.False(t.TryReturn(ArchiveStage.Archive, "ناقص", At, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void Re_archiving_after_a_return_clears_the_open_return()
    {
        var t = AtStage(ArchiveTransactionStatus.Archived);
        Assert.True(t.TryReturn(ArchiveStage.SalesEmployee, "ناقص وصل أمانة", At, out _));

        Assert.True(t.TrySend(ArchiveStage.SalesEmployee, At, out _));
        Assert.True(t.TryReceive(ArchiveStage.BranchManagement, 2, At, out _));
        Assert.True(t.TrySend(ArchiveStage.BranchManagement, At, out _));
        Assert.True(t.TryReceive(ArchiveStage.Follower, 3, At, out _));
        Assert.True(t.TrySend(ArchiveStage.Follower, At, out _));
        Assert.True(t.TryReceive(ArchiveStage.Archive, 4, At, out _));

        Assert.True(t.IsSettled);
        Assert.False(t.HasOpenReturn);
        Assert.Equal(1, t.ReturnCount);
    }

    [Fact]
    public void Voided_transaction_cannot_move_forward_but_keeps_its_custody()
    {
        var t = AtStage(ArchiveTransactionStatus.AtBranch);

        Assert.True(t.Void("المبيع خرج من مُتمّ", At));
        Assert.False(t.TrySend(ArchiveStage.BranchManagement, At, out var error));
        Assert.NotNull(error);
        Assert.Equal(ArchiveTransactionStatus.AtBranch, t.Status);

        Assert.True(t.Revive());
        Assert.True(t.TrySend(ArchiveStage.BranchManagement, At, out _));
    }

    [Fact]
    public void Void_and_revive_are_idempotent()
    {
        var t = NewTransaction();

        Assert.True(t.Void("سبب", At));
        Assert.False(t.Void("سبب", At));
        Assert.True(t.Revive());
        Assert.False(t.Revive());
    }

    [Fact]
    public void Rebucketing_is_allowed_only_before_the_paper_moves()
    {
        var fresh = NewTransaction();
        Assert.True(fresh.TryRebucket(2, out _));
        Assert.Equal(2, fresh.BundleId);

        var moved = AtStage(ArchiveTransactionStatus.SentToBranch);
        Assert.False(moved.TryRebucket(2, out var error));
        Assert.NotNull(error);
        Assert.Equal(1, moved.BundleId);
    }

    [Fact]
    public void Every_transition_bumps_the_concurrency_stamp()
    {
        var t = NewTransaction();
        var start = t.ConcurrencyStamp;

        Assert.True(t.TrySend(ArchiveStage.SalesEmployee, At, out _));
        Assert.Equal(start + 1, t.ConcurrencyStamp);

        Assert.True(t.TryReceive(ArchiveStage.BranchManagement, 2, At, out _));
        Assert.Equal(start + 2, t.ConcurrencyStamp);

        Assert.False(t.MatchesExpectedVersion(start));
        Assert.True(t.MatchesExpectedVersion(start + 2));
    }
}
