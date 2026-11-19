using Application.FollowerWork.Commands.Admin.ManageFollower;

namespace Application.FollowerWork.Commands.Admin.CreateFollower;

public class CreateFollowerCommandValidator : AbstractValidator<CreateFollowerCommand>
{
    public CreateFollowerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .Must(v => string.IsNullOrWhiteSpace(v) == false)
            .WithMessage("اسم المتابع مطلوب")
            .MaximumLength(FollowerAccountRules.MaxFullNameLength)
            .WithMessage("يجب ألا يتجاوز اسم المتابع 50 حرفاً");

        RuleFor(x => x.Username)
            .Must(v => string.IsNullOrWhiteSpace(v) == false && v.Trim().Length >= FollowerAccountRules.MinUsernameLength)
            .WithMessage("اسم المستخدم 3 أحرف على الأقل")
            .MaximumLength(FollowerAccountRules.MaxUsernameLength)
            .WithMessage("يجب ألا يتجاوز اسم المستخدم 100 حرف");

        RuleFor(x => x.Password)
            .MinimumLength(FollowerAccountRules.MinPasswordLength)
            .WithMessage("الرمز السري 8 أحرف على الأقل")
            .MaximumLength(FollowerAccountRules.MaxPasswordLength)
            .WithMessage("يجب ألا يتجاوز الرمز السري 50 حرفاً");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(FollowerAccountRules.MaxPhoneLength)
            .WithMessage("يجب ألا يتجاوز رقم الهاتف 30 حرفاً")
            .When(x => string.IsNullOrWhiteSpace(x.PhoneNumber) == false);

        RuleFor(x => x.BranchIds)
            .NotEmpty()
            .WithMessage("حدد فرعاً واحداً على الأقل");
    }
}
