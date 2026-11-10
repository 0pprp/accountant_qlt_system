using Application.Users.Common;
using Domain.Entities.RoleAggregate;

namespace Application.Tests.Users;

public class EffectivePermissionCalculatorTests
{
    private static Permission P(int id, string name) =>
        new(name, name, "Scope", "Scope") { Id = id };

    [Fact]
    public void Inherit_allows_what_role_allows()
    {
        var role = new List<Permission> { P(1, "Order.Read"), P(2, "User.Read") };
        var effective = EffectivePermissionCalculator.Compute(role, [], []);
        Assert.Equal(2, effective.Count);
        Assert.Contains(effective, x => x.Id == 1);
    }

    [Fact]
    public void Deny_blocks_inherited_permission()
    {
        var role = new List<Permission> { P(1, "Order.Read"), P(2, "User.Read") };
        var deny = new List<Permission> { P(1, "Order.Read") };
        var effective = EffectivePermissionCalculator.Compute(role, [], deny);
        Assert.Single(effective);
        Assert.Equal(2, effective[0].Id);
    }

    [Fact]
    public void Allow_adds_non_inherited_permission()
    {
        var role = new List<Permission> { P(1, "Order.Read") };
        var allow = new List<Permission> { P(3, "User.Create") };
        var effective = EffectivePermissionCalculator.Compute(role, allow, []);
        Assert.Equal(2, effective.Count);
        Assert.Contains(effective, x => x.Id == 3);
    }

    [Fact]
    public void Inherit_removes_exception_and_restores_role()
    {
        var role = new List<Permission> { P(1, "Order.Read") };
        var afterDenyRemoved = EffectivePermissionCalculator.Compute(role, [], []);
        Assert.Contains(afterDenyRemoved, x => x.Id == 1);
    }

    [Fact]
    public void Deny_wins_over_allow_and_role()
    {
        var role = new List<Permission> { P(1, "Order.Read") };
        var allow = new List<Permission> { P(1, "Order.Read") };
        var deny = new List<Permission> { P(1, "Order.Read") };
        var effective = EffectivePermissionCalculator.Compute(role, allow, deny);
        Assert.Empty(effective);
    }

    [Fact]
    public void Role_change_keeps_denial_effect()
    {
        var newRole = new List<Permission> { P(1, "Order.Read"), P(5, "Safe.Read") };
        var deny = new List<Permission> { P(1, "Order.Read") };
        var allow = new List<Permission> { P(9, "Expense.Read") };
        var effective = EffectivePermissionCalculator.Compute(newRole, allow, deny);
        Assert.DoesNotContain(effective, x => x.Id == 1);
        Assert.Contains(effective, x => x.Id == 5);
        Assert.Contains(effective, x => x.Id == 9);
    }

    [Fact]
    public void Legacy_accounts_without_denials_keep_union_result()
    {
        var role = new List<Permission> { P(1, "Order.Read") };
        var allow = new List<Permission> { P(2, "User.Create") };
        var effective = EffectivePermissionCalculator.Compute(role, allow, []);
        Assert.Equal(2, effective.Count);
    }
}
