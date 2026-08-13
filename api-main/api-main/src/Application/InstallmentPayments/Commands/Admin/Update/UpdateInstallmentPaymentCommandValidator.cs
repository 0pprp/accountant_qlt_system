using Application.Common.Extensions;

namespace Application.InstallmentPayments.Commands.Admin.Update;

public class UpdateInstallmentPaymentCommandValidator : AbstractValidator<UpdateInstallmentPaymentCommand>
{
    public UpdateInstallmentPaymentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون التسديد أكبر من الصفر.")
            .MustBeDivisibleBy1000();
    }
}