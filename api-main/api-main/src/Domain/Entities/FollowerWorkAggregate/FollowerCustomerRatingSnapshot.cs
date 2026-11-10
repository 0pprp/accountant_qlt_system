using Domain.Common;

namespace Domain.Entities.FollowerWorkAggregate;

/// <summary>
/// لقطة تقييم يومية لزبون ضمن قائمة متابع — مصدر مقارنة «المتدنية».
/// يُكتب عند تحديث اللقطات (تطبيق المتابع / قراءة القسم)، وليس تاريخًا وهميًا.
/// الفرادة: CustomerId + CaptureDate (يوم بغداد).
/// </summary>
public class FollowerCustomerRatingSnapshot : Entity<long>
{
    private FollowerCustomerRatingSnapshot()
    {
    }

    public FollowerCustomerRatingSnapshot(
        int customerId,
        int orderListId,
        string? customerName,
        string? phoneNumber,
        string ratingLabel,
        int score,
        DateOnly captureDate,
        DateTimeOffset capturedAtUtc)
    {
        CustomerId = customerId;
        OrderListId = orderListId;
        CustomerName = customerName;
        PhoneNumber = phoneNumber;
        RatingLabel = ratingLabel;
        Score = score;
        CaptureDate = captureDate;
        CapturedAtUtc = capturedAtUtc;
    }

    public int CustomerId { get; private set; }
    public int OrderListId { get; private set; }
    public string? CustomerName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string RatingLabel { get; private set; } = "";
    public int Score { get; private set; }
    public DateOnly CaptureDate { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }

    public void ReplaceCapture(
        int orderListId,
        string? customerName,
        string? phoneNumber,
        string ratingLabel,
        int score,
        DateTimeOffset capturedAtUtc)
    {
        OrderListId = orderListId;
        CustomerName = customerName;
        PhoneNumber = phoneNumber;
        RatingLabel = ratingLabel;
        Score = score;
        CapturedAtUtc = capturedAtUtc;
    }
}
