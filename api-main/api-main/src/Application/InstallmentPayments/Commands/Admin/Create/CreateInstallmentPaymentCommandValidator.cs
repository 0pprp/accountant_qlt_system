using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.InstallmentPayments.Commands.Admin.Create;

public class CreateInstallmentPaymentCommandValidator : AbstractValidator<CreateInstallmentPaymentCommand>
{
    public CreateInstallmentPaymentCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون التسديد أكبر من الصفر.")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(dateTimeProvider.Today)
            .WithMessage("لا يمكن أن يكون التاريخ المُدخل أكبر من تاريخ اليوم.");
    }
}