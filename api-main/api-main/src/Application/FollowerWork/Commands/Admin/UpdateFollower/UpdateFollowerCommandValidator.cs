using Application.FollowerWork.Commands.Admin.ManageFollower;

namespace Application.FollowerWork.Commands.Admin.UpdateFollower;

public class UpdateFollowerCommandValidator : AbstractValidator<UpdateFollowerCommand>
{
    public UpdateFollowerCommandValidator()
    {
        RuleFor(x => x.FollowerId)
            .GreaterThan(0)
            .WithMessage("معرّف المتابع غير صالح");

        RuleFor(x => x.FullName)
            .Must(v => string.IsNullOrWhiteSpace(v) == false)
            .WithMessage("اسم المتابع مطلوب")
            .MaximumLength(FollowerAccountRules.MaxFullNameLength)
            .WithMessage("يجب ألا يتجاوز اسم المتابع 50 حرفاً");

        RuleFor(x => x.NewPassword)
            .MinimumLength(FollowerAccountRules.MinPasswordLength)
            .WithMessage("الرمز السري 8 أحرف على الأقل")
            .MaximumLength(FollowerAccountRules.MaxPasswordLength)
            .WithMessage("يجب ألا يتجاوز الرمز السري 50 حرفاً")
            .When(x => string.IsNullOrEmpty(x.NewPassword) == false);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(FollowerAccountRules.MaxPhoneLength)
            .WithMessage("يجب ألا يتجاوز رقم الهاتف 30 حرفاً")
            .When(x => string.IsNullOrWhiteSpace(x.PhoneNumber) == false);
    }
}
