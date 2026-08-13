using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.ActivityLogAggregate.Enums;

public enum ActivityType : byte
{
    [Display(Name = "تسجيل دخول")]
    Login = 0,

    [Display(Name = "تسجيل خروج")]
    Logout = 1,

    [Display(Name = "إنشاء مستخدم")]
    UserCreated = 2,

    [Display(Name = "تعديل مستخدم")]
    UserUpdated = 3,

    [Display(Name = "تعديل صلاحيات مستخدم")]
    UserPermissionsUpdated = 4,

    [Display(Name = "تعديل راتب مستخدم")]
    UserSalaryDetailUpdated = 5,

    [Display(Name = "إكمال مستندات مستخدم")]
    UserPersonalDocumentsCompleted = 6,

    [Display(Name = "إنشاء فرع")]
    BranchCreated = 7,

    [Display(Name = "تعديل فرع")]
    BranchUpdated = 8,

    [Display(Name = "حذف فرع")]
    BranchDeleted = 9,

    [Display(Name = "إنشاء عميل")]
    CustomerCreated = 10,

    [Display(Name = "تعديل عميل")]
    CustomerUpdated = 11,

    [Display(Name = "إنشاء منتج")]
    ProductCreated = 12,

    [Display(Name = "تعديل منتج")]
    ProductUpdated = 13,

    [Display(Name = "حذف منتج")]
    ProductDeleted = 14,

    [Display(Name = "إنشاء تصنيف منتج")]
    ProductCategoryCreated = 15,

    [Display(Name = "تعديل تصنيف منتج")]
    ProductCategoryUpdated = 16,

    [Display(Name = "حذف تصنيف منتج")]
    ProductCategoryDeleted = 17,

    [Display(Name = "إنشاء طلب")]
    OrderCreated = 18,

    [Display(Name = "تعديل طلب")]
    OrderUpdated = 19,

    [Display(Name = "حذف طلب")]
    OrderDeleted = 20,

    [Display(Name = "تغيير حالة موافقة طلب")]
    OrderApprovalStatusChanged = 21,

    [Display(Name = "تعيين معلومات بائع طلب")]
    OrderSellerInfoSet = 22,

    [Display(Name = "إكمال مرفقات طلب")]
    OrderAttachmentsCompleted = 23,

    [Display(Name = "إنشاء قائمة طلبات")]
    OrderListCreated = 24,

    [Display(Name = "تعديل قائمة طلبات")]
    OrderListUpdated = 25,

    [Display(Name = "إنشاء تسديد")]
    InstallmentPaymentCreated = 26,

    [Display(Name = "تعديل تسديد")]
    InstallmentPaymentUpdated = 27,

    [Display(Name = "حذف تسديد")]
    InstallmentPaymentDeleted = 28,

    [Display(Name = "مزامنة تسديدات")]
    InstallmentPaymentsSynced = 29,

    [Display(Name = "إنشاء مشتريات")]
    PurchaseCreated = 30,

    [Display(Name = "تعديل مشتريات")]
    PurchaseUpdated = 31,

    [Display(Name = "إنشاء صرفيات")]
    ExpenseCreated = 32,

    [Display(Name = "تعديل صرفيات")]
    ExpenseUpdated = 33,

    [Display(Name = "تسليم نقدي")]
    CashDeliveryCreated = 34,

    [Display(Name = "تسليم نقدي جماعي")]
    CashDeliveriesCreated = 35,

    [Display(Name = "تحويل أموال")]
    SafeTransferCreated = 36,

    [Display(Name = "تغيير حالة معاملة")]
    TransactionStatusChanged = 37,

    [Display(Name = "إنشاء مرفق")]
    AttachmentCreated = 38,

    [Display(Name = "تعديل مرفق")]
    AttachmentUpdated = 39,

    [Display(Name = "تحديد إشعار كمقروء")]
    NotificationMarkedAsRead = 40,

    [Display(Name = "تحديد جميع الإشعارات كمقروءة")]
    AllNotificationsMarkedAsRead = 41
}
