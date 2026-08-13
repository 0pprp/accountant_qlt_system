using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.Safes.Commands.Admin.CreateCashDelivery;

public class CreateCashDeliveryCommandValidator : AbstractValidator<CreateCashDeliveryCommand>
{
    public CreateCashDeliveryCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون المبلغ أكبر من الصفر")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(dateTimeProvider.Today)
            .WithMessage("لا يمكن للتاريخ أن يكون أعظم من اليوم");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("يجب ألا يتجاوز ملاحظات 500 حرف")
            .When(x => x.Description is not null);
    }
}