using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.Tests.ManagementTasks;

public class ManagementTaskDomainTests
{
    private static ManagementTask NewTask(int assigneeId = 10, int creatorId = 1, int branchId = 1)
        => new(
            "عنوان",
            "وصف",
            ManagementTaskPriority.Medium,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)),
            null,
            branchId,
            creatorId,
            assigneeId,
            "مكلّف",
            null,
            null);

    [Fact]
    public void Accept_FromSent_ToAccepted()
    {
        var task = NewTask();
        var ok = task.TryAccept(10, DateTimeOffset.UtcNow, out var error);
        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(ManagementTaskStatus.Accepted, task.Status);
        Assert.Equal(1, task.ConcurrencyStamp);
    }

    [Fact]
    public void Reject_RequiresNote()
    {
        var task = NewTask();
        var ok = task.TryReject(10, "  ", DateTimeOffset.UtcNow, out var error);
        Assert.False(ok);
        Assert.Equal("ملاحظة الرفض مطلوبة.", error);
        Assert.Equal(ManagementTaskStatus.Sent, task.Status);
    }

    [Fact]
    public void Reject_FromSent_ToReturned()
    {
        var task = NewTask();
        var ok = task.TryReject(10, "سبب", DateTimeOffset.UtcNow, out _);
        Assert.True(ok);
        Assert.Equal(ManagementTaskStatus.Returned, task.Status);
        Assert.Equal("سبب", task.RejectNote);
    }

    [Fact]
    public void Transfer_Then_BranchComplete_RequiresConfirm()
    {
        var task = NewTask(assigneeId: 10);
        Assert.True(task.TryAccept(10, DateTimeOffset.UtcNow, out _));
        Assert.True(task.TryTransfer(10, 20, "فرع", null, DateTimeOffset.UtcNow, out _));
        Assert.Equal(ManagementTaskStatus.Transferred, task.Status);
        Assert.Equal(20, task.CurrentHolderUserId);

        Assert.True(task.TryAccept(20, DateTimeOffset.UtcNow, out _));
        Assert.True(task.TryComplete(20, DateTimeOffset.UtcNow, out _));
        Assert.Equal(ManagementTaskStatus.AwaitingDelegatedConfirm, task.Status);
        Assert.Equal(10, task.CurrentHolderUserId);

        Assert.True(task.TryConfirm(10, DateTimeOffset.UtcNow, out _));
        Assert.Equal(ManagementTaskStatus.Completed, task.Status);
    }

    [Fact]
    public void SelfComplete_ByAssignee_NoConfirm()
    {
        var task = NewTask(assigneeId: 10);
        Assert.True(task.TryAccept(10, DateTimeOffset.UtcNow, out _));
        Assert.True(task.TryComplete(10, DateTimeOffset.UtcNow, out _));
        Assert.Equal(ManagementTaskStatus.Completed, task.Status);
        Assert.NotNull(task.AssigneeCompletedAtUtc);
        Assert.Null(task.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_WithoutTransfer_Fails()
    {
        var task = NewTask();
        Assert.True(task.TryAccept(10, DateTimeOffset.UtcNow, out _));
        var ok = task.TryConfirm(10, DateTimeOffset.UtcNow, out var error);
        Assert.False(ok);
        Assert.Equal("لا يمكن التصديق بدون نقل سابق.", error);
    }

    [Fact]
    public void NonHolder_CannotAccept()
    {
        var task = NewTask(assigneeId: 10);
        var ok = task.TryAccept(99, DateTimeOffset.UtcNow, out var error);
        Assert.False(ok);
        Assert.Equal("لست حامل المهمة الحالي.", error);
    }

    [Fact]
    public void ExpectedVersion_MatchesStamp()
    {
        var task = NewTask();
        Assert.True(task.MatchesExpectedVersion(0));
        Assert.True(task.TryAccept(10, DateTimeOffset.UtcNow, out _));
        Assert.True(task.MatchesExpectedVersion(1));
        Assert.False(task.MatchesExpectedVersion(0));
    }

    [Fact]
    public void Resend_FromReturned_ClearsTransfer()
    {
        var task = NewTask(assigneeId: 10);
        Assert.True(task.TryAccept(10, DateTimeOffset.UtcNow, out _));
        Assert.True(task.TryTransfer(10, 20, "فرع", null, DateTimeOffset.UtcNow, out _));
        Assert.True(task.TryReject(20, "رفض فرع", DateTimeOffset.UtcNow, out _));
        Assert.Equal(ManagementTaskStatus.Returned, task.Status);

        var ok = task.TryResend(
            30,
            "جديد",
            "عنوان2",
            "وصف2",
            ManagementTaskPriority.High,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            out _);
        Assert.True(ok);
        Assert.Equal(ManagementTaskStatus.Sent, task.Status);
        Assert.Equal(30, task.AssigneeUserId);
        Assert.Equal(30, task.CurrentHolderUserId);
        Assert.Null(task.RejectNote);
        Assert.Null(task.TransferredAtUtc);
    }
}
