namespace Application.Users.Commands.Admin.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون الأسم الثلاثي فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز الأسم الثلاثي 50 حرف");
        
        RuleFor(x => x.MotherName)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون أسم الأم فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز أسم الأم 50 حرف");
        
        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون رقم الهوية فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز رقم الهوية 50 حرف");
        
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون اسم المستخدم فارغين.")
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز اسم المستخدم 100 حرف");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون الرمز السري فارغين.")
            .MaximumLength(50)
            .WithMessage("يجب ألا يتجاوز الرمز السري 50 حرف")
            .MinimumLength(8)
            .WithMessage("يجب أن تتكون الرمز السري من 8 أحرف على الأقل.");

        RuleFor(x => x.BranchIds)
            .NotEmpty()
            .WithMessage("يجب تحديد فرع واحد على الأقل.");
        
        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون عنوان السكن فارغين.")
            .MaximumLength(400)
            .WithMessage("يجب ألا يتجاوز عنوان السكن 400 حرف");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون رقم الهاتف فارغين.")
            .MaximumLength(30)
            .WithMessage("يجب ألا يتجاوز رقم الهاتف 30 حرف");
    }
}