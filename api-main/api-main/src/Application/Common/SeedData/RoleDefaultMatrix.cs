namespace Application.Common.SeedData;

/// <summary>
/// مصفوفة القوالب الافتراضية للتنقل/الصلاحيات — مستخرجة من FE_Company
/// (DefaultLayoutWithVerticalNav + plugins/1.router)، قابلة للتخصيص عبر overrides.
/// ليست قيود role في وقت التشغيل؛ الـevaluator يعتمد الصلاحيات فقط.
/// </summary>
public static class RoleDefaultMatrix
{
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

    private static readonly string[] SharedSalesRequestTabs =
    [
        Permissions.SalesRequest.TabIncoming,
        Permissions.SalesRequest.TabPrepared,
        Permissions.SalesRequest.TabInspected,
        Permissions.SalesRequest.TabPending,
        Permissions.SalesRequest.TabRejected,
        Permissions.SalesRequest.TabSold,
        Permissions.SalesRequest.TabSent,
        Permissions.SalesRequest.TabExceptions,
        Permissions.SalesRequest.TabDeleted,
        Permissions.SalesRequest.TabLegal,
        Permissions.SalesRequest.ReadUnassigned,
    ];

    private static readonly string[] DirectorReadyPermissions =
    [
        Permissions.ManagementSummary.Read,
        Permissions.ManagementSummary.Drilldown,
        Permissions.ManagementRatings.Read,
        Permissions.ManagementRatings.DetailsRead,
        Permissions.FollowerWork.Read,
        Permissions.FollowerWork.SalesReviewRead,
        Permissions.FollowerWork.DecliningRead,
        Permissions.FollowerWork.NotesRead,
        Permissions.FollowerWork.OldCustomersRead,
        Permissions.FollowerWork.DelegateNotesRead,
        Permissions.FollowerWork.CollectionRead,
        Permissions.FollowerWork.CollectionManageEnabled,
        Permissions.FollowerWork.LocationRead,
        Permissions.FollowerWork.RouteRead,
        Permissions.LegalFollowup.Read,
        Permissions.LegalFollowup.Create,
        Permissions.LegalFollowup.Cancel,
        Permissions.SalesSummary.Read,
        Permissions.SalesSummary.Drilldown,
        Permissions.Safe.Read,
        Permissions.DelegateFollowup.Read,
        Permissions.DelegateFollowup.UpsertBoardRating,
        Permissions.SalesFollowup.Read,
        Permissions.SalesFollowup.UpsertManagerRating,
        Permissions.ManagementTask.Read,
        Permissions.ManagementTask.Create,
        Permissions.ManagementTask.Assign,
        Permissions.ManagementTask.Accept,
        Permissions.ManagementTask.Reject,
        Permissions.ManagementTask.Complete,
        Permissions.ManagementTask.Confirm,
        Permissions.ManagementTask.Transfer,
        Permissions.ManagementTask.ReadAttachment,
        Permissions.CommitteeTask.Read,
        Permissions.CommitteeTask.Write,
        Permissions.AppNote.Read,
        Permissions.Waiver.Read,
        Permissions.Waiver.Decide,
        Permissions.CustomerDecision.Read,
        Permissions.LegalConversion.Decide,
        Permissions.Announcement.Read,
        Permissions.Announcement.Create,
        Permissions.Announcement.Update,
        Permissions.Announcement.Delete,
        Permissions.Tutorial.Read,
        Permissions.Tutorial.Create,
        Permissions.Tutorial.Update,
        Permissions.Tutorial.Delete,
        Permissions.WrongSale.Read,
        Permissions.WrongSale.MarkDelegateRead,
        Permissions.WrongSale.MarkBranchReviewed,
        Permissions.BranchNewSale.Read,
        Permissions.BranchNewSale.MarkRead,
        Permissions.BranchDeclining.Read,
        Permissions.Complaint.Read,
        Permissions.Complaint.MarkRead,
        Permissions.BranchVisit.Read,
        Permissions.BranchVisit.Open,
        Permissions.BranchVisit.Rate,
        Permissions.BranchVisit.Depart,
        Permissions.BranchManagerAccount.Read,
        Permissions.BranchManagerAccount.Write,
        Permissions.DelegateHome.Read,
        Permissions.Order.Read,
        Permissions.OrderList.Read,
        Permissions.Customer.Read,
        Permissions.User.Read,
        Permissions.Notification.Read,
        Permissions.Notification.Update,
        Permissions.ActivityLog.Read,
        Permissions.SalesRequest.ExceptionDecide,
        ..SharedSalesRequestTabs,
    ];

    private static readonly string[] MonitorReadyPermissions =
    [
        Permissions.ManagementRatings.Read,
        Permissions.ManagementRatings.DetailsRead,
        Permissions.FollowerWork.Read,
        Permissions.FollowerWork.SalesReviewRead,
        Permissions.FollowerWork.DecliningRead,
        Permissions.FollowerWork.NotesRead,
        Permissions.FollowerWork.OldCustomersRead,
        Permissions.FollowerWork.DelegateNotesRead,
        Permissions.FollowerWork.CollectionRead,
        Permissions.FollowerWork.CollectionManageEnabled,
        Permissions.FollowerWork.LocationRead,
        Permissions.FollowerWork.RouteRead,
        Permissions.LegalFollowup.Read,
        Permissions.LegalFollowup.Create,
        Permissions.LegalFollowup.Cancel,
        Permissions.CommitteeTask.Read,
        Permissions.AppNote.Read,
        Permissions.Waiver.Read,
        Permissions.CustomerDecision.Read,
        Permissions.LegalConversion.Decide,
        Permissions.WrongSale.Read,
        Permissions.WrongSale.MarkBranchReviewed,
        Permissions.PriceBoard.Read,
        Permissions.PriceBoard.Create,
        Permissions.PriceBoard.Update,
        Permissions.PriceBoard.Delete,
        Permissions.SalesSummary.Read,
        Permissions.SalesSummary.Drilldown,
        Permissions.DelegateFollowup.Read,
        Permissions.SalesFollowup.Read,
        Permissions.SalesOverview.Read,
        Permissions.SalesEmployee.Read,
        Permissions.SalesLocation.Read,
        Permissions.SalesRoute.Read,
        Permissions.Order.Read,
        Permissions.OrderList.Read,
        Permissions.Customer.Read,
        Permissions.User.Read,
        Permissions.Notification.Read,
        Permissions.Notification.Update,
        Permissions.ActivityLog.Read,
        ..SharedSalesRequestTabs,
    ];

    private static readonly string[] SalesManagerReadyPermissions =
    [
        Permissions.SalesFollowup.Read,
        Permissions.SalesOverview.Read,
        Permissions.SalesEmployee.Read,
        Permissions.SalesLocation.Read,
        Permissions.SalesRoute.Read,
        Permissions.SalesSummary.Read,
        Permissions.SalesSummary.Drilldown,
        Permissions.LegalFollowup.Read,
        Permissions.LegalFollowup.Create,
        Permissions.LegalFollowup.Cancel,
        Permissions.CommitteeTask.Read,
        Permissions.CommitteeTask.Write,
        Permissions.AppNote.Read,
        Permissions.Order.Read,
        Permissions.Customer.Read,
        Permissions.User.Read,
        Permissions.Notification.Read,
        Permissions.Notification.Update,
        ..SharedSalesRequestTabs,
    ];
}
