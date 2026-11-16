using Application.Common.AdminPanel;
using Application.Common.SeedData;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Tests.Users;

public class RoleDefaultMatrixTests
{
    [Fact]
    public void Admin_includes_panel_tab_and_sales_read_permissions()
    {
        var permissions = RolePermissions.All.Single(x => x.RoleName == SeedRoles.Admin.Name).Permissions;
        Assert.Contains(SeedPermissions.SalesOverview.Read, permissions);
        Assert.Contains(AdminPanelTabs.PermissionName("saleOverview"), permissions);
        Assert.Contains(AdminPanelTabs.PermissionName("sr.incoming"), permissions);
    }

    [Fact]
    public void Director_template_uses_panel_tabs_not_manage_permissions()
    {
        var perms = RoleDefaultMatrix.SeedPermissionsFor(SeedRoles.Director.Name).ToList();
        Assert.Contains(AdminPanelTabs.PermissionName("managementDelegateFollowup"), perms);
        Assert.Contains(AdminPanelTabs.PermissionName("managementCommitteeTasks"), perms);
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions, perms);
        Assert.DoesNotContain(SeedPermissions.DelegateFollowup.Read, perms);
    }

    [Fact]
    public void Monitor_template_excludes_committee_tasks_and_manage_permissions()
    {
        var perms = RoleDefaultMatrix.SeedPermissionsFor(SeedRoles.Monitor.Name).ToList();
        Assert.Contains(AdminPanelTabs.PermissionName("managementDelegateFollowup"), perms);
        Assert.Contains(AdminPanelTabs.PermissionName("saleFollowup"), perms);
        Assert.DoesNotContain(AdminPanelTabs.PermissionName("managementCommitteeTasks"), perms);
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions, perms);
    }

    [Fact]
    public void SalesManager_excludes_delegate_followup_and_manage_permissions()
    {
        var perms = RoleDefaultMatrix.SeedPermissionsFor(SeedRoles.SalesManager.Name).ToList();
        Assert.Contains(AdminPanelTabs.PermissionName("saleFollowup"), perms);
        Assert.Contains(AdminPanelTabs.PermissionName("saleOrders"), perms);
        Assert.DoesNotContain(AdminPanelTabs.PermissionName("managementDelegateFollowup"), perms);
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions, perms);
    }

    [Fact]
    public void Matrix_marks_summary_director_only_and_sales_summary_monitor_only()
    {
        var summary = RoleDefaultMatrix.Rows.Single(r => r.StableId == "managementSummary");
        Assert.True(summary.Director);
        Assert.False(summary.Monitor);
        Assert.Equal("ready", summary.Status);

        var salesSummary = RoleDefaultMatrix.Rows.Single(r => r.StableId == "saleSummary");
        Assert.True(salesSummary.Monitor);
        Assert.False(salesSummary.SalesManager);
        Assert.False(salesSummary.Director);
        Assert.Equal("ready", salesSummary.Status);
    }

    [Fact]
    public void RolePermissions_templates_registered_for_new_roles()
    {
        var roleNames = RolePermissions.All.Select(x => x.RoleName).ToHashSet();
        Assert.Contains(SeedRoles.Director.Name, roleNames);
        Assert.Contains(SeedRoles.Monitor.Name, roleNames);
        Assert.Contains(SeedRoles.SalesManager.Name, roleNames);
    }

    [Fact]
    public void Seed_permission_sets_are_idempotent_without_duplicates()
    {
        foreach (var role in new[]
                 {
                     SeedRoles.Director.Name, SeedRoles.Monitor.Name, SeedRoles.SalesManager.Name
                 })
        {
            var perms = RoleDefaultMatrix.SeedPermissionsFor(role).ToList();
            Assert.Equal(perms.Count, perms.Distinct().Count());
            var again = RoleDefaultMatrix.SeedPermissionsFor(role).ToList();
            Assert.Equal(perms, again);
        }
    }

    [Fact]
    public void User_tab_bundle_excludes_ManagePermissions()
    {
        Assert.True(AdminPanelTabs.TryGet("user", out var tab));
        Assert.DoesNotContain(SeedPermissions.User.ManagePermissions, tab.GrantedActions);
    }

    [Fact]
    public void CommitteeRatings_is_not_ready()
    {
        Assert.True(AdminPanelTabs.TryGet("committeeRatings", out var tab));
        Assert.False(tab.Ready);
        Assert.DoesNotContain(AdminPanelTabs.PermissionName("committeeRatings"), AdminPanelTabs.ReadyPermissionNames);
    }
}
