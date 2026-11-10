using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Common;

/// <summary>
/// «مهامي» = مهام يحتاجها المستخدم عملًا، وليست CurrentHolder وحده لكل الحالات.
/// </summary>
public static class ManagementTaskInboxRules
{
    /// <summary>
    /// هل تظهر المهمة في صندوق «مهامي» للمستخدم؟
    /// حامل حالي؛ أو مكلَّف أصلي بانتظار تصديق؛ أو مكلَّف أصلي لمهمة معادة لإعادة الإرسال.
    /// </summary>
    public static bool MatchesInboxForMe(
        int userId,
        int currentHolderUserId,
        int assigneeUserId,
        ManagementTaskStatus status)
    {
        if (userId <= 0)
            return false;

        if (currentHolderUserId == userId)
            return true;

        // بعد رفض الفرع يبقى الحامل الرافض؛ المكلَّف الأصلي يحتاج رؤيتها لإعادة الإرسال
        if (assigneeUserId == userId
            && status is ManagementTaskStatus.Returned or ManagementTaskStatus.AwaitingDelegatedConfirm)
            return true;

        return false;
    }

    /// <summary>الافتراضي لصندوق مفتوح: يستبعد المكتملة فقط (يشمل المعادة للمكلَّف).</summary>
    public static bool IsDefaultOpenStatus(ManagementTaskStatus status)
        => status != ManagementTaskStatus.Completed;
}
