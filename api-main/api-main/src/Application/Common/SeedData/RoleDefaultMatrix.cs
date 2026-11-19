using Application.Common.AdminPanel;

namespace Application.Common.SeedData;

/// <summary>
/// مصفوفة القوالب الافتراضية للتنقل/الصلاحيات — مستخرجة من FE_Company
/// (DefaultLayoutWithVerticalNav + plugins/1.router)، قابلة للتخصيص عبر overrides.
/// ليست قيود role في وقت التشغيل؛ الـevaluator يعتمد الصلاحيات فقط.
/// البذرة الافتراضية للأدوار الإدارية = مفاتيح Panel.Tab.* (تُوسَّع لحزم الإجراءات وقت الاحتساب).
/// </summary>
public static class RoleDefaultMatrix
{
    private static string T(string tabKey) => AdminPanelTabs.PermissionName(tabKey);

    public sealed record NavDefault(
        string StableId,
        string LegacyRouteName,
        bool Director,
        bool Monitor,
        bool SalesManager,
        string? ExistingPermissionHint,
        string Status);

    /// <summary>إثبات مصدري — لا صلاحيات placeholder للميزات غير المنفَّذة.</summary>
    public static readonly IReadOnlyList<NavDefault> Rows =
    [
        new("managementSummary", "director-summary", true, false, false,
            Permissions.ManagementSummary.Read, "ready"),
        new("managementRatings", "director-ratings", true, true, false,
            Permissions.ManagementRatings.Read, "ready"),
        new("managementDelegateFollowup", "director-delegate-followup", true, true, false,
            Permissions.DelegateFollowup.Read, "ready"),
        new("managementSalesMap", "management-sales-map", true, true, false,
            Permissions.ManagementSalesMap.Read, "ready"),
        new("managementFollowerWork", "director-follower-work", true, true, false,
            Permissions.FollowerWork.Read, "ready"),
        new("managementTasks", "director-assigned-tasks", true, false, false,
            Permissions.ManagementTask.Read, "ready"),
        new("managementCommitteeTasks", "director-committee-tasks", true, false, false,
            Permissions.CommitteeTask.Read, "ready"),
        new("managementAppNotes", "director-notes", true, false, false,
            Permissions.AppNote.Read, "ready"),
        new("managementWaivers", "director-waivers", true, true, false,
            Permissions.Waiver.Read, "ready"),
        new("managementAnnouncements", "director-announcements", true, false, false,
            Permissions.Announcement.Read, "ready"),
        new("managementTutorials", "director-tutorial-videos", true, false, false,
            Permissions.Tutorial.Read, "ready"),
        new("managementLegalNames", "decision-board", false, true, false,
            Permissions.LegalNames.Read, "ready"),
        new("managementPriceBoard", "price-board", false, true, false,
            Permissions.PriceBoard.Read, "ready"),
        new("saleOverview", "sales-manager-dashboard", false, true, true, null, "not_implemented"),
        new("saleEmployees", "sales-manager-employees", false, true, true, null, "not_implemented"),
        new("saleFollowup", "director-sales-followup", true, true, false,
            Permissions.SalesFollowup.Read, "ready"),
        new("saleLiveMap", "sales-manager-map", false, true, true,
            Permissions.SalesLocation.Read, "ready"),
        new("saleRoutes", "sales-manager-routes", false, true, true,
            Permissions.SalesRoute.Read, "ready"),
        new("saleOrders", "sales-manager-sales", false, true, true,
            Permissions.Order.Read, "ready"),
        new("saleSummary", "sales-manager-sales-summary", false, true, false,
            Permissions.SalesSummary.Read, "ready"),
        new("saleLegal", "sales-manager-legal-followups", false, true, true,
            Permissions.LegalFollowup.Read, "ready"),
        new("saleRequests", "sales-manager-requests", true, true, true, null, "ready"),
    ];

    /// <summary>
    /// صلاحيات جاهزة تُزرع لقوالب الأدوار فقط.
    /// لا User.ManagePermissions ولا إدارة Admin.
    /// الميزات غير المنفَّذة لا تُزرع بصلاحية وهمية.
    /// </summary>
    public static IReadOnlyList<string> SeedPermissionsFor(string roleName)
    {
        if (roleName == Roles.Director.Name)
            return DirectorReadyPermissions;
        if (roleName == Roles.Monitor.Name)
            return MonitorReadyPermissions;
        if (roleName == Roles.SalesManager.Name)
            return SalesManagerReadyPermissions;
        return [];
    }

    private static readonly string[] SharedSalesRequestPanelTabs =
    [
        T("sr.unassigned"),
        T("sr.incoming"),
        T("sr.prepared"),
        T("sr.inspected"),
        T("sr.pending"),
        T("sr.rejected"),
        T("sr.sold"),
        T("sr.sent"),
        T("sr.exceptions"),
        T("sr.deleted"),
        T("sr.legal"),
    ];

    private static readonly string[] DirectorReadyPermissions =
    [
        T("home"),
        T("notifications"),
        T("activityLog"),
        T("user"),
        T("customer"),
        T("payment"),
        T("order"),
        T("managementSummary"),
        T("managementRatings"),
        T("managementDelegateFollowup"),
        T("managementSalesMap"),
        T("managementCommitteeTasks"),
        T("managementAppNotes"),
        T("managementWaivers"),
        T("managementAnnouncements"),
        T("managementTutorials"),
        T("followerSales"),
        T("followerDeclining"),
        T("followerNotes"),
        T("followerOldCustomers"),
        T("followerDelegateNotes"),
        T("followerCollection"),
        T("followerLiveMap"),
        T("followerRoutes"),
        T("followerShopCapture"),
        T("followerManage"),
        T("saleFollowup"),
        T("saleSummary"),
        T("saleOrders"),
        T("saleLegal"),
        T("app-branch-branchAssignedTasks"),
        T("app-branch-legalNames"),
        T("app-branch-branchDeclining"),
        T("app-branch-customers"),
        T("app-branch-sales"),
        T("app-branch-wrongSalesBranch"),
        T("app-branch-branchRatings"),
        T("app-branch-payments"),
        T("app-branch-branchWaivers"),
        T("app-branch-legalDecisions"),
        T("branchVisitWorkspace"),
        T("app-delegate-delegateHome"),
        T("app-delegate-assignedTasks"),
        T("app-delegate-delegateRatings"),
        T("app-delegate-complaints"),
        T("app-delegate-exceptions"),
        T("app-delegate-wrongSalesDelegate"),
        T("app-delegate-delegateWaivers"),
        T("app-delegate-accounts"),
        T("app-delegate-branchVisits"),
        T("app-delegate-delegateSettings"),
        ..SharedSalesRequestPanelTabs,
    ];

    private static readonly string[] MonitorReadyPermissions =
    [
        T("home"),
        T("notifications"),
        T("activityLog"),
        T("user"),
        T("customer"),
        T("payment"),
        T("order"),
        T("managementRatings"),
        T("managementDelegateFollowup"),
        T("managementSalesMap"),
        T("managementLegalNames"),
        T("managementPriceBoard"),
        T("managementAppNotes"),
        T("managementWaivers"),
        T("followerSales"),
        T("followerDeclining"),
        T("followerNotes"),
        T("followerOldCustomers"),
        T("followerDelegateNotes"),
        T("followerCollection"),
        T("followerLiveMap"),
        T("followerRoutes"),
        T("followerShopCapture"),
        T("followerManage"),
        T("saleOverview"),
        T("saleEmployees"),
        T("saleFollowup"),
        T("saleLiveMap"),
        T("saleRoutes"),
        T("saleOrders"),
        T("saleSummary"),
        T("saleLegal"),
        T("app-branch-wrongSalesBranch"),
        T("app-branch-legalDecisions"),
        ..SharedSalesRequestPanelTabs,
    ];

    private static readonly string[] SalesManagerReadyPermissions =
    [
        T("home"),
        T("notifications"),
        T("user"),
        T("customer"),
        T("saleOverview"),
        T("saleEmployees"),
        T("saleFollowup"),
        T("saleLiveMap"),
        T("saleRoutes"),
        T("saleOrders"),
        T("saleSummary"),
        T("saleLegal"),
        T("saleExcelSearch"),
        T("managementAppNotes"),
        T("managementCommitteeTasks"),
        ..SharedSalesRequestPanelTabs,
    ];
}
