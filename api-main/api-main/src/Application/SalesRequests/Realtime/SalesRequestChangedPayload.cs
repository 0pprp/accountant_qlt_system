using Domain.Entities.SalesRequestAggregate;

namespace Application.SalesRequests.Realtime;

public sealed record SalesRequestChangedPayload(
    Guid EventId,
    int RequestId,
    int Revision,
    DateTimeOffset OccurredAtUtc,
    string ChangeType,
    int BranchId,
    int? TargetEmployeeId,
    int? PreviousTargetEmployeeId,
    string Audience)
{
    public const string AudienceBranch = "branch";
    public const string AudienceBranchAndEmployee = "branch+employee";

    public const string HubMethodName = "SalesRequestChanged";

    public static SalesRequestChangedPayload From(SalesRequestChangeOutbox row)
        => new(
            row.EventId,
            row.RequestId,
            row.Revision,
            row.OccurredAtUtc,
            row.ChangeType.ToString(),
            row.BranchId,
            row.TargetEmployeeId,
            row.PreviousTargetEmployeeId,
            row.TargetEmployeeId is > 0 || row.PreviousTargetEmployeeId is > 0
                ? AudienceBranchAndEmployee
                : AudienceBranch);
}
