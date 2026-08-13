using Application.Common.Extensions;

namespace Application.InstallmentPayments.Commands.App.Create;

public class CreateInstallmentPaymentCommandValidator : AbstractValidator<CreateInstallmentPaymentCommand>
{
    public CreateInstallmentPaymentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون التسديد أكبر من الصفر.")
            .MustBeDivisibleBy1000();
    }
}

