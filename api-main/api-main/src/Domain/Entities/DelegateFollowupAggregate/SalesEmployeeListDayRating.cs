using Domain.Common;

namespace Domain.Entities.DelegateFollowupAggregate;

/// <summary>
/// تقييم مندوب المبيعات للقائمة — UX (EmployeeUserId, OrderListId, RatingDate).
/// يُكتب من وحدة المبيعات؛ اللوحة تعرض متوسط الدرجة لليوم.
/// </summary>
public class SalesEmployeeListDayRating : Entity
{
    private SalesEmployeeListDayRating()
    {
    }

    public SalesEmployeeListDayRating(
        int orderListId,
        int branchId,
        int employeeUserId,
        DateOnly ratingDate,
        int score,
        string? note,
        DateTimeOffset ratedAtUtc)
    {
        OrderListId = orderListId;
        BranchId = branchId;
        EmployeeUserId = employeeUserId;
        RatingDate = ratingDate;
        Score = score;
        Note = note;
        RatedAtUtc = ratedAtUtc;
    }

    public int OrderListId { get; set; }
    public int BranchId { get; set; }
    public int EmployeeUserId { get; set; }
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
