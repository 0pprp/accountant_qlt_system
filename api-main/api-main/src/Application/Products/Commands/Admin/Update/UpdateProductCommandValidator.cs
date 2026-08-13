using Application.Common.Extensions;

namespace Application.Products.Commands.Admin.Update;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("اسم المنتج مطلوب")
            .MaximumLength(100)
            .WithMessage("يجب ألا يتجاوز اسم المنتج 100 حرف");

        RuleFor(x => x.RemainingCount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون مخزون المنتج أكبر من الصفر.");

        RuleFor(x => x.BuyAmount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون سعر شراء المنتج أكبر من الصفر.")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.SellAmount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون سعر البیع المنتج أكبر من الصفر.")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.DailyInstallmentAmount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون القسط المنتج أكبر من الصفر.")
            .LessThanOrEqualTo(x => x.SellAmount)
            .WithMessage("يجب أن يكون القسط أقل من سعر البیع")
            .MustBeDivisibleBy1000();

        RuleFor(x => x.Description)
            .MaximumLength(400)
            .WithMessage("يجب ألا يتجاوز ملاحظات 400 حرف")
            .When(x => x.Description is not null);
    }
}