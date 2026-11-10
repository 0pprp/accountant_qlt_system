using Application.SalesRequests.Commands.App.Submit;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.SalesRequests;

public class SalesRequestSubmissionTests
{
    [Theory]
    [InlineData("07801234567", true)]
    [InlineData("٠٧٨٠١٢٣٤٥٦٧", true)]
    [InlineData("۰۷۸۰۱۲۳۴۵۶۷", true)]
    [InlineData("+9647801234567", false)]
    [InlineData("0780123456", false)]
    [InlineData("078012345678", false)]
    [InlineData("08801234567", false)]
    [InlineData("07801 34567", false)]
    [InlineData("0780 123 4567", true)]
    [InlineData("٠٧٨٠ ١٢٣٤٥٦٧", true)]
    public void Phone_contract(string input, bool valid)
        => Assert.Equal(valid, SalesRequestSubmissionRules.ValidPhone(input));

    [Theory]
    [InlineData(SalesRequestSourceType.EmployeeSubmitted)]
    [InlineData(SalesRequestSourceType.Delegate)]
    [InlineData(SalesRequestSourceType.Follower)]
    public void Submission_stays_unassigned_without_customer_or_order(SalesRequestSourceType source)
    {
        var entity = new SalesRequest("أحمد محمد", "07801234567", source, 1, null, null, 10);
        entity.SetSubmission("بغداد", "ثلاجة", null, Guid.NewGuid(), "hash", DateTimeOffset.UtcNow);
        Assert.True(entity.IsUnassigned());
        Assert.Null(entity.TargetEmployeeId);
        Assert.Null(entity.LinkedOrderId);
        Assert.Null(entity.ExistingCustomerId);
    }

    [Fact]
    public void Same_key_different_content_has_different_fingerprint()
    {
        var command = new SubmitSalesRequestCommand { SubmissionKey = Guid.NewGuid(), BranchId = 1,
            CustomerName = "أحمد", CustomerPhone = "07801234567", Address = "بغداد", RequestedProduct = "ثلاجة" };
        Assert.Equal(SalesRequestSubmissionRules.Fingerprint(command),
            SalesRequestSubmissionRules.Fingerprint(command with { CustomerPhone = "٠٧٨٠١٢٣٤٥٦٧" }));
        Assert.NotEqual(SalesRequestSubmissionRules.Fingerprint(command),
            SalesRequestSubmissionRules.Fingerprint(command with { RequestedProduct = "غسالة" }));
        Assert.NotEqual(SalesRequestSubmissionRules.Fingerprint(command),
            SalesRequestSubmissionRules.Fingerprint(command with { BranchId = 2 }));
    }

    [Fact]
    public void Missing_fields_rejected_but_note_optional()
    {
        var validator = new SubmitSalesRequestCommandValidator();
        Assert.False(validator.Validate(new SubmitSalesRequestCommand()).IsValid);
        Assert.True(validator.Validate(new SubmitSalesRequestCommand { SubmissionKey = Guid.NewGuid(), BranchId = 1,
            CustomerName = "أحمد", CustomerPhone = "07801234567", Address = "بغداد", RequestedProduct = "ثلاجة" }).IsValid);
    }
}
