namespace Application.Users.Commands.Admin.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("لا يمكن أن يكون الأسم الثلاثي فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز الأسم الثلاثي 50 حرف");

        RuleFor(x => x.MotherName)
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز أسم الأم 50 حرف")
            .When(x => !string.IsNullOrWhiteSpace(x.MotherName));

        RuleFor(x => x.NationalCode)
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز رقم الهوية 50 حرف")
            .When(x => !string.IsNullOrWhiteSpace(x.NationalCode));

        RuleFor(x => x.Username)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("لا يمكن أن يكون اسم المستخدم فارغين.")
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز اسم المستخدم 100 حرف");

        RuleFor(x => x.Password)
            .Must(v => v is not null)
            .WithMessage("لا يمكن أن يكون الرمز السري فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز الرمز السري 50 حرف")
            .MinimumLength(8)
            .WithMessage("يجب أن تتكون الرمز السري من 8 أحرف على الأقل.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد المنصب الوظيفي.");

        RuleFor(x => x.BranchIds)
            .NotEmpty()
            .WithMessage("يجب تحديد فرع واحد على الأقل.");

        RuleFor(x => x.Address)
            .MaximumLength(400)
            .WithMessage("يجب ألا يتجاوز عنوان السكن 400 حرف")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .WithMessage("يجب ألا يتجاوز رقم الهاتف 30 حرف")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
