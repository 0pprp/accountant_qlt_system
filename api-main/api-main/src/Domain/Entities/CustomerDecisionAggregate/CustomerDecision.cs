using Domain.Common;
using Domain.Entities.CustomerDecisionAggregate.Enums;

namespace Domain.Entities.CustomerDecisionAggregate;

/// <summary>
/// سجل قرارات أعمال الفرع — مقابل CustomerWeekDecisions.
/// ليس ActivityLog وليس LegalFollowupCase.
/// </summary>
public class CustomerDecision : Entity
{
    private CustomerDecision()
    {
    }

    public CustomerDecision(
        int customerId,
        int branchId,
        int actorUserId,
        string? actorName,
        CustomerDecisionType decisionType,
        string note,
        double? weekPaid,
        double? amountTotalSales,
        double? paidPercent,
        DateTimeOffset? snoozeUntil,
        int? relatedWaiverRequestId,
        int? relatedLegalConversionId,
        DateTimeOffset createdAtUtc)
    {
        CustomerId = customerId;
        BranchId = branchId;
        ActorUserId = actorUserId;
        ActorName = actorName;
        DecisionType = decisionType;
        Note = note;
        WeekPaid = weekPaid;
        AmountTotalSales = amountTotalSales;
        PaidPercent = paidPercent;
        SnoozeUntil = snoozeUntil;
        RelatedWaiverRequestId = relatedWaiverRequestId;
        RelatedLegalConversionId = relatedLegalConversionId;
        CreatedAtUtc = createdAtUtc;
    }

    public int CustomerId { get; private set; }
    public int BranchId { get; private set; }
    public int ActorUserId { get; private set; }
    public string? ActorName { get; private set; }
    public CustomerDecisionType DecisionType { get; private set; }
    public string Note { get; private set; } = "";
    public double? WeekPaid { get; private set; }
    public double? AmountTotalSales { get; private set; }
    public double? PaidPercent { get; private set; }
    public DateTimeOffset? SnoozeUntil { get; private set; }
    public int? RelatedWaiverRequestId { get; private set; }
    public int? RelatedLegalConversionId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
