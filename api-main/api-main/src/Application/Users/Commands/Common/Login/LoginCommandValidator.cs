namespace Application.Users.Commands.Common.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("اسم المستخدم مطلوب")
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز اسم المستخدم 100 حرف");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("كلمة المرور مطلوبة");
    }
}