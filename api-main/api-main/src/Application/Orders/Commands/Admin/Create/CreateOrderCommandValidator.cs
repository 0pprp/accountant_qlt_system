using Application.Common.Extensions;
using Application.Common.Utilities;
using ByteSizeLib;
using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Commands.Admin.Create;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
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

                validator.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("يجب اختيار المنتج من المستودع")
                    .When(x => x.ProductType == ProductType.Warehouse);
                
                validator.RuleFor(x => x.ProductId)
                    .Null()
                    .WithMessage("لا يمكن اختيار المنتج من المستودع.")
                    .When(x => x.ProductType == ProductType.Foreign);
                
                validator.RuleFor(x => x.ProductName)
                    .NotEmpty()
                    .WithMessage("لا يمكن أن يكون اسم المنتج فارغاً")
                    .When(x => x.ProductType == ProductType.Foreign);

                validator.RuleFor(x => x.PrepaymentAmount)
                    .LessThanOrEqualTo(x => x.SellAmount)
                    .WithMessage("لا يجوز أن يتجاوز مبلغ المقدمة سعر القسط");
                
                validator.RuleFor(x => x.DailyInstallmentAmount)
                    .LessThanOrEqualTo(x => x.SellAmount)
                    .WithMessage("لا يجوز أن يتجاوز القسط اليومي سعر القسط");
            });

        RuleForEach(x => x.Attachments)
            .NotEmpty()
            .WithMessage("لا يمكن أن تكون الصورة فارغة.")
            .Must(x => AttachmentUtility.ImageContentTypes.Contains(x.File.ContentType))
            .WithMessage("تنسيق الصورة غير صحيح.")
            .Must(x => ByteSize.FromBytes(x.File.Length).MegaBytes <= 2)
            .WithMessage("لا يمكن أن يتجاوز حجم الصورة 2 میغابایت.");
    }
}