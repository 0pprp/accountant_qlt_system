using Domain.Common;

namespace Domain.Entities.UserAggregate;

public class UserLogin : Entity
{
    public UserLogin(string refreshToken, DateTimeOffset refreshTokenExpiresAt, int? userId)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        UserId = userId;
    }

    public string RefreshToken { get; set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public int? UserId { get; set; }

    public User? User { get; set; }
}