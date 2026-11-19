using Domain.Entities.SafeAggregate.Enums;

namespace Application.Common.Extensions;

public static class SafeMovementKindExtensions
{
    public static string ToArabicString(this SafeMovementKind kind) => kind.GetDisplayName();
}
