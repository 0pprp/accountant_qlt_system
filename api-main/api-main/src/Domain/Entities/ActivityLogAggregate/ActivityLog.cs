using Domain.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.UserAggregate;
using Domain.Folder;

namespace Domain.Entities.ActivityLogAggregate;

public class ActivityLog : AuditableEntity
{
    public ActivityLog(
        ActivityType activityType,
        string description,
        string userName,
        string userRoles,
        TargetEntityType targetEntityType,
        int? targetEntityId,
        string? ipAddress,
        string? userAgent,
        DeviceType deviceType,
        string? browser,
        string? operatingSystem,
        int branchId,
        int userId)
    {
        ActivityType = activityType;
        Description = description;
        UserName = userName;
        UserRoles = userRoles;
        TargetEntityType = targetEntityType;
        TargetEntityId = targetEntityId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        DeviceType = deviceType;
        Browser = browser;
        OperatingSystem = operatingSystem;
        BranchId = branchId;
        UserId = userId;
    }

    public ActivityType ActivityType { get; set; }
    public string Description { get; set; }
    public string UserName { get; set; }
    public string UserRoles { get; set; }
    public TargetEntityType TargetEntityType { get; set; }
    public int? TargetEntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DeviceType DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public int BranchId { get; set; }
    public int UserId { get; set; }

    public Branch? Branch { get; set; }
    public User? User { get; set; }
}
