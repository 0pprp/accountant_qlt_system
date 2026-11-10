using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Realtime;

public sealed record SalesRequestChangeMessage(
    int RequestId,
    int Revision,
    SalesRequestChangeType ChangeType,
    int BranchId,
    int? TargetEmployeeId,
    int? PreviousTargetEmployeeId,
    DateTimeOffset OccurredAtUtc);

public interface ISalesRequestChangeNotifier
{
    void Enqueue(SalesRequestChangeMessage message);
}
