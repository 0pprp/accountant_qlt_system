namespace Application.Users.Commands.Common.Login;

public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    /// <summary>يُضبط من AppArea فقط — فرض إيقاف تطبيق موظف المبيعات.</summary>
    public bool EnforceSalesAppBlock { get; set; }
}