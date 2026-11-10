using Domain.Entities.OrderListAggregate;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.FollowerWork;

public class FollowerDelegateNotesAndCollectionRulesTests
{
    [Fact]
    public void Delegate_notes_read_does_not_grant_write()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.DelegateNotesRead,
            SeedPermissions.FollowerWork.DelegateNotesWrite);
    }

    [Fact]
    public void Collection_read_does_not_grant_manage_enabled()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.CollectionRead,
            SeedPermissions.FollowerWork.CollectionManageEnabled);
    }

    [Fact]
    public void Notes_and_delegate_notes_permissions_are_distinct()
    {
        Assert.NotEqual(
            SeedPermissions.FollowerWork.NotesRead,
            SeedPermissions.FollowerWork.DelegateNotesRead);
        Assert.NotEqual(
            SeedPermissions.FollowerWork.NotesWrite,
            SeedPermissions.FollowerWork.DelegateNotesWrite);
    }

    [Fact]
    public void Enable_collection_sets_assigned_at_and_bumps_version()
    {
        var list = new OrderList("قائمة", mandobId: 2, motabaId: 5, branchId: 1);
        Assert.False(list.FollowerCollectionEnabled);
        Assert.Equal(0, list.CollectionToggleVersion);

        var at = DateTimeOffset.UtcNow;
        Assert.True(list.SetFollowerCollectionEnabled(true, at, expectedVersion: 0));
        Assert.True(list.FollowerCollectionEnabled);
        Assert.Equal(at, list.FollowerCollectionAssignedAtUtc);
        Assert.Equal(1, list.CollectionToggleVersion);
    }

    [Fact]
    public void Same_state_is_idempotent_without_version_bump()
    {
        var list = new OrderList("قائمة", 2, 5, 1);
        list.SetFollowerCollectionEnabled(true, DateTimeOffset.UtcNow, 0);
        var v = list.CollectionToggleVersion;
        Assert.False(list.SetFollowerCollectionEnabled(true, DateTimeOffset.UtcNow, v));
        Assert.Equal(v, list.CollectionToggleVersion);
    }

    [Fact]
    public void Stale_expected_version_throws()
    {
        var list = new OrderList("قائمة", 2, 5, 1);
        Assert.Throws<InvalidOperationException>(() =>
            list.SetFollowerCollectionEnabled(true, DateTimeOffset.UtcNow, expectedVersion: 99));
    }

    [Fact]
    public void Disable_does_not_clear_assigned_at()
    {
        var list = new OrderList("قائمة", 2, 5, 1);
        var at = DateTimeOffset.UtcNow;
        list.SetFollowerCollectionEnabled(true, at, 0);
        list.SetFollowerCollectionEnabled(false, DateTimeOffset.UtcNow, 1);
        Assert.False(list.FollowerCollectionEnabled);
        Assert.Equal(at, list.FollowerCollectionAssignedAtUtc);
    }
}
