using Domain.Entities.SafeAggregate.Enums;

namespace Application.Common.Extensions;

public static class TransactionTypeExtensions
{
    public static string ToArabicString(this TransactionType type)
    {
        return type switch
        {
            TransactionType.SellerPayment => "تسدید البائع",
            TransactionType.Purchase => "مشتریات",
            TransactionType.Expense => "صرفیات",
            TransactionType.SafeTransfer => "تحويل الأموال",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}