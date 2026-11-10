using Domain.Common;

namespace Domain.Entities.DelegateFollowupAggregate;

/// <summary>
/// تقييم مسؤول اللجنة الميدانية (المتابع) — UX (FollowerUserId, OrderListId, RatingDate).
/// يُكتب من تطبيق المتابع؛ اللوحة تعرض أحدث/اليوم ومتوسط اليوم إن تعدد.
/// </summary>
public class FollowerDelegateDayRating : Entity
{
    private FollowerDelegateDayRating()
    {
    }

    public FollowerDelegateDayRating(
        int orderListId,
        int branchId,
        int followerUserId,
        DateOnly ratingDate,
        int score,
        string? note,
        DateTimeOffset ratedAtUtc)
    {
        OrderListId = orderListId;
        BranchId = branchId;
        FollowerUserId = followerUserId;
        RatingDate = ratingDate;
        Score = score;
        Note = note;
        RatedAtUtc = ratedAtUtc;
    }

    public int OrderListId { get; set; }
    public int BranchId { get; set; }
    public int FollowerUserId { get; set; }
    public DateOnly RatingDate { get; set; }
    public int Score { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset RatedAtUtc { get; set; }

    public void Update(int score, string? note, DateTimeOffset ratedAtUtc)
    {
        Score = score;
        Note = note;
        RatedAtUtc = ratedAtUtc;
    }
}
