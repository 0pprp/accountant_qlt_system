using PermissionEntity = Domain.Entities.RoleAggregate.Permission;

namespace Application.Common.SeedData;

public static class Permissions
{
    public static readonly List<PermissionEntity> All = [];

    static Permissions()
    {
        All.AddRange(Attachment.All);
        All.AddRange(Branch.All);
        All.AddRange(Province.All);
        All.AddRange(InstallmentPayment.All);
        All.AddRange(Order.All);
        All.AddRange(OrderList.All);
        All.AddRange(Product.All);
        All.AddRange(ProductCategory.All);
        All.AddRange(Role.All);
        All.AddRange(User.All);
        All.AddRange(Warehouse.All);
        All.AddRange(Permission.All);
        All.AddRange(Customer.All);
        All.AddRange(Purchase.All);
        All.AddRange(Expense.All);
        All.AddRange(Safe.All);
        All.AddRange(Transaction.All);
        All.AddRange(Report.All);
        All.AddRange(Notification.All);
        All.AddRange(ActivityLog.All);
    }

    private const string CreateDisplayName = "أضافة";
    private const string ReadDisplayName = "رؤية";
    private const string UpdateDisplayName = "تعديل";
    private const string DeleteDisplayName = "حذف";

    public static class Attachment
    {
        private const string AttachmentDisplayName = "المرفقات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Attachment), AttachmentDisplayName),
            new(Read, ReadDisplayName, nameof(Attachment), AttachmentDisplayName),
            new(Update, UpdateDisplayName, nameof(Attachment), AttachmentDisplayName),
            new(Delete, DeleteDisplayName, nameof(Attachment), AttachmentDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.AttachmentAggregate.Attachment)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.AttachmentAggregate.Attachment)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.AttachmentAggregate.Attachment)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.AttachmentAggregate.Attachment)}.{nameof(Delete)}";
    }

    public static class Branch
    {
        private const string BranchDisplayName = "الفروع";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Branch), BranchDisplayName),
            new(Read, ReadDisplayName, nameof(Branch), BranchDisplayName),
            new(Update, UpdateDisplayName, nameof(Branch), BranchDisplayName),
            new(Delete, DeleteDisplayName, nameof(Branch), BranchDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.BranchAggregate.Branch)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.BranchAggregate.Branch)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.BranchAggregate.Branch)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.BranchAggregate.Branch)}.{nameof(Delete)}";
    }

    public static class Province
    {
        private const string ProvinceDisplayName = "المحافظات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Province), ProvinceDisplayName),
            new(Read, ReadDisplayName, nameof(Province), ProvinceDisplayName),
            new(Update, UpdateDisplayName, nameof(Province), ProvinceDisplayName),
            new(Delete, DeleteDisplayName, nameof(Province), ProvinceDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.ProvinceAggregate.Province)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.ProvinceAggregate.Province)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.ProvinceAggregate.Province)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.ProvinceAggregate.Province)}.{nameof(Delete)}";
    }

    public static class InstallmentPayment
    {
        private const string InstallmentPaymentDisplayName = "التسديدات";
        private const string CollectDisplayName = "تسدید";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(InstallmentPayment), InstallmentPaymentDisplayName),
            new(Read, ReadDisplayName, nameof(InstallmentPayment), InstallmentPaymentDisplayName),
            new(Update, UpdateDisplayName, nameof(InstallmentPayment), InstallmentPaymentDisplayName),
            new(Delete, DeleteDisplayName, nameof(InstallmentPayment), InstallmentPaymentDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.InstallmentPaymentAggregate.InstallmentPayment)}.{nameof(Delete)}";
    }

    public static class Order
    {
        private const string OrderDisplayName = "المبيعات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Order), OrderDisplayName),
            new(Read, ReadDisplayName, nameof(Order), OrderDisplayName),
            new(Update, UpdateDisplayName, nameof(Order), OrderDisplayName),
            new(Delete, DeleteDisplayName, nameof(Order), OrderDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.OrderAggregate.Order)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.OrderAggregate.Order)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.OrderAggregate.Order)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.OrderAggregate.Order)}.{nameof(Delete)}";
    }

    public static class OrderList
    {
        private const string OrderListDisplayName = "القوائم";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(OrderList), OrderListDisplayName),
            new(Read, ReadDisplayName, nameof(OrderList), OrderListDisplayName),
            new(Update, UpdateDisplayName, nameof(OrderList), OrderListDisplayName),
            new(Delete, DeleteDisplayName, nameof(OrderList), OrderListDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.OrderListAggregate.OrderList)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.OrderListAggregate.OrderList)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.OrderListAggregate.OrderList)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.OrderListAggregate.OrderList)}.{nameof(Delete)}";
    }

    public static class Product
    {
        private const string ProductDisplayName = "منتجات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Product), ProductDisplayName),
            new(Read, ReadDisplayName, nameof(Product), ProductDisplayName),
            new(Update, UpdateDisplayName, nameof(Product), ProductDisplayName),
            new(Delete, DeleteDisplayName, nameof(Product), ProductDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.ProductAggregate.Product)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.ProductAggregate.Product)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.ProductAggregate.Product)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.ProductAggregate.Product)}.{nameof(Delete)}";
    }

    public static class ProductCategory
    {
        private const string ProductCategoryDisplayName = "فئات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(ProductCategory), ProductCategoryDisplayName),
            new(Read, ReadDisplayName, nameof(ProductCategory), ProductCategoryDisplayName),
            new(Update, UpdateDisplayName, nameof(ProductCategory), ProductCategoryDisplayName),
            new(Delete, DeleteDisplayName, nameof(ProductCategory), ProductCategoryDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.ProductAggregate.ProductCategory)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.ProductAggregate.ProductCategory)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.ProductAggregate.ProductCategory)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.ProductAggregate.ProductCategory)}.{nameof(Delete)}";
    }

    public static class Role
    {
        private const string RoleDisplayName = "دور";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Role), RoleDisplayName),
            new(Read, ReadDisplayName, nameof(Role), RoleDisplayName),
            new(Update, UpdateDisplayName, nameof(Role), RoleDisplayName),
            new(Delete, DeleteDisplayName, nameof(Role), RoleDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.RoleAggregate.Role)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.RoleAggregate.Role)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.RoleAggregate.Role)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.RoleAggregate.Role)}.{nameof(Delete)}";
    }

    public static class User
    {
        private const string UserDisplayName = "المستخدمین";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(User), UserDisplayName),
            new(Read, ReadDisplayName, nameof(User), UserDisplayName),
            new(Update, UpdateDisplayName, nameof(User), UserDisplayName),
            new(Delete, DeleteDisplayName, nameof(User), UserDisplayName)
        ];

        /// <summary>
        /// خارج Permissions.All عمدًا حتى لا تُمنح للمحاسبين عبر RolePermissions.All.
        /// تُمنح لدور Admin فقط بشكل صريح.
        /// </summary>
        public static readonly PermissionEntity ManagePermissionsPermission =
            new(ManagePermissions, "إدارة تخصيص الصلاحيات", nameof(User), UserDisplayName);

        public const string Create = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Delete)}";
        public const string ManagePermissions =
            $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(ManagePermissions)}";
    }

    public static class Warehouse
    {
        private const string WarehouseDisplayName = "المخزن";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Warehouse), WarehouseDisplayName),
            new(Read, ReadDisplayName, nameof(Warehouse), WarehouseDisplayName),
            new(Update, UpdateDisplayName, nameof(Warehouse), WarehouseDisplayName),
            new(Delete, DeleteDisplayName, nameof(Warehouse), WarehouseDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.WarehouseAggregate.Warehouse)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.WarehouseAggregate.Warehouse)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.WarehouseAggregate.Warehouse)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.WarehouseAggregate.Warehouse)}.{nameof(Delete)}";
    }

    public static class Permission
    {
        private const string PermissionDisplayName = "الصلاحيات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Permission), PermissionDisplayName),
            new(Read, ReadDisplayName, nameof(Permission), PermissionDisplayName),
            new(Update, UpdateDisplayName, nameof(Permission), PermissionDisplayName),
            new(Delete, DeleteDisplayName, nameof(Permission), PermissionDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.RoleAggregate.Permission)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.RoleAggregate.Permission)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.RoleAggregate.Permission)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.RoleAggregate.Permission)}.{nameof(Delete)}";
    }

    public static class Customer
    {
        private const string CustomerDisplayName = "العملاء";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Customer), CustomerDisplayName),
            new(Read, ReadDisplayName, nameof(Customer), CustomerDisplayName),
            new(Update, UpdateDisplayName, nameof(Customer), CustomerDisplayName),
            new(Delete, DeleteDisplayName, nameof(Customer), CustomerDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.CustomerAggregate.Customer)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.CustomerAggregate.Customer)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.CustomerAggregate.Customer)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.CustomerAggregate.Customer)}.{nameof(Delete)}";
    }

    public static class Purchase
    {
        private const string PurchaseDisplayName = "المشتريات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Purchase), PurchaseDisplayName),
            new(Read, ReadDisplayName, nameof(Purchase), PurchaseDisplayName),
            new(Update, UpdateDisplayName, nameof(Purchase), PurchaseDisplayName),
            new(Delete, DeleteDisplayName, nameof(Purchase), PurchaseDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.PurchaseAggregate.Purchase)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.PurchaseAggregate.Purchase)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.PurchaseAggregate.Purchase)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.PurchaseAggregate.Purchase)}.{nameof(Delete)}";
    }
    
    public static class Expense
    {
        private const string ExpenseDisplayName = "الصرفيات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Expense), ExpenseDisplayName),
            new(Read, ReadDisplayName, nameof(Expense), ExpenseDisplayName),
            new(Update, UpdateDisplayName, nameof(Expense), ExpenseDisplayName),
            new(Delete, DeleteDisplayName, nameof(Expense), ExpenseDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.ExpenseAggregate.Expense)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.ExpenseAggregate.Expense)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.ExpenseAggregate.Expense)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.ExpenseAggregate.Expense)}.{nameof(Delete)}";
    }
    
    public static class Safe
    {
        private const string SafeDisplayName = "القاصه";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Safe), SafeDisplayName),
            new(Read, ReadDisplayName, nameof(Safe), SafeDisplayName),
            new(Update, UpdateDisplayName, nameof(Safe), SafeDisplayName),
            new(Delete, DeleteDisplayName, nameof(Safe), SafeDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.SafeAggregate.Safe)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.SafeAggregate.Safe)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.SafeAggregate.Safe)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.SafeAggregate.Safe)}.{nameof(Delete)}";
    }
    
    public static class Transaction
    {
        private const string TransactionDisplayName = "عملية";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Transaction), TransactionDisplayName),
            new(Read, ReadDisplayName, nameof(Transaction), TransactionDisplayName),
            new(Update, UpdateDisplayName, nameof(Transaction), TransactionDisplayName),
            new(Delete, DeleteDisplayName, nameof(Transaction), TransactionDisplayName),
            new(CollectCash, "تسدید", nameof(Transaction), TransactionDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.SafeAggregate.Transaction)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.SafeAggregate.Transaction)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.SafeAggregate.Transaction)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.SafeAggregate.Transaction)}.{nameof(Delete)}";
        public const string CollectCash = $"{nameof(Domain.Entities.SafeAggregate.Transaction)}.{nameof(CollectCash)}";
    }

    public static class Report
    {
        private const string ReportDisplayName = "التقارير";

        public static readonly PermissionEntity[] All =
        [
            new(Read, ReadDisplayName, nameof(Report), ReportDisplayName)
        ];

        public const string Read = $"{nameof(Report)}.{nameof(Read)}";
    }

    public static class Notification
    {
        private const string NotificationDisplayName = "الإشعارات";

        public static readonly PermissionEntity[] All =
        [
            new(Create, CreateDisplayName, nameof(Notification), NotificationDisplayName),
            new(Read, ReadDisplayName, nameof(Notification), NotificationDisplayName),
            new(Update, UpdateDisplayName, nameof(Notification), NotificationDisplayName),
            new(Delete, DeleteDisplayName, nameof(Notification), NotificationDisplayName)
        ];

        public const string Create = $"{nameof(Domain.Entities.NotificationAggregate.Notification)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.NotificationAggregate.Notification)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.NotificationAggregate.Notification)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.NotificationAggregate.Notification)}.{nameof(Delete)}";
    }

    public static class ActivityLog
    {
        private const string ActivityLogDisplayName = "سجل النشاط";

        public static readonly PermissionEntity[] All =
        [
            new(Read, ReadDisplayName, nameof(ActivityLog), ActivityLogDisplayName)
        ];

        public const string Read = $"{nameof(ActivityLog)}.{nameof(Read)}";
    }

    /// <summary>
    /// صلاحيات تدقيق طلبات البيع — خارج Permissions.All عمدًا
    /// حتى لا تُمنح عبر RolePermissions.All للمحاسبين.
    /// تُمنح لدور Admin فقط بشكل صريح في RolePermissions.
    /// </summary>
    public static class SalesRequest
    {
        public const string SubmitEmployee = "SalesRequest.SubmitEmployee";
        public const string SubmitDelegate = "SalesRequest.SubmitDelegate";
        public const string SubmitFollower = "SalesRequest.SubmitFollower";
        private const string SalesRequestDisplayName = "طلبات البيع";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(SubmitEmployee, "إرسال طلب من موظف المبيعات", nameof(SalesRequest), SalesRequestDisplayName),
            new(SubmitDelegate, "إرسال طلب من المندوب", nameof(SalesRequest), SalesRequestDisplayName),
            new(SubmitFollower, "إرسال طلب من مسؤول اللجنة", nameof(SalesRequest), SalesRequestDisplayName),
            new(ReadUnassigned, "قراءة غير المسند", nameof(SalesRequest), SalesRequestDisplayName),
            new(MatchRead, "قراءة نتائج المطابقة", nameof(SalesRequest), SalesRequestDisplayName),
            new(Assign, "إسناد الطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(Reject, "رفض الطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(RequestException, "طلب استثناء", nameof(SalesRequest), SalesRequestDisplayName),
            new(ReadFinancial, "قراءة التفاصيل المالية للمطابقة", nameof(SalesRequest), SalesRequestDisplayName),
            new(ReadAuditProfile, "بروفايل تدقيق الزبون", nameof(SalesRequest), SalesRequestDisplayName),
            new(ReadAuditDocuments, "مستندات بروفايل التدقيق", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppRead, "قراءة طلبات الموظف", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppView, "تسجيل اطلاع الطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppPrepare, "تجهيز الطلب للبيع", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppPend, "تعليق الطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppReject, "رفض الطلب من الموظف", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppInspect, "كشف الطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppLinkDraft, "ربط مسودة البيع بالطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(AppTransferName, "نقل الاسم لزميل (تطبيق الموظف)", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabIncoming, "تبويب طلبات البيع", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabPrepared, "تبويب جاهز للبيع", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabInspected, "تبويب تم الكشف", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabPending, "تبويب المعلقة", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabRejected, "تبويب المرفوض", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabSold, "تبويب تم البيع", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabSent, "تبويب الطلبات المرسلة", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabExceptions, "تبويب الاستثناءات", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabDeleted, "تبويب المحذوفة", nameof(SalesRequest), SalesRequestDisplayName),
            new(TabLegal, "تبويب طلبات القانونية", nameof(SalesRequest), SalesRequestDisplayName),
            new(ExceptionDecide, "قرار الاستثناء (موافقة/رفض)", nameof(SalesRequest), SalesRequestDisplayName),
            new(SoftDelete, "حذف ناعم للطلب", nameof(SalesRequest), SalesRequestDisplayName),
            new(Restore, "استعادة طلب محذوف", nameof(SalesRequest), SalesRequestDisplayName),
            new(TimelineRead, "قراءة سجل حركة الطلب", nameof(SalesRequest), SalesRequestDisplayName),
        ];

        public const string ReadUnassigned =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(ReadUnassigned)}";
        public const string MatchRead =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(MatchRead)}";
        public const string Assign =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(Assign)}";
        public const string Reject =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(Reject)}";
        public const string RequestException =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(RequestException)}";
        public const string ReadFinancial =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(ReadFinancial)}";
        public const string ReadAuditProfile =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(ReadAuditProfile)}";
        public const string ReadAuditDocuments =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(ReadAuditDocuments)}";
        public const string AppRead =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppRead)}";
        public const string AppView =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppView)}";
        public const string AppPrepare =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppPrepare)}";
        public const string AppPend =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppPend)}";
        public const string AppReject =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppReject)}";
        public const string AppInspect =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppInspect)}";
        public const string AppLinkDraft =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppLinkDraft)}";
        /// <summary>عقد جديد صريح — نقل ملكية الطلب بين موظفي نفس الفرع مع سبب إلزامي.</summary>
        public const string AppTransferName =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(AppTransferName)}";
        public const string TabIncoming =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabIncoming)}";
        public const string TabPrepared =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabPrepared)}";
        public const string TabInspected =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabInspected)}";
        public const string TabPending =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabPending)}";
        public const string TabRejected =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabRejected)}";
        public const string TabSold =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabSold)}";
        public const string TimelineRead =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TimelineRead)}";
        public const string TabSent =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabSent)}";
        public const string TabExceptions =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabExceptions)}";
        public const string TabDeleted =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabDeleted)}";
        public const string TabLegal =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(TabLegal)}";
        public const string ExceptionDecide =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(ExceptionDecide)}";
        public const string SoftDelete =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(SoftDelete)}";
        public const string Restore =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(Restore)}";

        /// <summary>موثّق كـ Not implemented — لا endpoint لتعديل الطلب في هذه المرحلة.</summary>
        public const string UpdateRequest =
            $"{nameof(Domain.Entities.SalesRequestAggregate.SalesRequest)}.{nameof(UpdateRequest)}";
    }

    /// <summary>خارج Permissions.All عمدًا — تُمنح لـ Admin (ومستخدمي اللوحة المخصصين) فقط.</summary>
    public static class SalesFollowup
    {
        private const string Display = "متابعة المبيعات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "رؤية لوحة متابعة المبيعات", nameof(SalesFollowup), Display),
            new(ManageAppBlock, "إيقاف/تفعيل تطبيق موظف المبيعات", nameof(SalesFollowup), Display),
            new(UpsertManagerRating, "تقييم المدير للمندوب", nameof(SalesFollowup), Display),
        ];

        public const string Read = $"{nameof(SalesFollowup)}.{nameof(Read)}";
        public const string ManageAppBlock = $"{nameof(SalesFollowup)}.{nameof(ManageAppBlock)}";
        public const string UpsertManagerRating = $"{nameof(SalesFollowup)}.{nameof(UpsertManagerRating)}";
    }

    /// <summary>نظرة عامة للمبيعات — خارج Permissions.All.</summary>
    public static class SalesOverview
    {
        private const string Display = "نظرة عامة للمبيعات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة مؤشرات نظرة عامة للمبيعات", nameof(SalesOverview), Display),
        ];

        public const string Read = "SalesOverview.Read";
    }

    /// <summary>دليل موظفي المبيعات — خارج Permissions.All. لا يمنح حظر التطبيق أو إدارة الصلاحيات.</summary>
    public static class SalesEmployee
    {
        private const string Display = "موظفو المبيعات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة قائمة وتفاصيل موظفي المبيعات", nameof(SalesEmployee), Display),
            new(ManageAssignment, "منح/إنهاء تكليف موظف المبيعات على فرع", nameof(SalesEmployee), Display),
        ];

        public const string Read = "SalesEmployee.Read";
        public const string ManageAssignment = "SalesEmployee.ManageAssignment";
    }

    /// <summary>الموقع المباشر لموظفي المبيعات — مستقل عن FollowerLocation.</summary>
    public static class SalesLocation
    {
        private const string Display = "موقع موظفي المبيعات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة آخر مواقع موظفي المبيعات", nameof(SalesLocation), Display),
        ];

        public const string Read = "SalesLocation.Read";
    }

    /// <summary>المسارات التاريخية لموظفي المبيعات — مستقلة عن SalesLocation وFollowerRoute.</summary>
    public static class SalesRoute
    {
        private const string Display = "مسارات موظفي المبيعات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة المسار التاريخي لموظف المبيعات", nameof(SalesRoute), Display),
        ];

        public const string Read = "SalesRoute.Read";
    }

    /// <summary>
    /// ملخص المبيعات (سعر البيع فقط) — خارج Permissions.All.
    /// لا يمنح ManagementSummary ولا العكس.
    /// </summary>
    public static class SalesSummary
    {
        private const string Display = "ملخص المبيعات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة مصفوفة ملخص المبيعات (سعر البيع)", nameof(SalesSummary), Display),
            new(Drilldown, "تفاصيل خلية ملخص المبيعات", nameof(SalesSummary), Display),
        ];

        public const string Read = "SalesSummary.Read";
        public const string Drilldown = "SalesSummary.Drilldown";
    }

    public static class DelegateFollowup
    {
        private const string Display = "متابعة المندوبين";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "رؤية لوحة متابعة المندوبين", nameof(DelegateFollowup), Display),
            new(UpsertBoardRating, "تقييم مشغّل اللوحة للقائمة", nameof(DelegateFollowup), Display),
        ];

        public const string Read = "DelegateFollowup.Read";
        public const string UpsertBoardRating = "DelegateFollowup.UpsertBoardRating";
    }

    /// <summary>
    /// الملخص الإداري (مصفوفة المدير) — خارج Permissions.All عمدًا.
    /// لا يُمنح عبر صلاحية ملخص المبيعات.
    /// </summary>
    public static class ManagementSummary
    {
        private const string Display = "الملخص الإداري";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة مصفوفة الملخص الإداري", nameof(ManagementSummary), Display),
            new(Drilldown, "تفاصيل خلية الملخص الإداري", nameof(ManagementSummary), Display),
        ];

        public const string Read = "ManagementSummary.Read";
        public const string Drilldown = "ManagementSummary.Drilldown";
    }

    /// <summary>
    /// التقييمات العامة (لوحة المدير/المراقب) — خارج Permissions.All.
    /// ليست SalesFollowup ولا تقييم عامل اللوحة.
    /// </summary>
    public static class ManagementRatings
    {
        private const string Display = "التقييمات العامة";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة لوحة التقييمات العامة", nameof(ManagementRatings), Display),
            new(DetailsRead, "تفاصيل زبائن فئات التقييم", nameof(ManagementRatings), Display),
        ];

        public const string Read = "ManagementRatings.Read";
        public const string DetailsRead = "ManagementRatings.DetailsRead";
    }

    /// <summary>
    /// عمل المتابعين (دليل البطاقات + صلاحيات الأقسام) — خارج Permissions.All.
    /// FollowerWork.Read = دليل الهوية/البطاقات؛ صلاحيات الأقسام مستقلة ولا تفتح بيانات بعضها.
    /// أقسام 06–13 مسجّلة للمستقبل؛ لا تُزرع كـ ready في القوالب حتى تنفيذ دفعتها.
    /// </summary>
    public static class FollowerWork
    {
        private const string Display = "عمل المتابعين";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة دليل المتابعين وبطاقاتهم", nameof(FollowerWork), Display),
            new(SalesReviewRead, "مصادقة البيع — عمل المتابعين", "FollowerSalesReview", Display),
            new(SalesReviewAuthenticate, "تسجيل مصادقة بيع ميدانية", "FollowerSalesReview", Display),
            new(DecliningRead, "المتدنية — عمل المتابعين", "FollowerDeclining", Display),
            new(NotesRead, "الملاحظات — عمل المتابعين", "FollowerNotes", Display),
            new(NotesWrite, "كتابة ملاحظة زبون ميدانية", "FollowerNotes", Display),
            new(OldCustomersRead, "الزبائن القديمة — عمل المتابعين", "FollowerOldCustomers", Display),
            new(DelegateNotesRead, "ملاحظات المندوبين — عمل المتابعين", "FollowerDelegateNotes", Display),
            new(DelegateNotesWrite, "كتابة ملاحظة مندوب ميدانية (متابع)", "FollowerDelegateNotes", Display),
            new(CollectionRead, "الجباية — عمل المتابعين", "FollowerCollection", Display),
            new(CollectionManageEnabled, "تشغيل/إيقاف جباية المتابع على القائمة", "FollowerCollection", Display),
            new(LocationRead, "الموقع المباشر — عمل المتابعين", "FollowerLocation", Display),
            new(RouteRead, "المسارات — عمل المتابعين", "FollowerRoute", Display),
        ];

        public const string Read = "FollowerWork.Read";
        public const string SalesReviewRead = "FollowerSalesReview.Read";
        public const string SalesReviewAuthenticate = "FollowerSalesReview.Authenticate";
        public const string DecliningRead = "FollowerDeclining.Read";
        public const string NotesRead = "FollowerNotes.Read";
        public const string NotesWrite = "FollowerNotes.Write";
        public const string OldCustomersRead = "FollowerOldCustomers.Read";
        public const string DelegateNotesRead = "FollowerDelegateNotes.Read";
        public const string DelegateNotesWrite = "FollowerDelegateNotes.Write";
        public const string CollectionRead = "FollowerCollection.Read";
        public const string CollectionManageEnabled = "FollowerCollection.ManageEnabled";
        public const string LocationRead = "FollowerLocation.Read";
        public const string RouteRead = "FollowerRoute.Read";
    }

    /// <summary>متابعة قانونية — قراءة/إنشاء خارج Permissions.All؛ Cancel مستقل.</summary>
    public static class LegalFollowup
    {
        private const string Display = "المتابعة القانونية";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة قائمة وتفاصيل المتابعة القانونية", nameof(LegalFollowup), Display),
            new(Create, "إنشاء قضية متابعة قانونية من طلب بيع", nameof(LegalFollowup), Display),
            new(Cancel, "إلغاء متابعة قانونية قيد العمل", nameof(LegalFollowup), Display),
            new(FieldRead, "قراءة قضايا المتابع القانونية المسندة له", nameof(LegalFollowup), Display),
            new(FieldUpload, "رفع مستمسكات قضية قانونية مسندة", nameof(LegalFollowup), Display),
            new(FieldComplete, "إكمال متابعة قانونية ميدانية", nameof(LegalFollowup), Display),
        ];

        public const string Read = "LegalFollowup.Read";
        public const string Create = "LegalFollowup.Create";
        public const string Cancel = "LegalFollowup.Cancel";
        public const string FieldRead = "LegalFollowup.FieldRead";
        public const string FieldUpload = "LegalFollowup.FieldUpload";
        public const string FieldComplete = "LegalFollowup.FieldComplete";
    }

    /// <summary>
    /// مهام إدارية — خارج Permissions.All عمدًا (أسماء العقود Task.* حسب المواصفة).
    /// تُمنح لـ Admin صراحة فقط؛ لا توسعة أدوار افتراضية بلا سياسة.
    /// </summary>
    public static class ManagementTask
    {
        private const string Display = "المهام الإدارية";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة المهام الإدارية", nameof(ManagementTask), Display),
            new(Create, "إنشاء مهمة إدارية", nameof(ManagementTask), Display),
            new(Assign, "إسناد/إعادة إرسال مهمة", nameof(ManagementTask), Display),
            new(Accept, "قبول مهمة", nameof(ManagementTask), Display),
            new(Reject, "رفض مهمة", nameof(ManagementTask), Display),
            new(Complete, "إنجاز مهمة", nameof(ManagementTask), Display),
            new(Confirm, "تصديق مهمة منقولة", nameof(ManagementTask), Display),
            new(Transfer, "نقل مهمة لمدير فرع", nameof(ManagementTask), Display),
            new(ReadAttachment, "تنزيل مرفقات المهمة", nameof(ManagementTask), Display),
        ];

        public const string Read = "Task.Read";
        public const string Create = "Task.Create";
        public const string Assign = "Task.Assign";
        public const string Accept = "Task.Accept";
        public const string Reject = "Task.Reject";
        public const string Complete = "Task.Complete";
        public const string Confirm = "Task.Confirm";
        public const string Transfer = "Task.Transfer";
        public const string ReadAttachment = "Task.ReadAttachment";
    }

    /// <summary>نشرة مهام اللجنة — خارج Permissions.All.</summary>
    public static class CommitteeTask
    {
        private const string Display = "مهام اللجنة";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة نشرة مهام اللجنة في اللوحة", nameof(CommitteeTask), Display),
            new(Write, "إنشاء/تعديل/حذف مهام اللجنة", nameof(CommitteeTask), Display),
            new(AppRead, "قراءة المهام المفعّلة في تطبيق اللجنة", nameof(CommitteeTask), Display),
        ];

        public const string Read = "CommitteeTask.Read";
        public const string Write = "CommitteeTask.Write";
        public const string AppRead = "CommitteeTask.AppRead";
    }

    /// <summary>ملاحظات التطبيقات → وارد الإدارة — خارج Permissions.All.</summary>
    public static class AppNote
    {
        private const string Display = "ملاحظات التطبيقات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة صندوق ملاحظات التطبيقات", nameof(AppNote), Display),
            new(Create, "إرسال ملاحظة من تطبيق ميداني", nameof(AppNote), Display),
        ];

        public const string Read = "AppNote.Read";
        public const string Create = "AppNote.Create";
    }

    /// <summary>التنازلات — خارج Permissions.All.</summary>
    public static class Waiver
    {
        private const string Display = "التنازلات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة طلبات التنازل", nameof(Waiver), Display),
            new(Create, "إنشاء طلب تنازل من الفرع", nameof(Waiver), Display),
            new(Decide, "قبول/رفض طلب التنازل (مفوض)", nameof(Waiver), Display),
            new(Apply, "تطبيق خصم التنازل (محاسب)", nameof(Waiver), Display),
        ];

        public const string Read = "Waiver.Read";
        public const string Create = "Waiver.Create";
        public const string Decide = "Waiver.Decide";
        public const string Apply = "Waiver.Apply";
    }

    /// <summary>أسماء قانونية / قرارات فرع — خارج Permissions.All.</summary>
    public static class CustomerDecision
    {
        private const string Display = "قرارات الفرع";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة سجل قرارات الفرع", nameof(CustomerDecision), Display),
            new(Create, "تسجيل قرار فرع (متواصل/وهمي/قانونية/تنازل)", nameof(CustomerDecision), Display),
        ];

        public const string Read = "CustomerDecision.Read";
        public const string Create = "CustomerDecision.Create";
    }

    public static class LegalNames
    {
        private const string Display = "الأسماء القانونية";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة طابور الأسماء القانونية للفرع", nameof(LegalNames), Display),
        ];

        public const string Read = "LegalNames.Read";
    }

    public static class LegalConversion
    {
        private const string Display = "تحويل قانونية";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Decide, "اعتماد/رفض طلب التحويل القانوني", nameof(LegalConversion), Display),
        ];

        public const string Decide = "LegalConversion.Decide";
    }

    /// <summary>لوحة أسعار المحافظة — إعلانات أسعار معلوماتية؛ خارج Permissions.All.</summary>
    public static class PriceBoard
    {
        private const string Display = "لوحة الأسعار";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة لوحة الأسعار", nameof(PriceBoard), Display),
            new(Create, "إضافة إعلان أسعار", nameof(PriceBoard), Display),
            new(Update, "تعديل إعلان أسعار", nameof(PriceBoard), Display),
            new(Delete, "حذف إعلان أسعار", nameof(PriceBoard), Display),
        ];

        public const string Read = "PriceBoard.Read";
        public const string Create = "PriceBoard.Create";
        public const string Update = "PriceBoard.Update";
        public const string Delete = "PriceBoard.Delete";
    }

    /// <summary>إعلانات بانر التطبيقات — خارج Permissions.All؛ ليست AppNote ولا Push.</summary>
    public static class Announcement
    {
        private const string Display = "الإعلانات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة إدارة الإعلانات", nameof(Announcement), Display),
            new(Create, "إنشاء إعلان", nameof(Announcement), Display),
            new(Update, "تعديل/تفعيل إعلان", nameof(Announcement), Display),
            new(Delete, "حذف إعلان", nameof(Announcement), Display),
            new(AppRead, "قراءة إعلانات التطبيق المستهدف", nameof(Announcement), Display),
        ];

        public const string Read = "Announcement.Read";
        public const string Create = "Announcement.Create";
        public const string Update = "Announcement.Update";
        public const string Delete = "Announcement.Delete";
        public const string AppRead = "Announcement.AppRead";
    }

    /// <summary>فيديوهات تعليمية (روابط خارجية) — خارج Permissions.All.</summary>
    public static class Tutorial
    {
        private const string Display = "الفيديوهات التعليمية";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة إدارة الفيديوهات التعليمية", nameof(Tutorial), Display),
            new(Create, "إنشاء فيديو تعليمي", nameof(Tutorial), Display),
            new(Update, "تعديل/تفعيل فيديو تعليمي", nameof(Tutorial), Display),
            new(Delete, "حذف فيديو تعليمي", nameof(Tutorial), Display),
            new(AppRead, "قراءة فيديوهات التطبيق المستهدف", nameof(Tutorial), Display),
        ];

        public const string Read = "Tutorial.Read";
        public const string Create = "Tutorial.Create";
        public const string Update = "Tutorial.Update";
        public const string Delete = "Tutorial.Delete";
        public const string AppRead = "Tutorial.AppRead";
    }

    /// <summary>نشر تحديثات التطبيقات الميدانية (إجباري/اختياري) — خارج Permissions.All.</summary>
    public static class MobileAppRelease
    {
        private const string Display = "تحديثات التطبيقات";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة إصدارات التطبيقات المنشورة", nameof(MobileAppRelease), Display),
            new(Publish, "نشر/رفع تحديث إجباري أو اختياري", nameof(MobileAppRelease), Display),
        ];

        public const string Read = "MobileAppRelease.Read";
        public const string Publish = "MobileAppRelease.Publish";
    }

    /// <summary>صندوق المبيعات الخاطئة (مصادقة ≤6) — خارج Permissions.All.</summary>
    public static class WrongSale
    {
        private const string Display = "المبيعات الخاطئة";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة صندوق المبيعات الخاطئة", nameof(WrongSale), Display),
            new(MarkDelegateRead, "تعليمليم مقروء (مفوض)", nameof(WrongSale), Display),
            new(MarkBranchReviewed, "مراجعة إدارة الفرع", nameof(WrongSale), Display),
        ];

        public const string Read = "WrongSale.Read";
        public const string MarkDelegateRead = "WrongSale.MarkDelegateRead";
        public const string MarkBranchReviewed = "WrongSale.MarkBranchReviewed";
    }

    /// <summary>صندوق مبيعات الفرع الجديدة (مقروء/غير مقروء) — خارج Permissions.All.</summary>
    public static class BranchNewSale
    {
        private const string Display = "مبيعات الفرع الجديدة";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة صندوق المبيعات الجديدة للفرع", nameof(BranchNewSale), Display),
            new(MarkRead, "تعليمليم مقروء في صندوق مبيعات الفرع", nameof(BranchNewSale), Display),
        ];

        public const string Read = "BranchNewSale.Read";
        public const string MarkRead = "BranchNewSale.MarkRead";
    }

    public static class BranchDeclining
    {
        private const string Display = "الزبائن المتدنية — الفرع";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة زبائن الفرع المتدنية", nameof(BranchDeclining), Display),
        ];

        public const string Read = "BranchDeclining.Read";
    }

    public static class Complaint
    {
        private const string Display = "الرسائل والشكاوى";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة صندوق الشكاوى", nameof(Complaint), Display),
            new(MarkRead, "تعليمليم شكوى مقروءة", nameof(Complaint), Display),
            new(Submit, "إرسال شكوى من التطبيق", nameof(Complaint), Display),
        ];

        public const string Read = "Complaint.Read";
        public const string MarkRead = "Complaint.MarkRead";
        public const string Submit = "Complaint.Submit";
    }

    public static class BranchVisit
    {
        private const string Display = "أعمال إدارة الفرع";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة زيارات الفرع", nameof(BranchVisit), Display),
            new(Open, "فتح زيارة فرع", nameof(BranchVisit), Display),
            new(Rate, "تقييم قوائم أثناء الزيارة", nameof(BranchVisit), Display),
            new(Depart, "مغادرة زيارة الفرع", nameof(BranchVisit), Display),
        ];

        public const string Read = "BranchVisit.Read";
        public const string Open = "BranchVisit.Open";
        public const string Rate = "BranchVisit.Rate";
        public const string Depart = "BranchVisit.Depart";
    }

    public static class BranchManagerAccount
    {
        private const string Display = "حسابات إدارة الفرع";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة حسابات مديري الفرع", nameof(BranchManagerAccount), Display),
            new(Write, "إنشاء/تعديل حسابات مديري الفرع ضمن النطاق", nameof(BranchManagerAccount), Display),
        ];

        public const string Read = "BranchManagerAccount.Read";
        public const string Write = "BranchManagerAccount.Write";
    }

    public static class DelegateHome
    {
        private const string Display = "رئيسية المفوض";

        public static readonly PermissionEntity[] OutsideAll =
        [
            new(Read, "قراءة ملخص رئيسية المفوض", nameof(DelegateHome), Display),
        ];

        public const string Read = "DelegateHome.Read";
    }
}