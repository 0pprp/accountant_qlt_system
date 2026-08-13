using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.Common.Extensions;

public static class ActivityTypeExtensions
{
    public static string ToArabicString(this ActivityType activityType) => activityType.GetDisplayName();
}
