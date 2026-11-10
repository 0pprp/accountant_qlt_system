using Domain.Common;

namespace Domain.Entities.DelegateFollowupAggregate;

/// <summary>
/// تقييم لجنة الإشراف (إدارة الفرع) لليوم — PK منطقي (OrderListId, RatingDate).
/// يُكتب من مسارات زيارة الفرع؛ لوحة متابعة المندوبين تقرأه فقط.
/// </summary>
public class ListSupervisionDayRating : Entity
{
    private ListSupervisionDayRating()
    {
    }

    public ListSupervisionDayRating(
        int orderListId,
        int branchId,
        DateOnly ratingDate,
        int score,
        string? note,
        int? ratedByUserId,
        string? ratedByUserName,
        DateTimeOffset ratedAtUtc)
    {
        OrderListId = orderListId;
        BranchId = branchId;
        RatingDate = ratingDate;
        Score = score;
        Note = note;
        RatedByUserId = ratedByUserId;
        RatedByUserName = ratedByUserName;
        RatedAtUtc = ratedAtUtc;
    }

    public int OrderListId { get; set; }
    public int BranchId { get; set; }
    public DateOnly RatingDate { get; set; }
    public int Score { get; set; }
    public string? Note { get; set; }
    public int? RatedByUserId { get; set; }
    public string? RatedByUserName { get; set; }
    public DateTimeOffset RatedAtUtc { get; set; }

    public void Update(
        int score,
        string? note,
        int? ratedByUserId,
        string? ratedByUserName,
        DateTimeOffset ratedAtUtc)
    {
        Score = score;
        Note = note;
        RatedByUserId = ratedByUserId;
        RatedByUserName = ratedByUserName;
        RatedAtUtc = ratedAtUtc;
    }
}
