using Domain.Common;

namespace Domain.Entities.SalesFollowupAggregate;

public class SalesEmployeeAppBlock : Entity
{
    private SalesEmployeeAppBlock()
    {
    }

    public SalesEmployeeAppBlock(int employeeUserId, bool isBlocked, DateTimeOffset updatedAtUtc, int? updatedByUserId, string? updatedByName)
    {
        EmployeeUserId = employeeUserId;
        IsBlocked = isBlocked;
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = updatedByUserId;
        UpdatedByName = updatedByName;
    }

    public int EmployeeUserId { get; set; }
    public bool IsBlocked { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public string? UpdatedByName { get; set; }

    public void SetBlocked(bool isBlocked, DateTimeOffset updatedAtUtc, int? updatedByUserId, string? updatedByName)
    {
        IsBlocked = isBlocked;
        UpdatedAtUtc = updatedAtUtc;
        UpdatedByUserId = updatedByUserId;
        UpdatedByName = updatedByName;
    }
}
