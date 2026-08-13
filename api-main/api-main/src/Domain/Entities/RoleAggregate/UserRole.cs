using Domain.Entities.UserAggregate;

namespace Domain.Entities.RoleAggregate;

public class UserRole
{
    public UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public UserRole(int roleId)
    {
        RoleId = roleId;
    }

    public int UserId { get; set; }
    public int RoleId { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}