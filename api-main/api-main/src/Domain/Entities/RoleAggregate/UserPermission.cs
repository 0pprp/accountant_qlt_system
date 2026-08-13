using Domain.Common;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.RoleAggregate;

public class UserPermission : BaseEntity
{
    public UserPermission(int userId, int permissionId)
    {
        UserId = userId;
        PermissionId = permissionId;
    }

    public int UserId { get; set; }
    public int PermissionId { get; set; }

    public User? User { get; set; }
    public Permission? Permission { get; set; }
}