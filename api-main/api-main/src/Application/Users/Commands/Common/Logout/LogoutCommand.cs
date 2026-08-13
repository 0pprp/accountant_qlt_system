namespace Application.Users.Commands.Common.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;