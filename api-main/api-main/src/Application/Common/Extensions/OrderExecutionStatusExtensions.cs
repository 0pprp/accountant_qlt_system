using Domain.Entities.OrderAggregate.Enums;

namespace Application.Common.Extensions;

public static class OrderExecutionStatusExtensions
{
    public static string ToArabicString(this OrderExecutionStatus executionStatus)
    {
        return executionStatus switch
        {
            OrderExecutionStatus.NotStarted => "لم يبدأ بعد",
            OrderExecutionStatus.InProgress => "جاري",
            OrderExecutionStatus.Completed => "مكتمل",
            _ => throw new ArgumentOutOfRangeException(nameof(executionStatus), executionStatus, null)
        };
    }
}