namespace Application.Users.Commands.Common.Login;

public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}