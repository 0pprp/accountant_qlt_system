using Application.Common.Extensions;
using Application.Common.Interfaces;

namespace Application.Orders.Commands.App.Update;

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

        RuleFor(x => x.Location)
            .ChildRules(validator =>
            {
                validator.RuleFor(x => x!.Latitude)
                    .Must(double.IsFinite)
                    .WithMessage("الموقع الجغرافي المحدد غير صحيح.")
                    .InclusiveBetween(-90, 90)
                    .WithMessage("الموقع الجغرافي المحدد غير صحيح.");

                validator.RuleFor(x => x!.Longitude)
                    .Must(double.IsFinite)
                    .WithMessage("الموقع الجغرافي المحدد غير صحيح.")
                    .InclusiveBetween(-180, 180)
                    .WithMessage("الموقع الجغرافي المحدد غير صحيح.");
            })
            .When(x => x.Location is not null);

        RuleFor(x => x.Location!)
            .Must(location => location.Latitude != 0 || location.Longitude != 0)
            .WithMessage("الموقع الجغرافي المحدد غير صحيح.")
            .When(x => x.Location is not null);

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