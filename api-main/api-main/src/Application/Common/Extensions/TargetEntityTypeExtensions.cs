using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.Common.Extensions;

public static class TargetEntityTypeExtensions
{
    public static string ToArabicString(this TargetEntityType targetEntityType)
    {
        return targetEntityType switch
        {
            TargetEntityType.User => "مستخدم",
            TargetEntityType.Branch => "فرع",
            TargetEntityType.Customer => "عميل",
            TargetEntityType.Order => "طلب",
            TargetEntityType.OrderList => "قائمة طلبات",
            TargetEntityType.Product => "منتج",
            TargetEntityType.ProductCategory => "تصنيف منتج",
            TargetEntityType.InstallmentPayment => "تسديد",
            TargetEntityType.Purchase => "مشتريات",
            TargetEntityType.Expense => "صرفيات",
            TargetEntityType.Safe => "خزنة",
            TargetEntityType.Transaction => "معاملة",
            TargetEntityType.Attachment => "مرفق",
            TargetEntityType.Notification => "إشعار",
            _ => throw new ArgumentOutOfRangeException(nameof(targetEntityType), targetEntityType, null)
        };
    }
}
