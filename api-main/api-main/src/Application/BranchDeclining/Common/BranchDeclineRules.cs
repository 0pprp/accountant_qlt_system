using Application.FollowerWork.Common;

namespace Application.BranchDeclining.Common;

/// <summary>
/// متدنية الفرع — مطابقة BmCustomerDeclineService:
/// ممتاز → سيء|متلكئ خلال 15 يومًا. بلا شرط هبوط نقاط (−10).
/// ليست FollowerDeclining (7 أيام + انتقالات مختلفة + MinScoreDrop).
/// </summary>
public static class BranchDeclineRules
{
    public const int LookbackDays = 15;

    public static bool IsTargetDecline(string? fromLabel, string? toLabel)
    {
        var from = FollowerDeclineRules.NormalizeBand(fromLabel);
        var to = FollowerDeclineRules.NormalizeBand(toLabel);
        return from == FollowerDeclineRules.Excellent
               && (to == FollowerDeclineRules.Bad || to == FollowerDeclineRules.Lagging);
    }

    public static bool IsDeclining(
        string? fromLabel,
        string? toLabel,
        int daysBetween)
    {
        if (daysBetween < 0 || daysBetween > LookbackDays)
            return false;
        return IsTargetDecline(fromLabel, toLabel);
    }
}
