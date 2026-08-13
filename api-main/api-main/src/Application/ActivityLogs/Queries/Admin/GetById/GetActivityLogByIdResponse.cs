using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Folder;

namespace Application.ActivityLogs.Queries.Admin.GetById;

public record GetActivityLogByIdResponse
{
    public int Id { get; set; }
    public ActivityType ActivityType { get; set; }
    public required string Description { get; set; }
    public required string UserName { get; set; }
    public required string UserRoles { get; set; }
    public TargetEntityType TargetEntityType { get; set; }
    public int? TargetEntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DeviceType DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public int BranchId { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}