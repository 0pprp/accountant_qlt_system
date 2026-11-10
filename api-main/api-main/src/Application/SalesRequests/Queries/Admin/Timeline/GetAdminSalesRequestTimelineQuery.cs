using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Queries.Admin.Timeline;

public record GetAdminSalesRequestTimelineQuery(int Id)
    : IRequest<Result<List<AdminSalesRequestTimelineItem>>>;

public record AdminSalesRequestTimelineItem
{
    public int Id { get; init; }
    public SalesRequestTimelineEventType EventType { get; init; }
    public required string EventLabel { get; init; }
    public int ActorUserId { get; init; }
    public string? ActorName { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public SalesRequestStatus? PreviousStatus { get; init; }
    public string? PreviousStatusLabel { get; init; }
    public SalesRequestStatus? NewStatus { get; init; }
    public string? NewStatusLabel { get; init; }
    public int? PreviousEmployeeId { get; init; }
    public int? NewEmployeeId { get; init; }
    public string? Reason { get; init; }
}
