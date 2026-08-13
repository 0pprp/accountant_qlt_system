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

        public const string Create = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Create)}";
        public const string Read = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Read)}";
        public const string Update = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Domain.Entities.UserAggregate.User)}.{nameof(Delete)}";
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
}