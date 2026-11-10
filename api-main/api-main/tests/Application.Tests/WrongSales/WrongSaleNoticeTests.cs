using Application.FollowerWork.Common;
using Domain.Entities.WrongSaleAggregate;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.WrongSales;

public class WrongSaleNoticeTests
{
    [Fact]
    public void Rating_threshold_matches_source()
    {
        Assert.True(FollowerSaleAuthRules.IsWrongSaleRating(6));
        Assert.False(FollowerSaleAuthRules.IsWrongSaleRating(7));
        Assert.Contains("مبيعات خاطئة", FollowerSaleAuthRules.FormatWrongSaleReason(
            5, "ملاحظة", 1000, 200, "زبون", "07"));
    }

    [Fact]
    public void Delegate_read_and_branch_review_are_independent_and_idempotent()
    {
        var n = new WrongSaleNotice(
            10, 20, 1, 5, "متابع", "زبون", "07", 4, "ملاحظة",
            1000, 200, "مبيعات خاطئة\n…", DateTimeOffset.UtcNow);

        Assert.True(n.MarkDelegateRead(9, DateTimeOffset.UtcNow, 0));
        Assert.False(n.MarkDelegateRead(9, DateTimeOffset.UtcNow, 1));
        Assert.Throws<InvalidOperationException>(() =>
            n.MarkBranchReviewed(8, "ab", DateTimeOffset.UtcNow, 1));
        Assert.True(n.MarkBranchReviewed(8, "تمت المراجعة", DateTimeOffset.UtcNow, 1));
        Assert.Throws<InvalidOperationException>(() =>
            n.MarkBranchReviewed(8, "تمت مرة أخرى", DateTimeOffset.UtcNow, 2));
        Assert.Equal(WrongSaleNoticeStatus.BranchReviewed, n.Status);
    }

    [Fact]
    public void Branch_manager_role_has_wrong_sale_without_manage_permissions()
    {
        var bm = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.BranchManager.Name);
        Assert.Contains(SeedPermissions.WrongSale.Read, bm.Permissions);
        Assert.Contains(SeedPermissions.WrongSale.MarkBranchReviewed, bm.Permissions);
        Assert.Contains(SeedPermissions.OrderList.Read, bm.Permissions);
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions, bm.Permissions);
    }

    [Fact]
    public void Permissions_outside_all()
    {
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.WrongSale.Read);
        var director = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Director.Name);
        Assert.Contains(SeedPermissions.WrongSale.MarkDelegateRead, director);
        var monitor = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Monitor.Name);
        Assert.Contains(SeedPermissions.WrongSale.MarkBranchReviewed, monitor);
    }
}
