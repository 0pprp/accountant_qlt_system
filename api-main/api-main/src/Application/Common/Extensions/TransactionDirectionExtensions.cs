using Domain.Entities.SafeAggregate.Enums;

namespace Application.Common.Extensions;

public static class TransactionDirectionExtensions
{
    public static string ToArabicString(this TransactionDirection direction)
    {
        return direction switch
        {
            TransactionDirection.In => "داخل",
            TransactionDirection.Out => "خارج",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}