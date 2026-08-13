using Domain.Entities.OrderAggregate.Enums;

namespace Application.Common.Extensions;

public static class OrderApprovalStatusExtensions
{
    public static string ToArabicString(this OrderApprovalStatus executionStatus)
    {
        return executionStatus switch
        {
            OrderApprovalStatus.Pending => "قيد المراجعة",
            OrderApprovalStatus.Approved => "تمت الموافقة",
            OrderApprovalStatus.Rejected => "مرفوض",
            _ => throw new ArgumentOutOfRangeException(nameof(executionStatus), executionStatus, null)
        };
    }
}