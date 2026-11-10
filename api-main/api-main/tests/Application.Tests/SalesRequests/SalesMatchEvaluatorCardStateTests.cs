using Application.SalesRequests.Common;
using Domain.Services.SalesMatching;

namespace Application.Tests.SalesRequests;

public class SalesMatchEvaluatorCardStateTests
{
    [Fact]
    public void Insufficient_name_is_not_zero_results()
    {
        var card = SalesRequestMatchEvaluator.EvaluateCard(
            SalesMatchCriteria.TripleName,
            "علي فقط",
            null,
            [new SalesMatchCandidate { Id = 1, Name = "علي حسن كاظم", Phone = null, BranchId = 1 }]);

        Assert.Equal("insufficientData", card.State);
        Assert.Null(card.Count);
    }

    [Fact]
    public void Insufficient_phone_is_not_zero_results()
    {
        var card = SalesRequestMatchEvaluator.EvaluateCard(
            SalesMatchCriteria.Phone,
            "علي حسن كاظم",
            "12",
            [new SalesMatchCandidate { Id = 1, Name = "س", Phone = "07801112233", BranchId = 1 }]);

        Assert.Equal("insufficientData", card.State);
        Assert.Null(card.Count);
    }

    [Fact]
    public void Same_customer_can_hit_multiple_criteria_without_unique_merge()
    {
        var candidates = new List<SalesMatchCandidate>
        {
            new() { Id = 9, Name = "حسين أحمد محمد", Phone = "07804924373", BranchId = 1 }
        };

        var triple = SalesRequestMatchEvaluator.EvaluateCard(
            SalesMatchCriteria.TripleName, "حسين أحمد محمد", "07804924373", candidates);
        var phone = SalesRequestMatchEvaluator.EvaluateCard(
            SalesMatchCriteria.Phone, "حسين أحمد محمد", "07804924373", candidates);

        Assert.Equal(1, triple.Count);
        Assert.Equal(1, phone.Count);
        // لا تُجمَع كزبون فريد واحد عبر البطاقات
        Assert.Equal(2, (triple.Count ?? 0) + (phone.Count ?? 0));
    }

    [Fact]
    public void Unknown_criterion_is_failed_not_zero()
    {
        var card = SalesRequestMatchEvaluator.EvaluateCard(
            "unknown", "أ ب ج", "07801112233", []);
        Assert.Equal("failed", card.State);
        Assert.Null(card.Count);
    }
}
