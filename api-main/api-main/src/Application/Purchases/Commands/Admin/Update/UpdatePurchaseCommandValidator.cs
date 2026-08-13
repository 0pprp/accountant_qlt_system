using Application.Common.Extensions;

namespace Application.Purchases.Commands.Admin.Update;

public class UpdatePurchaseCommandValidator : AbstractValidator<UpdatePurchaseCommand>
{
    public UpdatePurchaseCommandValidator()
    {
        RuleFor(x => x.PurchaseItems)
            .NotEmpty()
            .WithMessage("منتجات مطلوب");

        RuleForEach(x => x.PurchaseItems)
            .ChildRules(x =>
            {
                x.RuleFor(dto => dto.Quantity)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون الكمية أكبر من الصفر");

                x.RuleFor(dto => dto.Amount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون السعر أكبر من الصفر")
                    .MustBeDivisibleBy1000();

                x.RuleFor(dto => dto.ProductId)
                    .GreaterThan(0)
                    .WithMessage("يجب اختيار المنتج")
                    .When(dto => dto.Id.HasValue == false);

                x.RuleFor(dto => dto.ForeignProductName)
                    .NotEmpty()
                    .WithMessage("لا يمكن أن يكون اسم المنتج فارغاً")
                    .MaximumLength(200)
                    .WithMessage("اسم المنتج طويل جداً")
                    .When(dto => dto.ForeignProductName is not null);
            });
    }
}