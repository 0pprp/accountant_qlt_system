using Domain.Common;
using Domain.Entities.LegalConversionAggregate.Enums;

namespace Domain.Entities.LegalConversionAggregate;

/// <summary>
/// طلب تحويل زبون إلى قانونية — يضبط Customer.IsLegal عند الاعتماد فقط.
/// منفصل عن LegalFollowupCase لطلبات البيع.
/// </summary>
public class LegalConversionRequest : Entity
{
    private LegalConversionRequest()
    {
    }

    public LegalConversionRequest(
        int customerId,
        int branchId,
        int requesterUserId,
        string? requesterName,
        string note,
        DateTimeOffset createdAtUtc)
    {
        CustomerId = customerId;
        BranchId = branchId;
        RequesterUserId = requesterUserId;
        RequesterName = requesterName;
        RequestNote = note;
        Status = LegalConversionStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        ConcurrencyStamp = 0;
    }

    public int CustomerId { get; private set; }
    public int BranchId { get; private set; }
    public int RequesterUserId { get; private set; }
    public string? RequesterName { get; private set; }
    public string RequestNote { get; private set; } = "";
    public LegalConversionStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public int? DecidedByUserId { get; private set; }
    public string? DecidedByName { get; private set; }
    public string? DecisionNote { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public void Approve(int actorId, string? actorName, string? note, int expectedVersion, DateTimeOffset at)
    {
        EnsurePending(expectedVersion);
        Status = LegalConversionStatus.Approved;
        DecidedAtUtc = at;
        DecidedByUserId = actorId;
        DecidedByName = actorName;
        DecisionNote = note;
        ConcurrencyStamp++;
    }

    public void Reject(int actorId, string? actorName, string reason, int expectedVersion, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("سبب الرفض إلزامي.");
        EnsurePending(expectedVersion);
        Status = LegalConversionStatus.Rejected;
        DecidedAtUtc = at;
        DecidedByUserId = actorId;
        DecidedByName = actorName;
        DecisionNote = reason.Trim();
        ConcurrencyStamp++;
    }

    private void EnsurePending(int expectedVersion)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        if (Status != LegalConversionStatus.Pending)
            throw new InvalidOperationException("الطلب ليس قيد المراجعة.");
    }
}
