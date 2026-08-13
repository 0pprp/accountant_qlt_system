using Application.Common.Interfaces;

namespace Application.Orders.Commands.App.SetSellerInfo;

public class SetOrderSellerInfoCommandValidator : AbstractValidator<SetOrderSellerInfoCommand>
{
    public SetOrderSellerInfoCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.CreationAddress)
            .NotEmpty()
            .WithMessage("عنوان البيع مطلوب")
            .MaximumLength(400)
            .WithMessage("يجب ألا يتجاوز عنوان البيع 400 حرف");

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

        RuleFor(x => x.Location)
            .Must(location => location!.Latitude != 0 || location.Longitude != 0)
            .WithMessage("الموقع الجغرافي المحدد غير صحيح.")
            .When(x => x.Location is not null);

        RuleFor(x => x.SaleDate)
            .NotEmpty()
            .WithMessage("تسجيل تاريخ الطلب إلزامي")
            .LessThanOrEqualTo(dateTimeProvider.Today)
            .WithMessage("يجب ألا يتجاوز تاريخ البيع یوم الجاری");
    }
}