using PermissionEntity = Domain.Entities.RoleAggregate.Permission;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Common.AdminPanel;

/// <summary>
/// كتالوج تبويبات لوحة الويب: Panel.Tab.{tabKey}.
/// الأب التنظيمي لا يمنح الأبناء. غير الجاهز غير قابل للتفعيل.
/// </summary>
public static class AdminPanelTabs
{
    public const string PermissionPrefix = "Panel.Tab.";
    public const string Scope = "PanelTab";
    public const string ScopeDisplayName = "تبويبات اللوحة";

    public static string PermissionName(string tabKey) => $"{PermissionPrefix}{tabKey}";

    public static bool IsPanelTabPermission(string? permissionName) =>
        string.IsNullOrWhiteSpace(permissionName) == false
        && permissionName.StartsWith(PermissionPrefix, StringComparison.Ordinal);

    public static string? TabKeyFromPermission(string permissionName) =>
        IsPanelTabPermission(permissionName)
            ? permissionName[PermissionPrefix.Length..]
            : null;

    public static IReadOnlyList<AdminPanelTabDefinition> All { get; } = BuildAll();

    public static IReadOnlyList<AdminPanelTabDefinition> ReadyTabs { get; } =
        All.Where(t => t.Ready).ToList();

    public static IReadOnlyDictionary<string, AdminPanelTabDefinition> ByKey { get; } =
        All.ToDictionary(t => t.TabKey, StringComparer.Ordinal);

    public static IReadOnlySet<string> ManagedActionNames { get; } =
        ReadyTabs.SelectMany(t => t.GrantedActions).ToHashSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> ReadyPermissionNames { get; } =
        ReadyTabs.Select(t => PermissionName(t.TabKey)).ToHashSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> NotReadyTabKeys { get; } =
        All.Where(t => t.Ready == false).Select(t => t.TabKey).ToHashSet(StringComparer.Ordinal);

    /// <summary>كيانات صلاحيات Panel.Tab.* للبذرة — خارج Permissions.All.</summary>
    public static PermissionEntity[] OutsideAll { get; } =
        All.Select(t => new PermissionEntity(
            PermissionName(t.TabKey),
            t.DisplayNameAr,
            Scope,
            ScopeDisplayName)).ToArray();

    public static bool TryGet(string tabKey, out AdminPanelTabDefinition tab) =>
        ByKey.TryGetValue(tabKey, out tab!);

    public static bool IsManagedPermissionName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        return IsPanelTabPermission(name) || ManagedActionNames.Contains(name);
    }

    /// <summary>
    /// يوسّع مجموعة الصلاحيات الفعلية بحزم إجراءات التبويبات المفتوحة الجاهزة.
    /// </summary>
    public static List<PermissionEntity> ExpandGrantedActions(
        IEnumerable<PermissionEntity> effective,
        IReadOnlyDictionary<string, PermissionEntity> permissionsByName)
    {
        var byKey = new Dictionary<string, PermissionEntity>(StringComparer.Ordinal);
        foreach (var p in effective)
        {
            if (p is null || string.IsNullOrWhiteSpace(p.Name))
                continue;
            byKey[p.Name] = p;
        }

        foreach (var tabPermName in byKey.Keys.ToList())
        {
            var tabKey = TabKeyFromPermission(tabPermName);
            if (tabKey is null || ByKey.TryGetValue(tabKey, out var tab) == false || tab.Ready == false)
                continue;

            foreach (var action in tab.GrantedActions)
            {
                if (byKey.ContainsKey(action))
                    continue;
                if (permissionsByName.TryGetValue(action, out var actionPerm))
                    byKey[action] = actionPerm;
                else
                    byKey[action] = new PermissionEntity(action, action, "Expanded", "Expanded");
            }
        }

        return byKey.Values.ToList();
    }

    private static AdminPanelTabDefinition Tab(
        string key,
        string? parent,
        string displayAr,
        bool ready,
        string[] actions,
        params AdminPanelEndpointBinding[] endpoints) =>
        new(key, parent, displayAr, ready, actions, endpoints);

    private static AdminPanelEndpointBinding Ep(
        string method,
        string path,
        string legacy,
        string scope,
        string hint) =>
        new(method, path, legacy, scope, hint);

    private static AdminPanelTabDefinition[] BuildAll()
    {
        var srCommon = new[]
        {
            SeedPermissions.SalesRequest.TimelineRead,
            SeedPermissions.SalesRequest.MatchRead,
            SeedPermissions.SalesRequest.ReadFinancial,
            SeedPermissions.SalesRequest.ReadAuditProfile,
            SeedPermissions.SalesRequest.ReadAuditDocuments,
            SeedPermissions.Customer.Read,
            SeedPermissions.Attachment.Read,
        };

        return
        [
            // —— أقسام عليا ——
            Tab("home", null, "الرئيسية", true, [], Ep("GET", "/api/v1/admin/Notifications/badge", SeedPermissions.Notification.Read, "session", "authenticated shell")),
            Tab("branch", null, "الفروع", true,
                [SeedPermissions.Branch.Read, SeedPermissions.Branch.Create, SeedPermissions.Branch.Update, SeedPermissions.Province.Read],
                Ep("GET", "/api/v1/admin/Branches", SeedPermissions.Branch.Read, "branch", "list")),
            Tab("payment", null, "التسديدات", true,
                [SeedPermissions.OrderList.Read, SeedPermissions.InstallmentPayment.Read],
                Ep("GET", "/api/v1/admin/OrderLists", SeedPermissions.OrderList.Read, "branch", "payment lists")),
            Tab("customer", null, "الزبائن", true,
                [SeedPermissions.Customer.Read, SeedPermissions.Customer.Create, SeedPermissions.Customer.Update, SeedPermissions.Attachment.Read],
                Ep("GET", "/api/v1/admin/Customers", SeedPermissions.Customer.Read, "branch", "customer list")),
            Tab("purchases", null, "المشتريات", true,
                [SeedPermissions.Purchase.Read, SeedPermissions.Purchase.Create, SeedPermissions.Purchase.Update, SeedPermissions.Attachment.Read],
                Ep("GET", "/api/v1/admin/Purchases", SeedPermissions.Purchase.Read, "branch", "purchases")),
            Tab("warehouse", null, "المخزن", true,
                [SeedPermissions.ProductCategory.Read, SeedPermissions.Product.Read, SeedPermissions.Warehouse.Read, SeedPermissions.Product.Create, SeedPermissions.Product.Update],
                Ep("GET", "/api/v1/admin/ProductCategories", SeedPermissions.ProductCategory.Read, "global", "warehouse")),
            Tab("archive", null, "الأرشيف", true,
                [
                    SeedPermissions.Archive.Read, SeedPermissions.Archive.ReceiveArchive,
                    SeedPermissions.Archive.Return, SeedPermissions.Archive.Oversight
                ],
                Ep("GET", "/api/v1/admin/Archive/bundles", SeedPermissions.Archive.Read, "branch", "حزم الأرشيف"),
                Ep("GET", "/api/v1/admin/Archive/flow-board", SeedPermissions.Archive.Oversight, "branch", "لوحة التدفّق"),
                Ep("POST", "/api/v1/admin/Archive/transactions/receive", SeedPermissions.Archive.ReceiveArchive, "branch", "استلام الأرشيف"),
                Ep("POST", "/api/v1/admin/Archive/transactions/return", SeedPermissions.Archive.Return, "branch", "استرجاع")),
            Tab("safe", null, "القاصة", true,
                [
                    SeedPermissions.Safe.Read, SeedPermissions.Safe.Create, SeedPermissions.Safe.Update,
                    SeedPermissions.Safe.Delete, SeedPermissions.Transaction.Read, SeedPermissions.Transaction.Create
                ],
                Ep("GET", "/api/v1/admin/Safes", SeedPermissions.Safe.Read, "branch", "خزائن الفرع"),
                Ep("GET", "/api/v1/admin/Safes/{id}/movements", SeedPermissions.Safe.Read, "branch", "سجل الحركات"),
                Ep("GET", "/api/v1/admin/Safes/{id}/verify", SeedPermissions.Safe.Read, "branch", "تحقق حسابي"),
                Ep("POST", "/api/v1/admin/Safes", SeedPermissions.Safe.Create, "branch", "إنشاء خزينة"),
                Ep("DELETE", "/api/v1/admin/Safes/{id}", SeedPermissions.Safe.Delete, "branch", "حذف خزينة"),
                Ep("POST", "/api/v1/admin/Safes/{id}/deposit", SeedPermissions.Transaction.Create, "branch", "إضافة"),
                Ep("POST", "/api/v1/admin/Safes/{id}/withdraw", SeedPermissions.Transaction.Create, "branch", "سحب"),
                Ep("POST", "/api/v1/admin/Safes/transfer", SeedPermissions.Transaction.Create, "branch", "نقل")),
            Tab("order", null, "القوائم", true,
                [SeedPermissions.OrderList.Read, SeedPermissions.OrderList.Create, SeedPermissions.OrderList.Update],
                Ep("GET", "/api/v1/admin/OrderLists", SeedPermissions.OrderList.Read, "branch", "order lists")),
            Tab("user", null, "المستخدمون", true,
                [SeedPermissions.User.Read, SeedPermissions.User.Create, SeedPermissions.User.Update, SeedPermissions.Role.Read],
                Ep("GET", "/api/v1/admin/Users", SeedPermissions.User.Read, "branch", "users — لا ManagePermissions")),
            Tab("activityLog", null, "سجل النشاط", true,
                [SeedPermissions.ActivityLog.Read],
                Ep("GET", "/api/v1/admin/ActivityLogs", SeedPermissions.ActivityLog.Read, "branch", "activity")),
            Tab("notifications", null, "الإشعارات", true,
                [SeedPermissions.Notification.Read, SeedPermissions.Notification.Update],
                Ep("GET", "/api/v1/admin/Notifications", SeedPermissions.Notification.Read, "user", "notifications")),

            // —— المبيعات ——
            Tab("saleOverview", "sale", "نظرة عامة للمبيعات", true,
                [SeedPermissions.SalesOverview.Read],
                Ep("GET", "/api/v1/admin/SalesOverview/summary", SeedPermissions.SalesOverview.Read, "branch", "overview")),
            Tab("saleEmployees", "sale", "موظفو المبيعات", true,
                [SeedPermissions.SalesEmployee.Read, SeedPermissions.User.Read],
                Ep("GET", "/api/v1/admin/SalesEmployees", SeedPermissions.SalesEmployee.Read, "branch", "employees")),
            Tab("saleFollowup", "sale", "متابعة المبيعات", true,
                [SeedPermissions.SalesFollowup.Read, SeedPermissions.SalesFollowup.ManageAppBlock, SeedPermissions.SalesFollowup.UpsertManagerRating],
                Ep("GET", "/api/v1/admin/SalesFollowup/board", SeedPermissions.SalesFollowup.Read, "branch", "sales followup")),
            Tab("saleLiveMap", "sale", "الخريطة المباشرة للمبيعات", true,
                [SeedPermissions.SalesLocation.Read],
                Ep("GET", "/api/v1/admin/SalesLocations/live", SeedPermissions.SalesLocation.Read, "branch", "sales map")),
            Tab("saleRoutes", "sale", "مسارات المبيعات", true,
                [SeedPermissions.SalesRoute.Read],
                Ep("GET", "/api/v1/admin/SalesRoutes", SeedPermissions.SalesRoute.Read, "branch", "sales routes")),
            Tab("saleOrders", "sale", "المبيعات المنجزة", true,
                [SeedPermissions.Order.Read],
                Ep("GET", "/api/v1/admin/Orders", SeedPermissions.Order.Read, "branch", "completed sales")),
            Tab("saleSummary", "sale", "ملخص المبيعات", true,
                [SeedPermissions.SalesSummary.Read, SeedPermissions.SalesSummary.Drilldown],
                Ep("GET", "/api/v1/admin/SalesSummary", SeedPermissions.SalesSummary.Read, "branch", "sales summary")),
            Tab("saleLegal", "sale", "المتابعة القانونية للمبيعات", true,
                [SeedPermissions.LegalFollowup.Read, SeedPermissions.LegalFollowup.Create, SeedPermissions.LegalFollowup.Cancel],
                Ep("GET", "/api/v1/admin/LegalFollowups", SeedPermissions.LegalFollowup.Read, "branch", "legal followups")),
            Tab("saleExcelSearch", "sale", "بحث Excel", true,
                [SeedPermissions.Customer.Read],
                Ep("GET", "/api/v1/admin/Customers/excel-search", SeedPermissions.Customer.Read, "branch", "excel search")),

            // —— حالات طلبات البيع (تبويبات مستقلة) ——
            Tab("sr.unassigned", "saleRequests", "طلبات غير مسندة", true,
                [SeedPermissions.SalesRequest.ReadUnassigned, SeedPermissions.SalesRequest.Assign, SeedPermissions.SalesRequest.TransferBranch, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=unassigned", SeedPermissions.SalesRequest.ReadUnassigned, "branch+status", "unassigned only")),
            Tab("sr.incoming", "saleRequests", "طلبات واردة", true,
                [SeedPermissions.SalesRequest.TabIncoming, SeedPermissions.SalesRequest.Assign, SeedPermissions.SalesRequest.Reject, SeedPermissions.SalesRequest.RequestException, SeedPermissions.SalesRequest.UpdateRequest, SeedPermissions.SalesRequest.SoftDelete, SeedPermissions.SalesRequest.CancelToUnassigned, SeedPermissions.SalesRequest.TransferBranch, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=incoming", SeedPermissions.SalesRequest.TabIncoming, "branch+status", "incoming")),
            Tab("sr.prepared", "saleRequests", "جاهز للبيع", true,
                [SeedPermissions.SalesRequest.TabPrepared, SeedPermissions.SalesRequest.SoftDelete, SeedPermissions.SalesRequest.ManagerPend, SeedPermissions.SalesRequest.CancelToUnassigned, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=prepared", SeedPermissions.SalesRequest.TabPrepared, "branch+status", "prepared")),
            Tab("sr.inspected", "saleRequests", "تم الكشف", true,
                [SeedPermissions.SalesRequest.TabInspected, SeedPermissions.SalesRequest.SoftDelete, SeedPermissions.SalesRequest.ManagerPrepare, SeedPermissions.SalesRequest.ManagerPend, SeedPermissions.SalesRequest.CancelToUnassigned, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=inspected", SeedPermissions.SalesRequest.TabInspected, "branch+status", "inspected")),
            Tab("sr.pending", "saleRequests", "معلقة", true,
                [SeedPermissions.SalesRequest.TabPending, SeedPermissions.SalesRequest.SoftDelete, SeedPermissions.SalesRequest.ManagerPrepare, SeedPermissions.SalesRequest.CancelToUnassigned, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=pending", SeedPermissions.SalesRequest.TabPending, "branch+status", "pending")),
            Tab("sr.rejected", "saleRequests", "مرفوضة", true,
                [SeedPermissions.SalesRequest.TabRejected, SeedPermissions.SalesRequest.ReturnRequest, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=rejected", SeedPermissions.SalesRequest.TabRejected, "branch+status", "rejected")),
            Tab("sr.sold", "saleRequests", "تم البيع", true,
                [SeedPermissions.SalesRequest.TabSold, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=sold", SeedPermissions.SalesRequest.TabSold, "branch+status", "sold")),
            Tab("sr.sent", "saleRequests", "مرسلة", true,
                [SeedPermissions.SalesRequest.TabSent, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=sent", SeedPermissions.SalesRequest.TabSent, "branch+status", "sent")),
            Tab("sr.exceptions", "saleRequests", "الاستثناءات", true,
                [SeedPermissions.SalesRequest.TabExceptions, SeedPermissions.SalesRequest.ExceptionDecide, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=exceptions", SeedPermissions.SalesRequest.TabExceptions, "branch+status", "exceptions — Decide محروس")),
            Tab("sr.deleted", "saleRequests", "محذوفة", true,
                [SeedPermissions.SalesRequest.TabDeleted, SeedPermissions.SalesRequest.Restore, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=deleted", SeedPermissions.SalesRequest.TabDeleted, "branch+status", "deleted")),
            Tab("sr.legal", "saleRequests", "قانونية الطلبات", true,
                [SeedPermissions.SalesRequest.TabLegal, SeedPermissions.SalesRequest.CancelToUnassigned, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=legal", SeedPermissions.SalesRequest.TabLegal, "branch+status", "legal tab")),

            // —— لوحة الإدارة ——
            Tab("managementSummary", "management", "الملخص الإداري", true,
                [SeedPermissions.ManagementSummary.Read, SeedPermissions.ManagementSummary.Drilldown],
                Ep("GET", "/api/v1/admin/ManagementSummary", SeedPermissions.ManagementSummary.Read, "branch", "mgmt summary")),
            Tab("managementRatings", "management", "تقييمات الإدارة", true,
                [SeedPermissions.ManagementRatings.Read, SeedPermissions.ManagementRatings.DetailsRead],
                Ep("GET", "/api/v1/admin/ManagementRatings", SeedPermissions.ManagementRatings.Read, "branch", "mgmt ratings")),
            Tab("managementDelegateFollowup", null, "متابعة المندوبين", true,
                [SeedPermissions.DelegateFollowup.Read, SeedPermissions.DelegateFollowup.UpsertBoardRating],
                Ep("GET", "/api/v1/admin/DelegateFollowup", SeedPermissions.DelegateFollowup.Read, "branch", "delegate followup")),
            Tab("managementSalesMap", "management", "خريطة مبيعات الإدارة", true,
                [SeedPermissions.ManagementSalesMap.Read],
                Ep("GET", "/api/v1/admin/ManagementSalesMap", SeedPermissions.ManagementSalesMap.Read, "branch", "mgmt sales map")),
            Tab("managementCommitteeTasks", "management", "مهام اللجنة / الإدارة", true,
                [SeedPermissions.ManagementTask.Read, SeedPermissions.ManagementTask.Create, SeedPermissions.ManagementTask.Assign, SeedPermissions.ManagementTask.Accept, SeedPermissions.ManagementTask.Reject, SeedPermissions.ManagementTask.Complete, SeedPermissions.ManagementTask.Confirm, SeedPermissions.ManagementTask.Transfer, SeedPermissions.ManagementTask.ReadAttachment, SeedPermissions.CommitteeTask.Read, SeedPermissions.CommitteeTask.Write],
                Ep("GET", "/api/v1/admin/ManagementTasks", SeedPermissions.ManagementTask.Read, "branch", "tasks")),
            Tab("managementAppNotes", "management", "ملاحظات التطبيق", true,
                [SeedPermissions.AppNote.Read],
                Ep("GET", "/api/v1/admin/AppNotes", SeedPermissions.AppNote.Read, "branch", "app notes")),
            Tab("managementWaivers", "management", "التنازلات", true,
                [SeedPermissions.Waiver.Read, SeedPermissions.Waiver.Decide, SeedPermissions.Waiver.Apply],
                Ep("GET", "/api/v1/admin/Waivers", SeedPermissions.Waiver.Read, "branch", "waivers")),
            Tab("managementDeletedSales", "management", "حذوفات المبيعات", true,
                [SeedPermissions.SalesRequest.DeletedSalesRead],
                Ep("GET", "/api/v1/admin/SalesRequests/deleted-sales", SeedPermissions.SalesRequest.DeletedSalesRead, "branch", "deleted sold sales — admin")),
            Tab("managementLegalNames", "management", "الأسماء القانونية", true,
                [SeedPermissions.LegalNames.Read],
                Ep("GET", "/api/v1/admin/LegalNames", SeedPermissions.LegalNames.Read, "branch", "legal names")),
            Tab("managementPriceBoard", "management", "لوحة الأسعار", true,
                [SeedPermissions.PriceBoard.Read, SeedPermissions.PriceBoard.Create, SeedPermissions.PriceBoard.Update, SeedPermissions.PriceBoard.Delete],
                Ep("GET", "/api/v1/admin/PriceBoard", SeedPermissions.PriceBoard.Read, "global", "price board")),
            Tab("managementAnnouncements", "management", "الإعلانات", true,
                [SeedPermissions.Announcement.Read, SeedPermissions.Announcement.Create, SeedPermissions.Announcement.Update, SeedPermissions.Announcement.Delete],
                Ep("GET", "/api/v1/admin/Announcements", SeedPermissions.Announcement.Read, "global", "announcements")),
            Tab("managementTutorials", "management", "الفيديوهات التعليمية", true,
                [SeedPermissions.Tutorial.Read, SeedPermissions.Tutorial.Create, SeedPermissions.Tutorial.Update, SeedPermissions.Tutorial.Delete],
                Ep("GET", "/api/v1/admin/Tutorials", SeedPermissions.Tutorial.Read, "global", "tutorials")),

            // —— عمل المتابعين (10) ——
            Tab("followerSales", "managementFollowerWork", "مراجعة مبيعات المتابع", true,
                [SeedPermissions.FollowerWork.SalesReviewRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerWork/{followerId}/sales-review", SeedPermissions.FollowerWork.SalesReviewRead, "branch", "follower sales")),
            Tab("followerDeclining", "managementFollowerWork", "المتراجعون — متابع", true,
                [SeedPermissions.FollowerWork.DecliningRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerWork/{followerId}/declining", SeedPermissions.FollowerWork.DecliningRead, "branch", "follower declining")),
            Tab("followerNotes", "managementFollowerWork", "ملاحظات المتابع", true,
                [SeedPermissions.FollowerWork.NotesRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerWork/{followerId}/notes", SeedPermissions.FollowerWork.NotesRead, "branch", "follower notes")),
            Tab("followerOldCustomers", "managementFollowerWork", "زبائن قدماء", true,
                [SeedPermissions.FollowerWork.OldCustomersRead, SeedPermissions.FollowerWork.Read, SeedPermissions.LegalFollowup.Cancel],
                Ep("GET", "/api/v1/admin/FollowerWork/{followerId}/old-customers", SeedPermissions.FollowerWork.OldCustomersRead, "branch", "old customers")),
            Tab("followerDelegateNotes", "managementFollowerWork", "ملاحظات المندوب للمتابع", true,
                [SeedPermissions.FollowerWork.DelegateNotesRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerWork/{followerId}/delegate-notes", SeedPermissions.FollowerWork.DelegateNotesRead, "branch", "delegate notes")),
            Tab("followerCollection", "managementFollowerWork", "جباية المتابع", true,
                [SeedPermissions.FollowerWork.CollectionRead, SeedPermissions.FollowerWork.CollectionManageEnabled, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerWork/{followerId}/collection", SeedPermissions.FollowerWork.CollectionRead, "branch", "collection")),
            Tab("followerLiveMap", "managementFollowerWork", "خريطة المتابعين", true,
                [SeedPermissions.FollowerWork.LocationRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerTracking/latest", SeedPermissions.FollowerWork.LocationRead, "branch", "follower map")),
            Tab("followerRoutes", "managementFollowerWork", "مسارات المتابعين", true,
                [SeedPermissions.FollowerWork.RouteRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerTracking/{followerId}/route", SeedPermissions.FollowerWork.RouteRead, "branch", "follower routes")),
            Tab("followerShopCapture", "managementFollowerWork", "مواقع وصور المحلات", true,
                [SeedPermissions.FollowerWork.LocationRead, SeedPermissions.FollowerWork.Read],
                Ep("GET", "/api/v1/admin/FollowerWork/shop-capture", SeedPermissions.FollowerWork.LocationRead, "branch", "shop capture")),
            Tab("followerManage", "managementFollowerWork", "إدارة حسابات المتابعين", true,
                [SeedPermissions.FollowerWork.ManageFollowers, SeedPermissions.FollowerWork.Read],
                Ep("POST", "/api/v1/admin/FollowerWork/followers", SeedPermissions.FollowerWork.ManageFollowers, "branch", "manage followers")),

            // —— تطبيق الفرع ——
            Tab("app-branch-branchAssignedTasks", "managementBranch", "مهام مسندة — فرع", true,
                [SeedPermissions.ManagementTask.Read, SeedPermissions.ManagementTask.Accept, SeedPermissions.ManagementTask.Reject, SeedPermissions.ManagementTask.Complete],
                Ep("GET", "/api/v1/admin/ManagementTasks?scope=branch", SeedPermissions.ManagementTask.Read, "branch", "branch tasks")),
            Tab("app-branch-archive", "managementBranch", "الأرشيف — فرع", true,
                [SeedPermissions.Archive.Read, SeedPermissions.Archive.ReceiveBranch, SeedPermissions.Archive.Send],
                Ep("GET", "/api/v1/admin/Archive/bundles?stage=BranchManagement", SeedPermissions.Archive.Read, "branch", "وارد الفرع"),
                Ep("POST", "/api/v1/admin/Archive/transactions/receive", SeedPermissions.Archive.ReceiveBranch, "branch", "استلام الفرع")),
            Tab("app-branch-legalNames", "managementBranch", "أسماء قانونية — فرع", true,
                [SeedPermissions.LegalNames.Read],
                Ep("GET", "/api/v1/admin/LegalNames", SeedPermissions.LegalNames.Read, "branch", "branch legal names")),
            Tab("app-branch-branchDeclining", "managementBranch", "المتراجعون — فرع", true,
                [SeedPermissions.BranchDeclining.Read],
                Ep("GET", "/api/v1/admin/BranchDeclining", SeedPermissions.BranchDeclining.Read, "branch", "branch declining")),
            Tab("app-branch-customers", "managementBranch", "زبائن الفرع", true,
                [SeedPermissions.Customer.Read],
                Ep("GET", "/api/v1/admin/Customers?scope=branch", SeedPermissions.Customer.Read, "branch", "BM search")),
            Tab("app-branch-sales", "managementBranch", "مبيعات جديدة — فرع", true,
                [SeedPermissions.BranchNewSale.Read, SeedPermissions.BranchNewSale.MarkRead],
                Ep("GET", "/api/v1/admin/BranchNewSales", SeedPermissions.BranchNewSale.Read, "branch", "new sales")),
            Tab("app-branch-wrongSalesBranch", "managementBranch", "مبيعات خاطئة — فرع", true,
                [SeedPermissions.WrongSale.Read, SeedPermissions.WrongSale.MarkBranchReviewed],
                Ep("GET", "/api/v1/admin/WrongSales?mailbox=unreviewed", SeedPermissions.WrongSale.Read, "branch", "wrong sales branch")),
            Tab("app-branch-branchRatings", "managementBranch", "تقييمات الفرع", true,
                [SeedPermissions.ManagementRatings.Read, SeedPermissions.ManagementRatings.DetailsRead],
                Ep("GET", "/api/v1/admin/ManagementRatings?scope=branch", SeedPermissions.ManagementRatings.Read, "branch", "branch ratings")),
            Tab("app-branch-payments", "managementBranch", "تسديدات الفرع", true,
                [SeedPermissions.OrderList.Read, SeedPermissions.InstallmentPayment.Read],
                Ep("GET", "/api/v1/admin/OrderLists?scope=branch", SeedPermissions.OrderList.Read, "branch", "branch payments")),
            Tab("app-branch-branchWaivers", "managementBranch", "تنازلات الفرع", true,
                [SeedPermissions.Waiver.Read],
                Ep("GET", "/api/v1/admin/Waivers?scope=branch", SeedPermissions.Waiver.Read, "branch", "branch waivers")),
            Tab("app-branch-legalDecisions", "managementBranch", "قرارات قانونية", true,
                [SeedPermissions.CustomerDecision.Read, SeedPermissions.LegalConversion.Decide],
                Ep("GET", "/api/v1/admin/CustomerDecisions", SeedPermissions.CustomerDecision.Read, "branch", "decisions")),
            Tab("branchVisitWorkspace", "managementBranch", "مساحة زيارة الفرع", true,
                [SeedPermissions.BranchVisit.Read, SeedPermissions.BranchVisit.Open, SeedPermissions.BranchVisit.Rate, SeedPermissions.BranchVisit.Depart],
                Ep("GET", "/api/v1/admin/BranchVisits", SeedPermissions.BranchVisit.Read, "branch", "visit workspace")),

            // —— تطبيق المفوض ——
            Tab("app-delegate-delegateHome", "managementDelegate", "رئيسية المفوض", true,
                [SeedPermissions.DelegateHome.Read],
                Ep("GET", "/api/v1/admin/DelegateHome/summary", SeedPermissions.DelegateHome.Read, "branch", "delegate home")),
            Tab("app-delegate-assignedTasks", "managementDelegate", "مهام مسندة — مفوض", true,
                [SeedPermissions.ManagementTask.Read, SeedPermissions.ManagementTask.Accept, SeedPermissions.ManagementTask.Complete],
                Ep("GET", "/api/v1/admin/ManagementTasks?scope=delegate", SeedPermissions.ManagementTask.Read, "branch", "delegate tasks")),
            Tab("app-delegate-delegateRatings", "managementDelegate", "تقييمات المفوض", true,
                [SeedPermissions.ManagementRatings.Read, SeedPermissions.ManagementRatings.DetailsRead],
                Ep("GET", "/api/v1/admin/ManagementRatings?scope=delegate", SeedPermissions.ManagementRatings.Read, "branch", "delegate ratings")),
            Tab("app-delegate-complaints", "managementDelegate", "الشكاوى", true,
                [SeedPermissions.Complaint.Read, SeedPermissions.Complaint.MarkRead],
                Ep("GET", "/api/v1/admin/Complaints", SeedPermissions.Complaint.Read, "branch", "complaints")),
            Tab("app-delegate-exceptions", "managementDelegate", "استثناءات المفوض", true,
                [SeedPermissions.SalesRequest.TabExceptions, SeedPermissions.SalesRequest.ExceptionDecide, ..srCommon],
                Ep("GET", "/api/v1/admin/SalesRequests?tab=exceptions", SeedPermissions.SalesRequest.TabExceptions, "branch", "delegate exceptions")),
            Tab("app-delegate-wrongSalesDelegate", "managementDelegate", "مبيعات خاطئة — مفوض", true,
                [SeedPermissions.WrongSale.Read, SeedPermissions.WrongSale.MarkDelegateRead],
                Ep("GET", "/api/v1/admin/WrongSales?mailbox=unread", SeedPermissions.WrongSale.Read, "branch", "wrong sales delegate")),
            Tab("app-delegate-delegateWaivers", "managementDelegate", "تنازلات المفوض", true,
                [SeedPermissions.Waiver.Read, SeedPermissions.Waiver.Decide],
                Ep("GET", "/api/v1/admin/Waivers?scope=delegate", SeedPermissions.Waiver.Read, "branch", "delegate waivers")),
            Tab("app-delegate-accounts", "managementDelegate", "حسابات مديري الفرع", true,
                [SeedPermissions.BranchManagerAccount.Read, SeedPermissions.BranchManagerAccount.Write],
                Ep("GET", "/api/v1/admin/BranchManagerAccounts", SeedPermissions.BranchManagerAccount.Read, "branch", "BM accounts")),
            Tab("app-delegate-branchVisits", "managementDelegate", "زيارات الفرع — مفوض", true,
                [SeedPermissions.BranchVisit.Read, SeedPermissions.BranchVisit.Open, SeedPermissions.BranchVisit.Rate, SeedPermissions.BranchVisit.Depart],
                Ep("GET", "/api/v1/admin/BranchVisits", SeedPermissions.BranchVisit.Read, "branch", "branch visits")),
            Tab("app-delegate-delegateSettings", "managementDelegate", "إعدادات المفوض", true,
                [],
                Ep("GET", "/api/v1/admin/Users/profile", "publicAuthenticated", "user", "settings")),

            Tab("fpHome", "managementFieldPromoter", "رئيسية المروّجين", true,
                [SeedPermissions.FieldPromoter.Read],
                Ep("GET", "/api/v1/admin/field-promoters/live", SeedPermissions.FieldPromoter.Read, "branch", "promoters home")),
            Tab("fpLiveMap", "managementFieldPromoter", "الخريطة الحية للمروّجين", true,
                [SeedPermissions.FieldPromoter.Read],
                Ep("GET", "/api/v1/admin/field-promoters/live", SeedPermissions.FieldPromoter.Read, "branch", "promoters live map")),
            Tab("fpDay", "managementFieldPromoter", "يوم مروّج", true,
                [SeedPermissions.FieldPromoter.Read],
                Ep("GET", "/api/v1/admin/field-promoters/{userId}/day", SeedPermissions.FieldPromoter.Read, "branch", "promoter day")),
            Tab("fpAttendance", "managementFieldPromoter", "حضور وأهداف المروّجين", true,
                [SeedPermissions.FieldPromoter.Read],
                Ep("GET", "/api/v1/admin/field-promoters/attendance", SeedPermissions.FieldPromoter.Read, "branch", "promoter attendance"),
                Ep("GET", "/api/v1/admin/field-promoters/attendance/export", SeedPermissions.FieldPromoter.Read, "branch", "attendance export")),
            Tab("fpPins", "managementFieldPromoter", "محلات المروّجين", true,
                [SeedPermissions.FieldPromoter.Read],
                Ep("GET", "/api/v1/admin/field-promoters/pins", SeedPermissions.FieldPromoter.Read, "branch", "promoter pins")),
            Tab("fpOrders", "managementFieldPromoter", "طلبات المروّجين", true,
                [SeedPermissions.FieldPromoter.Read],
                Ep("GET", "/api/v1/admin/field-promoters/orders", SeedPermissions.FieldPromoter.Read, "branch", "promoter orders")),
            Tab("fpAlerts", "managementFieldPromoter", "تنبيهات وقوف المروّجين", true,
                [SeedPermissions.FieldPromoter.Read, SeedPermissions.FieldPromoter.Alerts],
                Ep("GET", "/api/v1/admin/field-promoters/alerts", SeedPermissions.FieldPromoter.Read, "branch", "promoter alerts")),
            Tab("fpPromoters", "managementFieldPromoter", "حسابات المروّجين", true,
                [SeedPermissions.FieldPromoter.Read, SeedPermissions.FieldPromoter.Manage],
                Ep("GET", "/api/v1/admin/field-promoters/promoters", SeedPermissions.FieldPromoter.Read, "branch", "promoter accounts"),
                Ep("POST", "/api/v1/admin/field-promoters/promoters", SeedPermissions.FieldPromoter.Manage, "branch", "manage promoters")),
            Tab("fpSettings", "managementFieldPromoter", "إعدادات المروّجين", true,
                [SeedPermissions.FieldPromoter.Read, SeedPermissions.FieldPromoter.Manage],
                Ep("GET", "/api/v1/admin/field-promoters/settings", SeedPermissions.FieldPromoter.Read, "global", "promoter settings"),
                Ep("PUT", "/api/v1/admin/field-promoters/settings", SeedPermissions.FieldPromoter.Manage, "global", "save promoter settings")),

            // —— غير جاهز ——
            Tab("committeeRatings", "managementDelegate", "تقييمات اللجنة", false, [],
                Ep("—", "—", "—", "—", "not_implemented — لا يُفعَّل")),
        ];
    }
}
