using Application.Common.Extensions;
using Application.Common.Utilities;
using ByteSizeLib;

namespace Application.Purchases.Commands.Admin.Create;

public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.PurchaseItems)
            .NotEmpty()
            .WithMessage("منتجات مطلوب");

        RuleForEach(x => x.PurchaseItems)
            .ChildRules(x =>
            {
                x.RuleFor(dto => dto.ProductId)
                    .GreaterThan(0)
                    .WithMessage("يجب اختيار المنتج");

                x.RuleFor(dto => dto.Quantity)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون الكمية أكبر من الصفر");

                x.RuleFor(dto => dto.Amount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("يجب أن يكون السعر أكبر من الصفر")
                    .MustBeDivisibleBy1000();
            });

        RuleForEach(x => x.Attachments)
            .Must(x => AttachmentUtility.ImageContentTypes.Contains(x.ContentType))
            .WithMessage("تنسيق الصورة غير صحيح.")
            .Must(x => ByteSize.FromBytes(x.Length).MegaBytes <= 2)
            .WithMessage("لا يمكن أن يتجاوز حجم الصورة 2 میغابایت.")
            .When(x => x.Attachments is not null);
    }
}