using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.Orders.Commands.Admin.Update;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleForEach(x => x.OrderItems)
            .ChildRules(validator =>
            {
                validator.RuleFor(x => x.BuyAmount)
                    .GreaterThan(0)
                    .WithMessage("يجب أن يكون سعر الشراء أكبر من الصفر.")
                    .MustBeDivisibleBy1000();

                validator.RuleFor(x => x.SellAmount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون سعر القسط أكبر من الصفر.")
                    .MustBeDivisibleBy1000();

                validator.RuleFor(x => x.PrepaymentAmount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون المقدمة أكبر من الصفر.")
                    .MustBeDivisibleBy1000();

                validator.RuleFor(x => x.DailyInstallmentAmount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون القسط اليومي أكبر من الصفر.")
                    .MustBeDivisibleBy1000();
            });

        RuleFor(x => x.CreationAddress)
            .MaximumLength(400)
            .WithMessage("يجب ألا يتجاوز عنوان البيع 400 حرف")
            .When(x => x.CreationAddress is not null);

        RuleFor(x => x.SaleDate)
            .LessThanOrEqualTo(dateTimeProvider.Today)
            .WithMessage("يجب ألا يتجاوز تاريخ البيع یوم الجاری")
            .When(x => x.SaleDate is not null);
    }
}
