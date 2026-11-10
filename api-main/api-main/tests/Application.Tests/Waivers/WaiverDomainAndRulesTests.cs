using Application.CustomerDecisions.Common;
using Domain.Entities.WaiverAggregate;
using Domain.Entities.WaiverAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.Waivers;

public class WaiverDomainAndRulesTests
{
    private static CustomerWaiverRequest NewPending(int requesterId = 10)
    {
        return new CustomerWaiverRequest(
            customerId: 1,
            branchId: 2,
            orderId: 99,
            requesterUserId: requesterId,
            requesterName: "فرع",
            note: "طلب تنازل لاختبار",
            createdAtUtc: DateTimeOffset.Parse("2026-11-09T10:00:00Z"));
    }

    [Fact]
    public void Create_starts_pending_unapplied()
    {
        var w = NewPending();
        Assert.Equal(WaiverRequestStatus.Pending, w.Status);
        Assert.False(w.Applied);
        Assert.Equal(0, w.ConcurrencyStamp);
        Assert.Null(w.ApprovedAmount);
    }

    [Fact]
    public void Approve_moves_to_awaiting_accountant()
    {
        var w = NewPending();
        var at = DateTimeOffset.Parse("2026-11-09T11:00:00Z");
        w.ApproveByDelegate(20, "مفوض", 150_000, "موافقة جزئية", 0, at);
        Assert.Equal(WaiverRequestStatus.AwaitingAccountant, w.Status);
        Assert.Equal(150_000, w.ApprovedAmount);
        Assert.Equal(1, w.ConcurrencyStamp);
        Assert.Equal(at, w.DecidedAtUtc);
    }

    [Fact]
    public void Reject_from_pending()
    {
        var w = NewPending();
        w.Reject(20, "مفوض", "غير مستحق", 0, DateTimeOffset.UtcNow);
        Assert.Equal(WaiverRequestStatus.Rejected, w.Status);
        Assert.False(w.Applied);
    }

    [Fact]
    public void Requester_cannot_approve_own_request()
    {
        var w = NewPending(requesterId: 7);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            w.ApproveByDelegate(7, "نفسه", 100, null, 0, DateTimeOffset.UtcNow));
        Assert.Contains("صاحب الطلب", ex.Message);
    }

    [Fact]
    public void Concurrent_second_decision_fails_version()
    {
        var w = NewPending();
        w.ApproveByDelegate(20, "مفوض", 50, null, 0, DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            w.Reject(21, "آخر", "رفض متأخر", 0, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Apply_is_idempotent_after_success()
    {
        var w = NewPending();
        w.ApproveByDelegate(20, "مفوض", 100, null, 0, DateTimeOffset.UtcNow);
        Assert.True(w.ApplyByAccountant(30, "محاسب", 80, 1, DateTimeOffset.UtcNow));
        Assert.Equal(WaiverRequestStatus.Approved, w.Status);
        Assert.True(w.Applied);
        Assert.Equal(80, w.AppliedAmount);
        Assert.False(w.ApplyByAccountant(30, "محاسب", 80, 2, DateTimeOffset.UtcNow));
        Assert.Equal(2, w.ConcurrencyStamp);
    }

    [Fact]
    public void Cap_limits_to_remaining()
    {
        Assert.Equal(40, WaiverDiscount.Cap(saleTotal: 100, paidTotal: 60, requested: 80));
        Assert.Equal(25, WaiverDiscount.Cap(100, 0, 25));
        Assert.Throws<ArgumentOutOfRangeException>(() => WaiverDiscount.Cap(100, 0, 0));
    }

    [Fact]
    public void Permissions_are_outside_all_and_separated()
    {
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.Waiver.Decide);
        Assert.Contains(SeedPermissions.Waiver.OutsideAll, p => p.Name == SeedPermissions.Waiver.Create);
        Assert.NotEqual(SeedPermissions.Waiver.Read, SeedPermissions.Waiver.Decide);
        Assert.NotEqual(SeedPermissions.Waiver.Decide, SeedPermissions.Waiver.Apply);
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.CustomerDecision.Create);
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.LegalNames.Read);
    }

    [Fact]
    public void Branch_manager_template_does_not_get_decide_or_apply()
    {
        var role = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.BranchManager.Name);
        Assert.Contains(SeedPermissions.Waiver.Create, role.Permissions);
        Assert.Contains(SeedPermissions.Waiver.Read, role.Permissions);
        Assert.DoesNotContain(SeedPermissions.Waiver.Decide, role.Permissions);
        Assert.DoesNotContain(SeedPermissions.Waiver.Apply, role.Permissions);
        Assert.Contains(SeedPermissions.LegalNames.Read, role.Permissions);
        Assert.Contains(SeedPermissions.CustomerDecision.Create, role.Permissions);
    }

    [Fact]
    public void Director_gets_decide_not_apply()
    {
        var perms = Application.Common.SeedData.RoleDefaultMatrix.SeedPermissionsFor(
            Application.Common.SeedData.Roles.Director.Name);
        Assert.Contains(SeedPermissions.Waiver.Decide, perms);
        Assert.Contains(SeedPermissions.Waiver.Read, perms);
        Assert.DoesNotContain(SeedPermissions.Waiver.Apply, perms);
        Assert.DoesNotContain(SeedPermissions.Waiver.Create, perms);
    }

    [Fact]
    public void Accountant_gets_apply_not_decide()
    {
        var branch = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.BranchAccountant.Name);
        Assert.Contains(SeedPermissions.Waiver.Apply, branch.Permissions);
        Assert.Contains(SeedPermissions.Waiver.Read, branch.Permissions);
        Assert.DoesNotContain(SeedPermissions.Waiver.Decide, branch.Permissions);
        Assert.DoesNotContain(SeedPermissions.Waiver.Create, branch.Permissions);

        var main = Application.Common.SeedData.RolePermissions.All
            .Single(r => r.RoleName == Application.Common.SeedData.Roles.MainAccountant.Name);
        Assert.Contains(SeedPermissions.Waiver.Apply, main.Permissions);
        Assert.DoesNotContain(SeedPermissions.Waiver.Decide, main.Permissions);
        Assert.DoesNotContain(SeedPermissions.Waiver.Create, main.Permissions);
    }

    [Theory]
    [InlineData(1000, 20, 980, null, false, false, false, false, false, true)]
    [InlineData(1000, 50, 950, null, false, false, false, false, false, false)] // >3.8%
    [InlineData(1000, 20, 980, null, true, false, false, false, false, false)] // already legal
    [InlineData(1000, 20, 980, null, false, true, false, false, false, false)] // fake sale
    [InlineData(1000, 20, 0, null, false, false, false, false, false, false)] // no remaining
    [InlineData(1000, 20, 980, null, false, false, true, false, false, false)] // snooze
    [InlineData(1000, 20, 980, null, false, false, false, true, false, false)] // pending conversion
    [InlineData(1000, 20, 980, null, false, false, false, false, true, false)] // open waiver
    public void Weak_payer_queue_rules(
        double sale,
        double weekPaid,
        double remaining,
        DateOnly? lastPay,
        bool isLegal,
        bool fake,
        bool snooze,
        bool pendingConv,
        bool openWaiver,
        bool expected)
    {
        var today = new DateOnly(2026, 11, 9);
        Assert.Equal(
            expected,
            WeakPayerRules.Matches(
                sale, weekPaid, remaining, lastPay, today,
                isLegal, fake, snooze, pendingConv, openWaiver));
    }

    [Fact]
    public void Weak_payer_requires_40_days_when_last_payment_exists()
    {
        var today = new DateOnly(2026, 11, 9);
        Assert.False(WeakPayerRules.Matches(
            1000, 10, 900, today.AddDays(-39), today,
            false, false, false, false, false));
        Assert.True(WeakPayerRules.Matches(
            1000, 10, 900, today.AddDays(-40), today,
            false, false, false, false, false));
    }
}
