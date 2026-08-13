using Domain.Entities.SafeAggregate.Enums;

namespace Application.Common.Extensions;

public static class TransactionStatusExtensions
{
    public static string ToArabicString(this TransactionStatus status)
    {
        return status switch
        {
            TransactionStatus.Pending => "قيد الانتظار",
            TransactionStatus.Approved => "موافقة",
            TransactionStatus.Rejected => "مرفوض",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}