using Domain.Entities.CustomerDecisionAggregate;
using Domain.Entities.CustomerDecisionAggregate.Enums;
using Domain.Entities.LegalConversionAggregate;
using Domain.Entities.LegalConversionAggregate.Enums;

namespace Application.Tests.CustomerDecisions;

public class CustomerDecisionDomainTests
{
    [Fact]
    public void Decision_ledger_is_immutable_record()
    {
        var at = DateTimeOffset.Parse("2026-11-09T12:00:00Z");
        var d = new CustomerDecision(
            customerId: 5,
            branchId: 1,
            actorUserId: 3,
            actorName: "مدير فرع",
            decisionType: CustomerDecisionType.Continue,
            note: "متابعة أسبوع",
            weekPaid: 10,
            amountTotalSales: 1000,
            paidPercent: 1,
            snoozeUntil: at.AddDays(7),
            relatedWaiverRequestId: null,
            relatedLegalConversionId: null,
            createdAtUtc: at);

        Assert.Equal(CustomerDecisionType.Continue, d.DecisionType);
        Assert.Equal("متابعة أسبوع", d.Note);
        Assert.Equal(at.AddDays(7), d.SnoozeUntil);
        Assert.Equal("متواصل", CustomerDecisionTypeLabels.Arabic(d.DecisionType));
    }

    [Theory]
    [InlineData("متواصل", CustomerDecisionType.Continue)]
    [InlineData("وهمي", CustomerDecisionType.FakeSale)]
    [InlineData("طلب قانونية", CustomerDecisionType.LegalRequest)]
    [InlineData("طلب تنازل", CustomerDecisionType.WaiverRequest)]
    [InlineData("قانونية", CustomerDecisionType.Legal)]
    [InlineData("تنازل", CustomerDecisionType.Waiver)]
    public void Arabic_labels_roundtrip(string arabic, CustomerDecisionType expected)
    {
        Assert.True(CustomerDecisionTypeLabels.TryParseArabic(arabic, out var parsed));
        Assert.Equal(expected, parsed);
        Assert.Equal(arabic, CustomerDecisionTypeLabels.Arabic(expected));
    }

    [Fact]
    public void Legal_conversion_approve_then_second_fails_on_status()
    {
        var at = DateTimeOffset.UtcNow;
        var c = new LegalConversionRequest(5, 1, 9, "فرع", "طلب قانونية", at);
        Assert.Equal(LegalConversionStatus.Pending, c.Status);

        c.Approve(2, "مراقب", "موافق", 0, at.AddMinutes(2));
        Assert.Equal(LegalConversionStatus.Approved, c.Status);
        Assert.Equal(1, c.ConcurrencyStamp);

        Assert.Throws<InvalidOperationException>(() =>
            c.Approve(2, "مراقب", "إعادة", 1, at.AddMinutes(3)));
    }

    [Fact]
    public void Legal_conversion_reject_requires_reason_and_version()
    {
        var c = new LegalConversionRequest(1, 1, 3, "فرع", "ملاحظة", DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            c.Reject(4, "مراقب", "", 0, DateTimeOffset.UtcNow));
        c.Reject(4, "مراقب", "غير مكتمل", 0, DateTimeOffset.UtcNow);
        Assert.Equal(LegalConversionStatus.Rejected, c.Status);
        Assert.Throws<InvalidOperationException>(() =>
            c.Reject(4, "مراقب", "مرة أخرى", 0, DateTimeOffset.UtcNow));
    }
}
