using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.ActivityLogs.Queries.Admin.GetPaginated;

public record GetPaginatedActivityLogsResponse
{
    public int Id { get; set; }
    public ActivityType ActivityType { get; set; }
    public required string Description { get; set; }
    public required string UserName { get; set; }
    public required string UserRoles { get; set; }
    public TargetEntityType TargetEntityType { get; set; }
    public int? TargetEntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
